using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoDisplay : MonoBehaviour
{
    static readonly Type charSaverType = typeof(CharactersData_SavedDataSO);
    
    public TextMeshProUGUI txt_Name;
    [ReadOnly] public CharacterConfig viewingCharacter;
    [Required] public PlayerEvents playerEvents;
    [Required] public Button selectBtn;

    public void OnEnable()
    {
        txt_Name.text = "Select A Character";
        viewingCharacter = null;
        selectBtn.interactable = false;
    }

    public void ViewCharacter(CharacterConfig character)
    {
        viewingCharacter = character;
        txt_Name.text = character.occupantName;
        selectBtn.interactable = true;
    }

    public void SelectCharacter()
    {
        playerEvents.SelectedNewPlayerCharacter(viewingCharacter);
        var charSaver = Saver.Instance.GetFirstSaver(charSaverType);
        var data = charSaver.currentData as CharactersData_SavedDataSO;
        if(data == null) Debug.LogError("No data found");
        Debug.Log("data character size: " + data.charactersData.Count);
        var charData = data.charactersData.Find(c => c.characterConfigName == viewingCharacter.occupantName);
        if(charData == null) Debug.LogError("No character data found, search name was: " + viewingCharacter.occupantName + "");
        charData.hasCharacter = true;
    }
}
