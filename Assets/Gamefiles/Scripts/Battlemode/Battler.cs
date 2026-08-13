using UnityEngine;

public class Battler : MonoBehaviour
{
    public int currentHP;
    public int currentArmor;

    public void Init(BattlerConfig config)
    {
        currentHP = config.maxHP;
        currentArmor = config.maxArmor;
    }

}
