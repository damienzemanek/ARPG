using System;
using System.Collections.Generic;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[ExecuteAlways]
public class WorldTweener : MonoBehaviour
{
    public Transform target;
    public Vector3 origPos;
    
    [Serializable]
    public class Tween
    {
        public bool x, y, z;
        [ShowIf("x")] public float finalX;
        [ShowIf("y")] public float finalY;
        [ShowIf("z")] public float finalZ;
        public Ease toEase;
        public float duration;
        public UnityEvent onTweenCompleteCb;
    }
    
    public List<Tween> sequence;

    void Awake()
    {
        origPos = transform.position;
    }
    
    [Button]
    public void PlaySequence() => TweenGivenTweens(sequence.ToArray());
    public void TweenGivenTweens(params Tween[] tweens)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var t in tweens)
        {
            Vector3 finalPos = new Vector3(
                t.x ? t.finalX : target.transform.localPosition.x, 
                t.y ? t.finalY : target.transform.localPosition.y,
                t.z ? t.finalZ : target.transform.localPosition.z);
            
            seq.Append(target.DOLocalMove(finalPos, t.duration)
                .SetEase(t.toEase)
                .OnComplete(() => t.onTweenCompleteCb?.Invoke()));
        }
        //seq.Append(transform.DOMove(origPos, 1f));
        seq.Play();
    }

    public void TweenToOffsetTransform(params Tween[] offsetTween)
    {
        Sequence seq = DOTween.Sequence();

        Vector3 transientPosition = transform.position;
        
        foreach (var t in offsetTween)
        {
            Vector3 offsetPos = new Vector3(
                t.x ? transientPosition.x += t.finalX : target.transform.localPosition.x,
                t.y ? transientPosition.y += t.finalY : target.transform.localPosition.y,
                t.z ? transientPosition.z += t.finalZ : target.transform.localPosition.z);
            
            seq.Append(target.DOLocalMove(offsetPos, t.duration))
                .SetEase(t.toEase)
                .OnComplete(() => t.onTweenCompleteCb?.Invoke());
        }
        seq.Play();
    }
    
    
    
}
