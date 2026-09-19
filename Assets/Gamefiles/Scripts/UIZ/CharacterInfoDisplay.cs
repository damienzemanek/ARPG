using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoDisplay : MonoBehaviour
{
    static readonly Type charSaverType = typeof(CharactersData_SavedDataSO);
    
    public enum CharacterSelectState
    {
        None,
        ChoseStarterCharacter,
    }
    
    public CharacterSelectState characterSelectState;
    [Required] public TextMeshProUGUI txt_Name;
    [Required] public TextMeshProUGUI txt_altName;
    [Required] public TextMeshProUGUI txt_spEffect;
    [Required] public TextMeshProUGUI txt_healthNum;
    [Required] public TextMeshProUGUI txt_apNum;
    [Required] public TextMeshProUGUI txt_dmgNum;
    [Required] public TextMeshProUGUI txt_armorNum;
    [Required] public TextMeshProUGUI txt_buttonLabel;

    
    [ReadOnly] public CharacterConfig selectedCharacter;
    [Required] public PlayerEvents playerEvents;
    [Required] public Button selectBtn;

    public void OnEnable()
    {
        selectedCharacter = null;
        selectBtn.interactable = false;
    }
    
    public void MainButtonPressed(CharacterSelectState state)
    {
        switch (state)
        {
            case CharacterSelectState.ChoseStarterCharacter: ChoseStarterCharacter(); break;
        }
    }
    
    #region ---------------------- Chose Starter Character ----------------------

    public void InitChoseStarterCharacter(CharacterConfig defaultCharacter)
    {
        characterSelectState = CharacterSelectState.ChoseStarterCharacter;
        selectedCharacter = defaultCharacter;
        txt_buttonLabel.text = "Select Character";
        SelectACharacter(defaultCharacter);
    }
    

    public void SelectACharacter(CharacterConfig character)
    {
        selectedCharacter = character;
        txt_Name.text = character.occupantName;
        txt_altName.text = character.occupantName;
        txt_spEffect.text = character.specialEffectName;
        txt_healthNum.text = character.maxHP.ToString();
        txt_apNum.text = character.maxAP.ToString();
        txt_dmgNum.text = character.damage.ToString();
        txt_armorNum.text = character.maxArmor.ToString();
        selectBtn.interactable = true;
    }
    
    public void ChoseStarterCharacter()
    {
        playerEvents.ChoseNewCharacter();

        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out var saver, out var data);
        if(data == null) Debug.LogError("No data found");
        var charData = data.charactersData.Find(c => c.characterConfigName == selectedCharacter.occupantName);
        if(charData == null) Debug.LogError("No character data found, search name was: " + selectedCharacter.occupantName + "");
        charData.hasCharacter = true;
    }
    
    #endregion ------------------------------------------------------------------
    
    
}
