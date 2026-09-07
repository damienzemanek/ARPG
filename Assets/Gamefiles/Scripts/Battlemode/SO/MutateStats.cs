using UnityEngine;

[CreateAssetMenu(fileName = "Mutate Stats Mark Strategy", menuName = "SO/ARPG/Mark Strategy")]
public class MutateStats : MarkStrategy
{
    public int dmgMultDelta;
    public int healMultDelta;
    public int armorMultDelta;
    public int apDelta;
    public int hitCountDelta;

    public override void ResolveMarkStrategy(OccupantCtx occupantCtx, BattlemodeActionCtx actionCtx)
    {
        actionCtx.deltaDmgMultiplier += dmgMultDelta;
        actionCtx.deltaHealMultiplier += healMultDelta;
        actionCtx.deltaArmorMultiplier += armorMultDelta;
        actionCtx.apDelta  += apDelta;
        actionCtx.hitCountDelta += hitCountDelta;
        Debug.Log("Mark Strategy Resolved " + name + $"Deltas: DMG: {dmgMultDelta} Heal: {healMultDelta} Armor: {armorMultDelta} AP: {apDelta} Hit Count: {hitCountDelta}");
    }
}