using UnityEngine;

public class EE_SinisterTome : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Sinister Tome";

    public override int priority => 1;

    public override bool isSpecial => false;

    // First attack of each turn has a 50% chance to gain +1 Swift,
    // 50% to get Move Block for the rest of the turn. Swift gain this way will not stack.

    bool hasTriggered = false;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SinisterTome effect to non-battler occupant");
            return actionCtx;
        }

        if(!hasTriggered)
        {
            if (Random.value < 0.5f)
            {
                // +1 Swift, does not stack
            }
            else
            {
                // Move block
            }
            hasTriggered = true;
        }
        return actionCtx;
    }
}
