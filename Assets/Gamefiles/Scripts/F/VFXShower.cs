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
    }


    public List<VFXGO<SelfHitVFX>> selfHitVfxs = new();
    public List<VFXGO<SelfAttackingVFX>> selfAttackingVfxs = new();

    [Button]
    public void ShowHitVFX(SelfHitVFX selfHitVFX, bool fromPlayer)
    {
        var go = selfHitVfxs.Find(vfx => vfx.vfxEnum == selfHitVFX).gameObject;
        go.SetActive(fromPlayer);
        var scale = go.transform.localScale;
        go.transform.localScale = fromPlayer 
            ? scale.With(x: -Mathf.Abs(scale.x))
            : scale.With(x: Mathf.Abs(scale.x));
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
