using System.Collections;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActionUserViewer : MonoBehaviour
{
    [Required] public BattlemodePlayerInstance battlemodePlayer;
    public Transform leftLoc;
    public Transform rightLoc;

    public float stay = 1f;
    public float tweenDuration = 0.1f;

    public IEnumerator C_UseActionUserViewer(
        GameObject actor, Vector3 actorFinishLoc,
        GameObject target, Vector3 targetFinishLoc,
        bool leftIsActor)
    {
        if (actor == null || target == null)
        {
            Debug.LogError("No actor or target given to ActionUserViewer.");
            yield break;
        }
        
        Sequence sequence = DOTween.Sequence();

        if (actor == target)
        {
            sequence
                .Append(actor.transform.DOMove(leftLoc.position, tweenDuration))
                .Join(actor.transform.DORotate(battlemodePlayer.transform.rotation.eulerAngles, tweenDuration))
                .AppendInterval(stay)
                .JoinCallback(() => LeftActsNoTarget(actor.transform))
                .Append(actor.transform.DOMove(actorFinishLoc, tweenDuration))
                .Join(actor.transform.DORotate(Quaternion.identity.eulerAngles, tweenDuration))
                .JoinCallback(() => DefaultAll(actor.transform, null));
        }
        else
        {
            sequence
                // Move both into position together
                .Append(actor.transform.DOMove(leftLoc.position, tweenDuration))
                .Join(actor.transform.DORotate(battlemodePlayer.transform.rotation.eulerAngles, tweenDuration))
                .Join(target.transform.DOMove(rightLoc.position, tweenDuration))
                .Join(target.transform.DORotate(battlemodePlayer.transform.rotation.eulerAngles, tweenDuration))


                // Stay
                .AppendInterval(stay)
                .JoinCallback(() =>
                {
                    if (leftIsActor) LeftAttacksRight(actor.transform, target.transform);
                    else RightAttacksLeft(actor.transform, target.transform);
                })

                // Move both back together
                .Append(actor.transform.DOMove(actorFinishLoc, tweenDuration))
                .Join(actor.transform.DORotate(Quaternion.identity.eulerAngles, tweenDuration))
                .Join(target.transform.DOMove(targetFinishLoc, tweenDuration))
                .Join(target.transform.DORotate(Quaternion.identity.eulerAngles, tweenDuration))
                .JoinCallback(() => DefaultAll(actor.transform, target.transform));
        }

        yield return sequence.WaitForCompletion();
    }

    void LeftAttacksRight(Transform actorLeft, Transform targetRight)
    {
        actorLeft.Get<Animator>().Play("LeftActing");
        targetRight.Get<Animator>().Play("RightHit");
    }

    void RightAttacksLeft(Transform actorRight, Transform targetLeft)
    {
        actorRight.Get<Animator>().Play("RightActing");
        targetLeft.Get<Animator>().Play("LeftHit");   
    }
    
    void LeftActsNoTarget(Transform actorLeft)
    {
        actorLeft.Get<Animator>().Play("LeftActing");
    }

    void DefaultAll(Transform actor, Transform target)
    {
        actor?.Get<Animator>().Play("Default");
        target?.Get<Animator>().Play("Default");  
    }
}