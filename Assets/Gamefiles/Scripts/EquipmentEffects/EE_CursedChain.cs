using System.Collections.Generic;
using UnityEngine;

public class EE_CursedChain : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Cursed Chain";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If you haven't moved by the end of turn, gain +2 AP and Move Locked for the next turn. Cool down 1 turn.

    BattlemodeEffectStrategy_MoveLocked newMoveLockedEffect => CreateInstance<BattlemodeEffectStrategy_MoveLocked>
        (1, 0, 0, BattlemodeEffectConfigInstance.AddOccurance.AfterAction);

    int cooldownTurnRemaining = 0;
    bool effectTrigger = false;

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx, BattleTile tile)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_CursedChain effect to non-battler occupant");
            return;
        }
        if (battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_CursedChain effect to non-character battler occupant");
            return;
        }

        if (battlerOccupantCtx.hasFreeRequeue) 
        {
            effectTrigger = true;
        }
        cooldownTurnRemaining--;
        return;
    }

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_CursedChain effect to non-battler occupant");
            return actionCtx;
        }
        if (battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_CursedChain effect to non-character battler occupant");
            return actionCtx;
        }

        if (effectTrigger && cooldownTurnRemaining == 0)
        {
            characterOccupantCtx.currentAP += 2;
            actionCtx.additionalEffectsToApplyToActor ??= new List<BattlemodeEffectStrategyInstance>();
            actionCtx.additionalEffectsToApplyToActor.Add(newMoveLockedEffect);

            effectTrigger = false;
            cooldownTurnRemaining = 2;
        }
        return actionCtx;
    }
}
