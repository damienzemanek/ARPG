using System.Collections.Generic;
using UnityEngine;

public class EE_SinisterTome : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Sinister Tome";

    public override int priority => 1;

    public override bool isSpecial => false;

    // First attack of each turn has a 50% chance to gain +1 Swift,
    // 50% to get Move Locked for the rest of the turn.

    BattlemodeEffectStrategy_Swift newSwiftEffect => CreateInstance<BattlemodeEffectStrategy_Swift>
        (1, 0, 0, BattlemodeEffectConfigInstance.AddOccurance.AfterAction);
    BattlemodeEffectStrategy_MoveLocked newMoveLockedEffect => CreateInstance<BattlemodeEffectStrategy_MoveLocked>
        (1, 0, 0, BattlemodeEffectConfigInstance.AddOccurance.AfterAction);

    bool hasTriggered = false;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SinisterTome effect to non-battler occupant");
            return actionCtx;
        }
        if(battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SinisterTome effect to non-character battler occupant");
            return actionCtx;
        }

        if (actionCtx.dmg <= 0)
        {
            return actionCtx;
        }
        else if (!hasTriggered)
        {
            if (Random.value < 0.5f)
            {
                actionCtx.additionalEffectsToApplyToActor ??= new List<BattlemodeEffectStrategyInstance>();
                actionCtx.additionalEffectsToApplyToActor.Add(newSwiftEffect);
            }
            else
            {
                actionCtx.additionalEffectsToApplyToActor ??= new List<BattlemodeEffectStrategyInstance>();
                actionCtx.additionalEffectsToApplyToActor.Add(newMoveLockedEffect);
            }
            hasTriggered = true;
        }
        return actionCtx;
    }

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx, BattleTile tile)
    {
        hasTriggered = false;
    }
}
