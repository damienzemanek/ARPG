using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static System.Single;


public enum NoBlends { }

[Serializable]
public struct AnimState<TAnimEnum>
    where TAnimEnum : Enum
{
    public string name;
    public int layer;
    public TAnimEnum animEnum;
    [ReadOnly] public int hash;
    public AnimState(string name, TAnimEnum animEnum, int layer = 0)
    {
        this.name = name;
        this.animEnum = animEnum;
        this.layer = layer;
        hash = Animator.StringToHash(name);
    }
    public void CalculateHash() => hash = Animator.StringToHash(name);
}
    
[Serializable]
public struct BlendTreeVariable<TAnimBlendEnum>
    where TAnimBlendEnum : Enum
{
    public string name;
    public TAnimBlendEnum blendEnum;
    [ReadOnly] public int hash;
    public BlendTreeVariable(string name, TAnimBlendEnum blendEnum)
    {
        this.name = name;
        this.blendEnum = blendEnum;
        hash = Animator.StringToHash(name);
    }
    public void CalculateHash() => hash = Animator.StringToHash(name);
}

[LabelWidth(125)]
[InlineProperty]
[Serializable]
public class AnimHandle<TAnimEnum, TAnimBlendEnum>
    where TAnimEnum : Enum
    where TAnimBlendEnum : Enum
{
    [FormerlySerializedAs("states")] [LabelWidth(150)] public AnimState<TAnimEnum>[] States;
    [FormerlySerializedAs("blendTreeVariables")] [LabelWidth(150)] public BlendTreeVariable<TAnimBlendEnum>[] BlendTreeVariables;

    Dictionary<TAnimEnum, (int hash, int layer)> states;
    Dictionary<TAnimBlendEnum, int> blendTreeVariables;
    
    [Button, PropertyOrder(-1)]
    public void RecalculateHashes()
    {
        if (States != null)
            for (int i = 0; i < States.Length; i++)
            {
                var s = States[i];
                s.CalculateHash();
                States[i] = s;
            }

        if (BlendTreeVariables != null)
            for (int i = 0; i < BlendTreeVariables.Length; i++)
            {
                var s = BlendTreeVariables[i];
                s.CalculateHash();
                BlendTreeVariables[i] = s;
            }

    }

    public int GetLayer(TAnimEnum animEnum)
    {
        return GetAnimInfo(animEnum).layer;
    }
    
    void Initialize()
    {
        states = new Dictionary<TAnimEnum, (int hash, int layer)>();
        blendTreeVariables = new Dictionary<TAnimBlendEnum, int>();
        foreach (var state in States) states.Add(state.animEnum, (state.hash, state.layer));
        foreach (var blendTreeVariable in BlendTreeVariables) blendTreeVariables.Add(blendTreeVariable.blendEnum, blendTreeVariable.hash);
    }

    (int hash, int layer) GetAnimInfo(TAnimEnum animEnum)     
    {
        if (states == null) Initialize(); // Add this line
        if (states.TryGetValue(animEnum, out var info)) return info;
        return (-1, -1);
    }

    public void UpdateAnimBlendFloat(Animator animator, TAnimBlendEnum blendEnum, float value)
    {
        if (blendTreeVariables == null) Initialize();
        if (animator == null) return;
        if (blendTreeVariables == null) return;
        
        if (!blendTreeVariables.TryGetValue(blendEnum, out var hash))
        {
            Debug.LogWarning($"AnimHandle: No hash mapped for blend enum {blendEnum}.");
            return;
        }
        
        animator.SetFloat(hash, value);
    }
    
    public bool Play(
        Animator animator, 
        TAnimEnum animEnum, 
        float normalizedTime = NegativeInfinity)
    {
        if (states == null) Initialize();
        if (animator == null) { Debug.LogWarning("Animator Null"); return false;}
        if (States == null) { Debug.LogError("States Null"); return false;}
        if(states == null) { Debug.LogError("States Dictionary Null"); return false;}
        if(!states.TryGetValue(animEnum, out var info))
        {
            Debug.LogError($"AnimHandle: No state mapped for enum {animEnum}");
            return false;
        }
        if (info.layer < 0 || info.layer >= animator.layerCount)  { Debug.LogError("Layer Out of Index Range"); return false;}
        if(info.hash == 0) { Debug.LogError($"AnimHandle: Hash for enum {animEnum} is 0, Please Recalculate Hashes"); return false;}
        animator.Play(info.hash, info.layer, normalizedTime);
        return true;
    }
    
    public bool CrossFade( Animator animator, TAnimEnum animEnum, float normalizedTime = NegativeInfinity)
    {
        if (states == null) Initialize();
        if (animator == null) { Debug.LogWarning("Animator Null"); return false;}
        if (States == null) { Debug.LogError("States Null"); return false;}
        if(states == null) { Debug.LogError("States Dictionary Null"); return false;}
        if(!states.TryGetValue(animEnum, out var info))
        {
            Debug.LogError($"AnimHandle: No state mapped for enum {animEnum}");
            return false;
        }
        if (info.layer < 0 || info.layer >= animator.layerCount)  { Debug.LogError("Layer Out of Index Range"); return false;}
        if(info.hash == 0) { Debug.LogError($"AnimHandle: Hash for enum {animEnum} is 0, Please Recalculate Hashes"); return false;}
        animator.CrossFade(info.hash, 0.2f, info.layer, normalizedTime);
        return true;
    }
    
    public bool PlayThenOnEnd(Animator animator, TAnimEnum animEnum, Action onEnd, bool crossFade, float normalizedTime = NegativeInfinity)
    {
        if (crossFade)
        {
            if (!Play(animator, animEnum, normalizedTime)) return false;
        }
        else if (!CrossFade(animator, animEnum, normalizedTime)) return false;

        

        PlayThenOnEndAsync(animator, animEnum, onEnd).Forget("AnimHandle: PlayThenOnEnd");
        return true;

        async Task PlayThenOnEndAsync(
            Animator _animator,
            TAnimEnum _animEnum,
            Action _OnEnd)
        {
            var info = GetAnimInfo(_animEnum);
            if (info.hash == -1 || _animator == null || info.layer < 0 || info.layer >= _animator.layerCount)
                return;

            const float timeoutSeconds = 1.5f;
            float start = Time.time;

            // 1) Wait until the requested state is actually active
            while (true)
            {
                if (_animator == null) return;

                var s = _animator.GetCurrentAnimatorStateInfo(info.layer);
                bool inTarget = s.shortNameHash == info.hash || s.fullPathHash == info.hash;

                if (!_animator.IsInTransition(info.layer) && inTarget)
                    break;

                if (Time.time - start > timeoutSeconds)
                {
                    Debug.LogWarning($"AnimHandle: Timeout waiting to ENTER {_animEnum} on layer {info.layer}.");
                    _OnEnd?.Invoke();
                    return;
                }

                await Awaitable.NextFrameAsync();
            }

            // 2) Wait until target state finishes (for non-loop clips)
            while (true)
            {
                if (_animator == null) return;

                var s = _animator.GetCurrentAnimatorStateInfo(info.layer);
                bool stillTarget = s.shortNameHash == info.hash || s.fullPathHash == info.hash;

                // If we already left target state, treat as ended
                if (!stillTarget)
                    break;

                // End condition for non-looping state
                if (!s.loop && !_animator.IsInTransition(info.layer) && s.normalizedTime >= 1f)
                    break;

                if (Time.time - start > timeoutSeconds)
                {
                    Debug.LogWarning($"AnimHandle: Timeout waiting to FINISH {_animEnum} on layer {info.layer}.");
                    return;
                }

                await Awaitable.NextFrameAsync();
            }

            _OnEnd?.Invoke();
        }
    }
    
    public bool PlayWeightSet(
        Animator animator,
        TAnimEnum animEnum,
        float initialWeight,
        float endWeight,
        float normalizedTime = NegativeInfinity)
    {
        Animator lclAnimator = animator;
        var info = GetAnimInfo(animEnum);
        if (info.layer < 0 || info.layer >= animator.layerCount) return false; 
        animator.SetLayerWeight(info.layer, initialWeight);
        if(!PlayThenOnEnd(animator, animEnum, SetWeightTo1, false, normalizedTime)) return false;
        return true;
        void SetWeightTo1() => lclAnimator.SetLayerWeight(info.layer, endWeight);
    }
    
    public bool PlayWeightSet(
        Animator animator,
        TAnimEnum animEnum,
        float weight,
        float normalizedTime = NegativeInfinity)
    {
        Animator lclAnimator = animator;
        var info = GetAnimInfo(animEnum);
        animator.SetLayerWeight(info.layer, weight);
        if(!Mathf.Approximately(animator.GetLayerWeight(info.layer), weight))      
            animator.SetLayerWeight(info.layer, weight);
        if(!Play(animator, animEnum, normalizedTime)) return false;
        return true;
    }
    
    public bool PlayWeightSetOnEnd(
        Animator animator,
        TAnimEnum animEnum,
        float initialWeight,
        float endWeight,
        Action onEnd,
        float normalizedTime = NegativeInfinity)
    {
        Animator lclAnimator = animator;
        Action lclOnEnd = onEnd;
        var info = GetAnimInfo(animEnum);
        animator.SetLayerWeight(info.layer, initialWeight);
        if(!PlayThenOnEnd(animator, animEnum, SetWeightTo1, false, normalizedTime)) return false;
        return true;
        void SetWeightTo1() {lclAnimator.SetLayerWeight(info.layer, endWeight); lclOnEnd?.Invoke();}
    }
}