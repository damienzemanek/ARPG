using System.Collections;
using DG.Tweening;
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
        GameObject actor,
        GameObject target)
    {
        if (actor == null || target == null)
        {
            Debug.LogError("No actor or target given to ActionUserViewer.");
            yield break;
        }

        Vector3 actorOriginalPosition = actor.transform.position;
        Vector3 targetOriginalPosition = target.transform.position;

        Sequence sequence = DOTween.Sequence();

        if (actor == target)
        {
            sequence
                .Append(actor.transform.DOMove(leftLoc.position, tweenDuration))
                .Join(actor.transform.DORotate(battlemodePlayer.transform.rotation.eulerAngles, tweenDuration))
                .AppendInterval(stay)
                .Append(actor.transform.DOMove(actorOriginalPosition, tweenDuration))
                .Join(actor.transform.DORotate(Quaternion.identity.eulerAngles, tweenDuration));
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

                // Move both back together
                .Append(actor.transform.DOMove(actorOriginalPosition, tweenDuration))
                .Join(actor.transform.DORotate(Quaternion.identity.eulerAngles, tweenDuration))
                .Join(target.transform.DOMove(targetOriginalPosition, tweenDuration))
                .Join(target.transform.DORotate(Quaternion.identity.eulerAngles, tweenDuration));
        }

        yield return sequence.WaitForCompletion();
    }
}