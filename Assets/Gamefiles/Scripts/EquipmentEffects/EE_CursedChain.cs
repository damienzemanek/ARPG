using UnityEngine;

public class EE_CursedChain : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Cursed Chain";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If you haven't moved by the end of turn, gain +2 AP and Move Block for the next turn. Cool down 1 turn.

    int cooldownTurnRemaining = 0;
    bool effectTrigger = false;

    // cant override for end of turn
    // if havent moved effectTrigger=true
    // cooldown--;

    public override BattlemodeActionCtx ResolveEffectPreBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_CursedChain effect to non-battler occupant");
            return actionCtx;
        }

        if (effectTrigger && cooldownTurnRemaining == 0)
        {
            // +2 AP and move block
            effectTrigger = false;
            cooldownTurnRemaining = 2;
        }
        return actionCtx;
    }
}
