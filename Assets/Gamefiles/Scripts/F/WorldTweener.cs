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
    [ReadOnly] public Vector3 origPos;

    [Serializable]
    public class TweenPose
    {
        public Tween pos;
        public Tween rot;
        public Tween scale;
        public float duration;
        public float postStayDuration = 0;
        [FoldoutGroup("OnComplete")] public UnityEvent onTweenCompleteCb;
    }
    
    [Serializable]
    public class Tween
    {
        public bool x, y, z;
        [FormerlySerializedAs("finalX")] [ShowIf("x")] public float tweenX;
        [FormerlySerializedAs("finalY")] [ShowIf("y")] public float tweenY;
        [FormerlySerializedAs("finalZ")] [ShowIf("z")] public float tweenZ;
        public Ease toEase;
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
            seq.Append(target.DOLocalMove(finalPos, duration).SetEase(t.toEase));
        }
        //seq.Append(transform.DOMove(origPos, 1f));
        seq.Play();
    }


    public Sequence GetTweenPositionSequence(Sequence seq, TweenPose tweenPose)
    {
        seq ??= DOTween.Sequence();
        foreach (var t in sequence)
        {
            Vector3 finalPos = new Vector3(
                t.x ? t.tweenX : target.transform.localPosition.x, 
                t.y ? t.tweenY : target.transform.localPosition.y,
                t.z ? t.tweenZ : target.transform.localPosition.z);
            seq.Append(target.DOLocalMove(finalPos, tweenPose.duration).SetEase(t.toEase));
        }
        return seq;
    }
    public Sequence GetTweenRotationSequence(Sequence seq, TweenPose tweenPose)
    {
        seq ??= DOTween.Sequence();
        foreach (var t in sequence)
        {
            Vector3 finalRot = new Vector3(
                t.x ? t.tweenX : target.transform.localRotation.x, 
                t.y ? t.tweenY : target.transform.localRotation.y,
                t.z ? t.tweenZ : target.transform.localRotation.z);
            seq.Append(target.DOLocalRotate(finalRot, tweenPose.duration).SetEase(t.toEase));
        }
        return seq;
    }
    public Sequence GetTweenScaleSequence(Sequence seq, TweenPose tweenPose)
    {
        seq ??= DOTween.Sequence();
        foreach (var t in sequence)
        {
            Vector3 finalScale = new Vector3(
                t.x ? t.tweenX : target.transform.localScale.x, 
                t.y ? t.tweenY : target.transform.localScale.y,
                t.z ? t.tweenZ : target.transform.localScale.z);
            seq.Append(target.DOScale(finalScale, tweenPose.duration).SetEase(t.toEase));
        }
        return seq;
    }
    
    public void TweenPosesLinearProgression(List<TweenPose> tweenPoses, Action onSequenceComplete = null)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var tp in tweenPoses)
        {
            GetTweenPositionSequence(seq, tp);
            GetTweenRotationSequence(seq, tp);
            GetTweenScaleSequence(seq, tp);
            seq.AppendCallback(() => onSequenceComplete?.Invoke());
            seq.AppendInterval(tp.postStayDuration);
        }
        seq.Play();
    }
    
    
    
    
}
