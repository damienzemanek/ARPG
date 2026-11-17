using UnityEngine;

public class EE_TornedLeatherHat : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Torned Leather Hat";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Increase Max HP by 25%, decrease Max Armor by 15%

    public override BattlemodeActionCtx ResolveEffecStartOfBattle(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_TornedLeatherHat effect to non-battler occupant");
            return actionCtx;
        }

        battlerOccupantCtx.maxHp = Mathf.FloorToInt(battlerOccupantCtx.maxHp * 1.25f + 0.5f);
        battlerOccupantCtx.maxArmor = Mathf.FloorToInt(battlerOccupantCtx.maxArmor * 0.85f + 0.5f);
        return actionCtx;
    }
}
