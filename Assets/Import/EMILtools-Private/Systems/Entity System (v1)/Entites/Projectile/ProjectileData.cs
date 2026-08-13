using UnityEngine;


[CreateAssetMenu(fileName = "Projectile Data", menuName = "ScriptableObjects/Projectile Data")]
public class ProjectileData : EntityData
{
    public float forceScalar;
    public ForceMode forceMode;
    public GameObject hitEffectPrefab;
    public IDamageable.DamageType dmgType;
    public int dmg;
    
    public IDamageable.DamageInfo CreateDamageContext(IDamageable.DamageLocation location)
        => new IDamageable.DamageInfo(dmg, dmgType, location);
}
