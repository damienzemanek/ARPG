using UnityEngine;

public class EE_ClockworkPendant : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Clockwork Pendant";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If the character has 0 AP at the end of turn, gain 1 AP at the start of next turn.

    bool isOutOfAP = false;

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx, BattleTile tile)
    {
        if(occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_ClockworkPendant effect to non-battler occupant");
            return;
        }
        if(battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_ClockworkPendant effect to non-character battler occupant");
            return;
        }

        if (characterOccupantCtx.currentAP <= 0)
        {
            isOutOfAP = true;
        }
        else
        {
            isOutOfAP = false;
        }
        return;
    }

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if(occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_ClockworkPendant effect to non-battler occupant");
            return actionCtx;
        }
        if(battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_ClockworkPendant effect to non-character battler occupant");
            return actionCtx;
        }

        if (isOutOfAP)
        {
            characterOccupantCtx.currentAP += 1;
            isOutOfAP = false;
        }
        return actionCtx;
    }

}
