using UnityEngine;

public class EE_Equipment1 : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Equipment1";

    public override int priority => 1;

    public override bool isSpecial => false;

    public override BattlemodeActionCtx ResolveEffecStartOfBattle(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if(occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply Equipment1 effect to non-battler occupant");
            return actionCtx;
        }


        if(battlerOccupantCtx.currentArmor > 0)
        {
            actionCtx.dmg = actionCtx.dmg * 2;
        }
        return actionCtx;
    }


}
