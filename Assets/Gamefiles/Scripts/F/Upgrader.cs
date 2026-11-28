using System.Collections;
using EMILtools.Extensions;
using TMPro;
using UnityEngine;

public class Upgrader : MonoBehaviour
{
    public Animator animator;
    public string animName = "Upgrade";
    public float mutateStatsDelay = 0.5f;

    public TextMeshProUGUI txt_newLvl;
    public GameObject go_stats;
    public GameObject go_cont_btn;
    public RectTransform rect_stats;
    
    public TextMeshProUGUI txt_ap;
    public TextMeshProUGUI txt_dmg;
    public TextMeshProUGUI txt_armor;
    public TextMeshProUGUI txt_health;

    public TextMeshProUGUI txt_delta_ap;
    public TextMeshProUGUI txt_delta_dmg;
    public TextMeshProUGUI txt_delta_armor;
    public TextMeshProUGUI txt_delta_health;
    
    public void Upgrade(int newLvl, CharacterConfig.CharacterStatDeltas currStats, CharacterConfig.CharacterStatDeltas deltaStats)
    {
        go_cont_btn.gameObject.SetActive(false);
        go_stats.SetActive(false);
        txt_newLvl.text = newLvl.ToString();
        txt_ap.text = currStats.ap.ToString();
        txt_dmg.text = currStats.damage.ToString();
        txt_armor.text = currStats.armor.ToString();
        txt_health.text = currStats.health.ToString();
        
        txt_ap.color = Color.white;
        txt_dmg.color = Color.white;
        txt_armor.color = Color.white;
        txt_health.color = Color.white;
        
        txt_delta_ap.text = deltaStats.ap > 0 ? "+" + deltaStats.ap : "";
        txt_delta_dmg.text = deltaStats.damage > 0 ? "+" + deltaStats.damage : "";
        txt_delta_armor.text = deltaStats.armor > 0 ? "+" + deltaStats.armor : "";
        txt_delta_health.text = deltaStats.health > 0 ? "+" + deltaStats.health : "";
        
        rect_stats.RefreshLayoutGroupsImmediateAndRecursive();
        animator.PlayOnEnd(animName, () =>  StartCoroutine(C_Upgrade(currStats, deltaStats)));
    }

    public IEnumerator C_Upgrade(CharacterConfig.CharacterStatDeltas currStats,
        CharacterConfig.CharacterStatDeltas deltaStats)
    {
        go_stats.SetActive(true);
        yield return new WaitForSeconds(mutateStatsDelay);
        
        if(deltaStats.ap > 0) txt_ap.color = Color.green;
        if(deltaStats.damage > 0) txt_dmg.color = Color.green;
        if(deltaStats.armor > 0) txt_armor.color = Color.green;
        if(deltaStats.health > 0) txt_health.color = Color.green;

        txt_ap.text = (currStats.ap + deltaStats.ap).ToString();
        txt_dmg.text = (currStats.damage + deltaStats.damage).ToString();
        txt_armor.text = (currStats.armor + deltaStats.armor).ToString();
        txt_health.text = (currStats.health + deltaStats.health).ToString();

        txt_delta_ap.text = "";
        txt_delta_dmg.text = "";
        txt_delta_armor.text = "";
        txt_delta_health.text = "";
        
        rect_stats.RefreshLayoutGroupsImmediateAndRecursive();

        yield return new WaitForSeconds(0.1f);

        go_cont_btn.gameObject.SetActive(true);
        
    }
}
