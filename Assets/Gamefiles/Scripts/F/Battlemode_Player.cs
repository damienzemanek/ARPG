using System;
using System.Collections;
using DG.Tweening;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class BattlemodePlayerInstance : MonoBehaviour
{
    public enum ZoomState
    {
        Battlefield,
        ZoomedIntoTile,
        QueuedAction
    }

    public Pose originalPose;
    public Pose zoomInOffsetPose;
    public float maxLateralDelta = 4f;
    public float scrollSmoothingRate = 0.15f;
    public float ADsmoothingRate = 0.3f;
    public float zoomInSmoothingRate = 0.3f;

    [ReadOnly, ShowInInspector] Vector3 savedBattlefieldZoomPos;
    [ReadOnly, ShowInInspector] public ZoomState zoomState = ZoomState.Battlefield;
    Vector3 playerSmoothingVelocity;
    Vector3 maxLeftPos, maxRightPos;
    Tween zoomTween;
    Tween zoomRotTween;
    
    void Awake()
    {
        maxLeftPos = new Vector3(transform.position.x - maxLateralDelta, transform.position.y, transform.position.z);
        maxRightPos = new Vector3(transform.position.x + maxLateralDelta, transform.position.y, transform.position.z);
    }

    [Button]
    public void ZoomIntoTile(BattleTile tileToZoomInto, Action onCompleteZoomIn)
    {
        if (zoomState.Equals(ZoomState.Battlefield))
            savedBattlefieldZoomPos = transform.position;
        
        zoomState = ZoomState.ZoomedIntoTile;
        tileToZoomInto.SelectImplementation();

        if (Application.isPlaying)
        {
            zoomRotTween?.Kill();
            zoomRotTween = transform
                .DORotate(zoomInOffsetPose.rotation, zoomInSmoothingRate)
                .SetEase(Ease.OutCubic);
            zoomTween?.Kill();
            zoomTween = transform
                .DOMove(tileToZoomInto.transform.position + zoomInOffsetPose.position, zoomInSmoothingRate)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => onCompleteZoomIn?.Invoke());
        }
        else transform.position = tileToZoomInto.transform.position + zoomInOffsetPose.position;
    }

    public void ZoomOutToSelectQueuedAction()
    {
        zoomState = ZoomState.QueuedAction;
        
        if (Application.isPlaying)
        {
            zoomRotTween?.Kill();
            zoomRotTween = transform
                .DORotate(originalPose.rotation, zoomInSmoothingRate)
                .SetEase(Ease.OutCubic);
            zoomTween?.Kill();
            zoomTween = transform
                .DOMove(originalPose.position, zoomInSmoothingRate)
                .SetEase(Ease.OutCubic);
        }
        else transform.position = originalPose.position;
    }

    [Button]
    public void BackZoomEsc()
    {
        if (zoomState.Equals(ZoomState.ZoomedIntoTile)) 
        {
            if (Application.isPlaying)
            {
                zoomRotTween?.Kill();
                zoomRotTween = transform
                    .DORotate(originalPose.rotation, zoomInSmoothingRate)
                    .SetEase(Ease.OutCubic);
                zoomTween?.Kill();
                zoomTween = transform
                    .DOMove(savedBattlefieldZoomPos, zoomInSmoothingRate)
                    .SetEase(Ease.OutCubic);
            }
            else transform.position = savedBattlefieldZoomPos;
            
            zoomState = ZoomState.Battlefield;
            savedBattlefieldZoomPos = Vector3.zero;
            BattleTracker.Instance.UnSelectAll();
            Debug.Log("UnZoom");
        }
        else if (zoomState.Equals(ZoomState.QueuedAction))
        {
            zoomState = ZoomState.ZoomedIntoTile;
            BattleTracker.Instance.SelectTile(BattleTracker.Instance.queuedPlayerAction.targetTile, true);
        }
        else if (zoomState.Equals(ZoomState.Battlefield)) 
            Debug.Log("Already at battlefield zoom");
    }

    public void ScrollUpMoveLeft(bool scroll)
    {
        if(zoomState.Equals(ZoomState.ZoomedIntoTile)) return;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            maxLeftPos,
            ref playerSmoothingVelocity,
            scroll ? scrollSmoothingRate : ADsmoothingRate);

    }
    public void ScrollUpMoveRight(bool scroll)
    {
        if(zoomState.Equals(ZoomState.ZoomedIntoTile)) return;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            maxRightPos,
            ref playerSmoothingVelocity,
            scroll ? scrollSmoothingRate : ADsmoothingRate);
    }

    void Update()
    {
        if (zoomTween != null && zoomTween.active) return;
        if (zoomState.Equals(ZoomState.ZoomedIntoTile)) return;
        if(transform.position.x < maxLeftPos.x) transform.position = maxLeftPos;
        if(transform.position.x > maxRightPos.x) transform.position = maxRightPos;
    }
    
    [Button]
    void ResetToOriginalLoc() => transform.position = originalPose.position;
    
}
