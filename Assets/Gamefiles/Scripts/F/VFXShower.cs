using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class VFXShower : MonoBehaviour
{
    public enum SelfAttackingVFX
    {
        
    }

    public enum SelfHitVFX
    {
        Knife,
        Mind,
        Bullet,
        DblBullet,
        Slash1,
        Slash2,
    }

    [Serializable]
    public class VFXGO<TEnum> where TEnum : Enum
    {
        public enum PoseType
        {
            Pos,
            Rot,
            Scale
        }
        public TEnum vfxEnum;
        public GameObject gameObject;
        public float postStayDuration = 0;
        public List<WorldTweener.TweenPose> linearPoseProgressionEnemyActorHitsRHS = new() { new WorldTweener.TweenPose()}; // end at least
        public List<WorldTweener.TweenPose> linearPoseProgressionPlayerActorHitsLHS = new() { new WorldTweener.TweenPose() }; // end end at least

        [Button]
        public void Take(PoseType poseType, int index, bool fromPlayer)
        {
            var list = fromPlayer ? linearPoseProgressionPlayerActorHitsLHS : linearPoseProgressionEnemyActorHitsRHS;
            if (poseType == PoseType.Pos)
            {
                var v = gameObject.transform.localPosition;
                list[index].pos.tweenX = v.x;
                list[index].pos.tweenY = v.y;
                list[index].pos.tweenZ = v.z;
            }
            else if (poseType == PoseType.Rot)
            {
                var v = gameObject.transform.localEulerAngles;
                list[index].rot.tweenX = v.x;
                list[index].rot.tweenY = v.y;
                list[index].rot.tweenZ = v.z;
            }
            else if (poseType == PoseType.Scale)
            {
                var v = gameObject.transform.localScale;
                list[index].scale.tweenX = v.x;
                list[index].scale.tweenY = v.y;
                list[index].scale.tweenZ = v.z;
            }
        }
    }

    [Required] public Transform vfxParent;
    public List<VFXGO<SelfHitVFX>> selfHitVfxs = new();
    public List<VFXGO<SelfAttackingVFX>> selfAttackingVfxs = new();

    [Button]
    public void ShowHitVFX(SelfHitVFX selfHitVFX, bool fromPlayer)
    {
        var vfx = selfHitVfxs.Find(vfx => vfx.vfxEnum == selfHitVFX);
        vfx.gameObject.SetActive(true);
        vfx.gameObject.transform.localPosition = fromPlayer 
            ? vfx.linearPoseProgressionPlayerActorHitsLHS[0]._pos ? vfx.linearPoseProgressionPlayerActorHitsLHS[0].pos.AsVector3() : vfx.gameObject.transform.localPosition
            : vfx.linearPoseProgressionEnemyActorHitsRHS[0]._pos ? vfx.linearPoseProgressionEnemyActorHitsRHS[0].pos.AsVector3() :  vfx.gameObject.transform.localPosition;

        vfx.gameObject.transform.localRotation = Quaternion.Euler(fromPlayer
            ? vfx.linearPoseProgressionPlayerActorHitsLHS[0]._rot ? vfx.linearPoseProgressionPlayerActorHitsLHS[0].rot.AsVector3() :  vfx.gameObject.transform.localEulerAngles
            : vfx.linearPoseProgressionEnemyActorHitsRHS[0]._rot ? vfx.linearPoseProgressionEnemyActorHitsRHS[0].rot.AsVector3() : vfx.gameObject.transform.localEulerAngles);
        
        vfx.gameObject.transform.localScale = fromPlayer 
            ? vfx.linearPoseProgressionPlayerActorHitsLHS[0]._scale ? (vfx.linearPoseProgressionPlayerActorHitsLHS[0].scale.AsVector3()) : vfx.gameObject.transform.localScale
            : vfx.linearPoseProgressionEnemyActorHitsRHS[0]._scale ?  vfx.linearPoseProgressionEnemyActorHitsRHS[0].scale.AsVector3() : vfx.gameObject.transform.localScale;
        
        var scale = vfx.gameObject.transform.localScale;
        vfx.gameObject.transform.localScale = fromPlayer 
            ? scale.With(x: -Mathf.Abs(scale.x))
            : scale.With(x: Mathf.Abs(scale.x));
        
        Debug.Log("Tweening...");
        vfx.gameObject.Get<WorldTweener>()
            .TweenPosesLinearProgression(
                fromPlayer ? vfx.linearPoseProgressionPlayerActorHitsLHS : vfx.linearPoseProgressionEnemyActorHitsRHS, 
                vfx.postStayDuration,
                () =>
                {
                    Debug.Log("VFX sequence completed - disabling VFX");
                    vfx.gameObject.SetActive(false);                    
                });
    }

    [Button]
    public void StopShowingHitVFX()
    {
        foreach (var vfx in selfHitVfxs)
            vfx.gameObject.SetActive(false);
    }
    

    [Button]
    public void InitEnums()
    {
        foreach (var val in Enum.GetValues(typeof(SelfAttackingVFX)))
        {
            if(selfAttackingVfxs.Any(sav => sav.vfxEnum == (SelfAttackingVFX)val))
                continue;
            selfAttackingVfxs.Add(new VFXGO<SelfAttackingVFX>());
        }

        foreach (var val in Enum.GetValues(typeof(SelfHitVFX)))
        {
            if(selfHitVfxs.Any(sav => sav.vfxEnum == (SelfHitVFX)val))
                continue;
            selfHitVfxs.Add(new VFXGO<SelfHitVFX>());
        }
    }
    
    
}
