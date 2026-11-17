using System;
using System.Collections.Generic;
using DG.Tweening;
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
    public void PlaySequence()
    {
        Sequence seq = DOTween.Sequence();
        foreach (var t in sequence)
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
}
