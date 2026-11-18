using UnityEngine;

public class EE_SkirmishersRapier : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Skirmisher's Rapier";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Attacks deal +1 damage everytime you move or reposition this turn

    int moveCount = 0;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SkirmishersRapier effect to non-battler occupant");
            return actionCtx;
        }

        // if move moveCount++

        actionCtx.dmg = Mathf.FloorToInt(actionCtx.dmg + moveCount + 0.5f);
        return actionCtx;
    }

    // cant override when turn ends function
    // reset moveCount 
}
