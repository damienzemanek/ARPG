using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using static BattlemodeActionConfig;

public class BattlerDisplayGenerator : MonoBehaviour
{
    [Required] public GameObject dmgNumberDisplayPrefab;
    [Required] public Transform spawnpos;

    public int vertOffset = 3;
    public string critPretext = "Crit!";
    public Color critColor;
    public Color healColor;
    public Color damageColor;
    public Color armorColor;
    public Color buffColor => Color.limeGreen;
    public Color debuffColor => Color.mediumPurple;
    
    [Button]
    public void GenerateNumberDisplay(int amount, bool crit, ActionIdentifier actionIdentifier)
    {
        var color = actionIdentifier switch
        {
            ActionIdentifier.Attack 
                or ActionIdentifier.AttackBuff
                or ActionIdentifier.AttackDebuff 
                or ActionIdentifier.AttackMove => damageColor,
            ActionIdentifier.Heal 
                or ActionIdentifier.HealBuff 
                or ActionIdentifier.HealDebuff 
                or ActionIdentifier.HealMove => healColor,
            ActionIdentifier.Armor
                or ActionIdentifier.ArmorBuff
                or ActionIdentifier.ArmorDebuff 
                or ActionIdentifier.ArmorMove => armorColor,
            ActionIdentifier.Buff
                or ActionIdentifier.BuffMove => buffColor,
            ActionIdentifier.Debuff
                or ActionIdentifier.DebuffMove => debuffColor
        };
        if (color == buffColor || color == debuffColor) return;
        var display = Instantiate(dmgNumberDisplayPrefab, transform);
        display.transform.position = spawnpos.position;
        var text = display.GetComponentInChildren<TextMeshPro>();
        string amountText = amount.ToString();
        text.color = crit ? critColor : color;
        text.text  = crit ? critPretext + amountText : amountText;
    }
}
