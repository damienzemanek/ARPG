using System;

public interface IDamageable
{
    public enum DeathType
    {
        Regular,
    }

    public enum DamageLocation
    {
        Body,
    }
    
    public enum DamageType
    {
        Regular,
    }
    
    [Serializable]
    public struct DamageInfo
    {
        public int dmg;
        public DamageLocation location;
        public DamageType type;

        public DamageInfo(int dmg, DamageType type, DamageLocation location)
        {
            this.dmg = dmg;
            this.location = location;
            this.type = type;
        }
    }
    
    public float TakeDamage(DamageInfo info);
}

public interface IHealable<TEnumHealthStates>
    where TEnumHealthStates : Enum
{
    public float Heal(float amount, out TEnumHealthStates newState);
}