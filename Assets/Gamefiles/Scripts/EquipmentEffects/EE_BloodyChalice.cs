using UnityEngine;

public class EE_BloodyChalice : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Bloody Chalice";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Heal 50% of max HP when HP falls below 20%, once per battle. Max HP -10%

    bool isTriggered = false;

    public override BattlemodeActionCtx ResolveEffecStartOfBattle(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_BloodyChalice effect to non-battler occupant");
            return actionCtx;
        }

        battlerOccupantCtx.maxHp = Mathf.FloorToInt(battlerOccupantCtx.maxHp * 0.85f + 0.5f);
        return actionCtx;
    }

    public override BattlemodeActionCtx ResolveEffectAfterHitByAction(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_BloodyChalice effect to non-battler occupant");
            return actionCtx;
        }

        if (battlerOccupantCtx.currentHp <= Mathf.FloorToInt(battlerOccupantCtx.maxHp * 0.2f + 0.5f) && !isTriggered)
        {
            battlerOccupantCtx.currentHp += Mathf.FloorToInt(battlerOccupantCtx.maxHp * 0.5f + 0.5f);
            isTriggered = true;
        }
        return actionCtx;
    }
}
