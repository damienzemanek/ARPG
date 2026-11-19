using UnityEngine;

public class EE_FeverishPotion : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Feverish Potion";

    public override int priority => 1;

    public override bool isSpecial => false;

    // Increase Max AP by 2, but lose -1 max AP at the end of turn for 3 turns

    int maxAPdecrease = 3;
    int timeTriggered = 0;

    public override BattlemodeActionCtx ResolveEffecStartOfBattle(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_FeverishPotion effect to non-battler occupant");
            return actionCtx;
        }
        if(battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_FeverishPotion effect to non-character battler occupant");
            return actionCtx;
        }

        characterOccupantCtx.maxAP += 2;
        return actionCtx;
    }

    public override void ResolveEffectAfterTurnEndsImplementation(OccupantCtx occupantCtx, BattleTile tile)
    {
        if (occupantCtx is not BattlerOccupantCtx battlerOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_FeverishPotion effect to non-battler occupant");
            return;
        }
        if(battlerOccupantCtx is not CharacterOccupantCtx characterOccupantCtx)
        {
            Debug.LogError("Trying to apply EE_FeverishPotion effect to non-character battler occupant");
            return;
        }
        if (timeTriggered < maxAPdecrease)
        {
            characterOccupantCtx.maxAP -= 1;
            timeTriggered++;
        }
    }
}
