using UnityEngine;

public class EE_SpikedBuckler : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Spiked Buckler";

    public override int priority => 1;

    public override bool isSpecial => false;

    // After landing 3 Attacks on the same turn, gain +1 Riposte. Once per turn.

    bool hasTriggered = false;
    int attackCount = 0;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SpikedBuckler effect to non-battler occupant");
            return actionCtx;
        }

        if (attackCount >= 3 && !hasTriggered)
        {
            // +1 Riposte
            hasTriggered = true;
        }

        attackCount++; // need to check if its an attack action
        return actionCtx;
    }

    // cant override when turn ends function
    // reset attackCount and hasTriggered
}
