using System.Collections.Generic;
using UnityEngine;

public class EE_SpikedBuckler : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Spiked Buckler";

    public override int priority => 1;

    public override bool isSpecial => false;

    // After landing 3 Attacks on the same turn, gain +1 Riposte. Once per turn.

    // Replace this with Rispote effect when it is implemented
    BattlemodeEffectStrategy_Swift newSwiftEffect => CreateInstance<BattlemodeEffectStrategy_Swift>
        (1, 0, 0, BattlemodeEffectConfigInstance.AddOccurance.AfterAction);

    bool hasTriggered = false;
    int maxCount = 3;
    int attackCount = 0;

    public override BattlemodeActionCtx ResolveEffectAfterActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_SpikedBuckler effect to non-battler occupant");
            return actionCtx;
        }

        if (actionCtx.dmg <= 0) // Only count actions that deal damage
        {
            return actionCtx;
        }
        attackCount++;

        if (attackCount >= maxCount && !hasTriggered)
        {
            // Replace this with Rispote effect when it is implemented
            actionCtx.additionalEffectsToApplyToActor ??= new List<BattlemodeEffectStrategyInstance>();
            actionCtx.additionalEffectsToApplyToActor.Add(newSwiftEffect);
            hasTriggered = true;
        }
        return actionCtx;
    }

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx, BattleTile tile)
    {
        hasTriggered = false;
    }
}
