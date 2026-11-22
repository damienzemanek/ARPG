using System;
using System.Collections.Generic;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;



[ExecuteAlways]
public class WorldTweener : MonoBehaviour
{
    public Transform target;
    public SpriteRenderer targSPR;
    [ReadOnly] public Vector3 origPos;

    [Serializable]
    public class TweenPose
    {
        [HorizontalGroup("Bool")] [GUIColor(1f, 0f, 0f)] [LabelWidth(15)] public bool _pos;
        [HorizontalGroup("Bool")] [GUIColor(0f, 1f, 0f)] [LabelWidth(15)] public bool _rot;
        [HorizontalGroup("Bool")] [GUIColor(0f, 1f, 1f)] [LabelWidth(15)] public bool _scale;
        [HorizontalGroup("Vals")] [GUIColor(1f, 0f, 0f)] [LabelWidth(15)] [HideLabel] public Tween pos;
        [HorizontalGroup("Vals")] [GUIColor(0f, 1f, 0f)] [LabelWidth(15)] [HideLabel] public Tween rot;
        [HorizontalGroup("Vals")] [GUIColor(0f, 1f, 1f)] [LabelWidth(15)] [HideLabel] public Tween scale;
        public float opacity = 1f;
        public float duration;
        public Ease toEase;
        [FoldoutGroup("OnComplete")] public UnityEvent onTweenCompleteCb;
    }
    
    [Serializable]
    public class Tween
    {
        [HorizontalGroup("Bool")] [LabelWidth(10)] public bool x;
        [HorizontalGroup("Bool")] [LabelWidth(10)] public bool y;
        [HorizontalGroup("Bool")] [LabelWidth(10)] public bool z;
        [HorizontalGroup("Loc")] [HideLabel] [ShowIf("x")] public float tweenX;
        [HorizontalGroup("Loc")] [HideLabel] [ShowIf("y")] public float tweenY;
        [HorizontalGroup("Loc")] [HideLabel] [ShowIf("z")] public float tweenZ;

        public void SetTweenXYZ(Vector3 v)
        {
            tweenX = v.x;
            tweenY = v.y;
            tweenZ = v.z;
        }

        public Vector3 AsVector3() => new(tweenX, tweenY, tweenZ);

    }
    
    public List<Tween> sequence;

    void Awake()
    {
        origPos = transform.position;
    }

    [Button]
    public void PlaySequence(float duration)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var t in sequence)
        {
            Vector3 finalPos = new Vector3(
                t.x ? t.tweenX : target.transform.localPosition.x, 
                t.y ? t.tweenY : target.transform.localPosition.y,
                t.z ? t.tweenZ : target.transform.localPosition.z);
            seq.Append(target.DOLocalMove(finalPos, duration).SetEase(Ease.OutSine));
        }
        //seq.Append(transform.DOMove(origPos, 1f));
        seq.Play();
    }


    public Sequence GetTweenPositionSequence(Sequence seq, TweenPose tweenPose)
    {
        seq ??= DOTween.Sequence();

        Vector3 current = target.localPosition;
        Vector3 finalPos = new Vector3(
            tweenPose.pos.x ? tweenPose.pos.tweenX : current.x,
            tweenPose.pos.y ? tweenPose.pos.tweenY : current.y,
            tweenPose.pos.z ? tweenPose.pos.tweenZ : current.z
        );
        seq.Join(target.DOLocalMove(finalPos, tweenPose.duration)
            .SetEase(tweenPose.toEase));
        return seq;
    }
    public Sequence GetTweenRotationSequence(Sequence seq, TweenPose tweenPose)
    {
        seq ??= DOTween.Sequence();
        Vector3 current = target.localEulerAngles;
        Vector3 finalPos = new Vector3(
            tweenPose.rot.x ? tweenPose.rot.tweenX : current.x,
            tweenPose.rot.y ? tweenPose.rot.tweenY : current.y,
            tweenPose.rot.z ? tweenPose.rot.tweenZ : current.z
        );

        seq.Join(target.DOLocalRotate(finalPos, tweenPose.duration)
            .SetEase(tweenPose.toEase));
        return seq;
    }
    public Sequence GetTweenScaleSequence(Sequence seq, TweenPose tweenPose)
    {
        seq ??= DOTween.Sequence();

        Vector3 current = target.localScale;
        Vector3 finalPos = new Vector3(
            tweenPose.scale.x ? tweenPose.scale.tweenX : current.x,
            tweenPose.scale.y ? tweenPose.scale.tweenY : current.y,
            tweenPose.scale.z ? tweenPose.scale.tweenZ : current.z
        );
        seq.Join(target.DOScale(finalPos, tweenPose.duration)
            .SetEase(tweenPose.toEase));
        return seq;
    }
    
    public void TweenPosesLinearProgression(List<TweenPose> tweenPoses,
        float postStayDuration, 
        float finalFadeDuration,
        Action onSequenceComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var tp in tweenPoses)
        {
            Sequence posSeq = DOTween.Sequence();

            if(tp._pos) GetTweenPositionSequence(posSeq, tp);
            if(tp._rot) GetTweenRotationSequence(posSeq, tp);
            if(tp._scale) GetTweenScaleSequence(posSeq, tp);
            
            posSeq.Join(targSPR.DOFade(tp.opacity, tp.duration)
                .SetEase(tp.toEase));
            
            seq.Append(posSeq);
        }

        seq.AppendInterval(postStayDuration);
        seq.Append(targSPR.DOFade(0f, finalFadeDuration));
        seq.OnComplete(() => onSequenceComplete?.Invoke());
        Debug.Log("Playing Sequence");
        seq.Play();
    }
    
    
    
    
}
