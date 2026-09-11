using EMILtools.Extensions;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class BattlerDisplayGenerator : MonoBehaviour
{
    [Required] public GameObject dmgNumberDisplayPrefab; 
    [Required] public SpriteRenderer bounds;
    Bounds _bounds => bounds.bounds;

    public string critPretext = "Crit!";
    public Color critColor;
    public Color normalColor;
    
    public void GenerateDmgNumberDisplay(int damage, bool crit)
    {
        var randX = Random.Range(_bounds.min.x, _bounds.max.x);
        var randY = Random.Range(_bounds.min.y, _bounds.max.y);
        var position = new Vector3(randX, randY, transform.position.z);
        var display = Instantiate(dmgNumberDisplayPrefab, transform);
        display.transform.position = position;
        var text = display.Get<TextMeshPro>();
        text.color = crit ? critColor : normalColor;
        text.text = crit ? critPretext + damage.ToString() : damage.ToString();
    }
}
