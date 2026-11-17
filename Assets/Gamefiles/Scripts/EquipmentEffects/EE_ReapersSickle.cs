using UnityEngine;

public class EE_ReapersSickle : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Reaper's Sickle";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Gain +1 Critical Available after killing an enemy

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_ReapersSickle effect to non-battler occupant");
            return actionCtx;
        }

        // if attack action kills enemy
        // +1 crit available
        return actionCtx;
    }
}
