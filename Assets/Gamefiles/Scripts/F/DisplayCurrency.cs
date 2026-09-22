using TMPro;
using UnityEngine;

public class DisplayCurrency : MonoBehaviour
{
    public TextMeshProUGUI txt_goldNum;
    public TextMeshProUGUI txt_crystalNum;

    public void UpdateCurrency(int gold, int crystals)
    {   
        txt_goldNum.text = gold.ToString();
        txt_crystalNum.text = crystals.ToString();
    }
}
