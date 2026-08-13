using DesignPatterns.CreationalPatterns;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class RegionUI : Singleton<RegionUI>
{
    [Required] public TextMeshProUGUI txt_regionName;
    [Required] public GameObject regionUI;
    
    public void DisplayRegionUI(string regionName)
    {
        txt_regionName.text = regionName;
        regionUI.SetActive(true);
    }
    
}
