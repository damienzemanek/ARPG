using UnityEngine;

public class EE_GamblersPlayingCard : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Gambler's Playing Card";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Attack Actions that cost 2 AP or less have a 50% chance to deal 150% damage, 50% chance to deal 50% damage.

    public override BattlemodeActionCtx ResolveEffectRightBeforeActing(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_GamblersPlayingCard effect to non-battler occupant");
            return actionCtx;
        }

        if (actionCtx.ap <= 2)
        {
            if (Random.value < 0.5f)
            {
                actionCtx.dmg = Mathf.FloorToInt(actionCtx.dmg * 1.5f + 0.5f);
            }
            else
            {
                actionCtx.dmg = Mathf.FloorToInt(actionCtx.dmg * 0.5f + 0.5f);
            }
        }
        return actionCtx;
    }
}
