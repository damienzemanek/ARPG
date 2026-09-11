using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using static BattlemodeActionConfig;

public class BattlerDisplayGenerator : MonoBehaviour
{
    [Required] public GameObject dmgNumberDisplayPrefab; 
    [Required] public SpriteRenderer bounds;
    Bounds _bounds => bounds.bounds;

    public string critPretext = "Crit!";
    public Color critColor;
    public Color healColor;
    public Color damageColor;
    public Color armorColor;
    
    public void GenerateNumberDisplay(int amount, bool crit, ActionIdentifier actionIdentifier)
    {
        var randX = Random.Range(_bounds.min.x, _bounds.max.x);
        var randY = Random.Range(_bounds.min.y, _bounds.max.y);
        var position = new Vector3(randX, randY, transform.position.z);
        var display = Instantiate(dmgNumberDisplayPrefab, transform);
        display.transform.position = position;
        var text = display.Get<TextMeshPro>();
        var color = actionIdentifier switch
        {
            ActionIdentifier.Attack 
            or ActionIdentifier.AttackBuff
            or ActionIdentifier.ArmorDebuff 
            or ActionIdentifier.AttackMove => damageColor,
            ActionIdentifier.Heal 
            or ActionIdentifier.HealBuff 
            or ActionIdentifier.HealDebuff 
            or ActionIdentifier.HealMove => healColor,
            ActionIdentifier.Armor
            or ActionIdentifier.ArmorBuff
            or ActionIdentifier.ArmorDebuff 
            or ActionIdentifier.ArmorMove => armorColor,
            _ => damageColor
        };
        text.color = crit ? critColor : color;
        text.text  = crit ? critPretext + amount.ToString() : amount.ToString();
    }
}
