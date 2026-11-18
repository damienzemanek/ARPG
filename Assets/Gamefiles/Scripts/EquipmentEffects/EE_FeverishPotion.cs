using UnityEngine;

public class EE_FeverishPotion : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Feverish Potion";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Increase Max AP by 2, but lose -1 max AP at the end of turn for 3 turns

    int maxAPdecrease = 3;
    int timeTriggered = 0;

    public override BattlemodeActionCtx ResolveEffecStartOfBattle(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_FeverishPotion effect to non-battler occupant");
            return actionCtx;
        }

        // battlerOccupantCtx.maxAP Theres no maxAP
        return actionCtx;
    }
    
    // is this at turn start?
    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_FeverishPotion effect to non-battler occupant");
            return actionCtx;
        }

        // i couldnt find AP in battlerOccupantCtx

        return actionCtx;
    }
}
