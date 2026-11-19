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
        public TEnum vfxEnum;
        public GameObject gameObject;
        [Required] public Vector3 localStartPosPlayerActorLHS;
        [Required] public Vector3 localStartPosEnemyActorRHS;
        public WorldTweener.Tween endPlayerActorHitsLHS;
        public WorldTweener.Tween endEnemyActorHitsRHS;
        
    }


    public List<VFXGO<SelfHitVFX>> selfHitVfxs = new();
    public List<VFXGO<SelfAttackingVFX>> selfAttackingVfxs = new();

    [Button]
    public void ShowHitVFX(SelfHitVFX selfHitVFX, bool fromPlayer)
    {
        var vfx = selfHitVfxs.Find(vfx => vfx.vfxEnum == selfHitVFX);
        vfx.gameObject.SetActive(fromPlayer);
        vfx.gameObject.transform.position = fromPlayer 
            ? vfx.localStartPosPlayerActorLHS 
            : vfx.localStartPosEnemyActorRHS;
        var scale = vfx.gameObject.transform.localScale;
        vfx.gameObject.transform.localScale = fromPlayer 
            ? scale.With(x: -Mathf.Abs(scale.x))
            : scale.With(x: Mathf.Abs(scale.x));
        vfx.gameObject.Get<WorldTweener>()
            .TweenGivenTweens(fromPlayer ? vfx.endPlayerActorHitsLHS : vfx.endEnemyActorHitsRHS);
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
