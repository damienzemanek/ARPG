using UnityEngine;

public class EE_ClockworkPendant : BattlemodeEffectStrategyInstance
{
    public override string effectName => "Clockwork Pendant";

    public override int priority => 1;

    public override bool isSpecial => false;

    // If the character has 0 AP at the end of turn, gain 1 AP at the start of next turn.

    bool isOutOfAP = false;

    // public override ResolveEffectAfterTurnEnds() I cant override this 
}
