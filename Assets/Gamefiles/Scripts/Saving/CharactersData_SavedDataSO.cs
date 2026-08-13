using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Characters Saved Data", menuName = "ARPG/SO/SavedData/Characters")]
public class CharactersData_SavedDataSO : SavedDataSO
{
    static readonly TypeSerialized<Type> _subType = new(typeof(CharactersData_SavedDataSO));
    public override TypeSerialized<Type> subType => _subType;
    
    public override string pathName => "Characters_SavedData";
    public override SavedDataSO CreateNewData() => CreateInstance<CharactersData_SavedDataSO>();
    
    public override void MigrateData(int oldVersion)
    {
        //noop for now
    }

    public override void ResetDataOptionalInternal()
    {
        if(currentCharacterConfigurations == null) Debug.LogError("No character configurations set");
        AddCharacterFromConfigurations(currentCharacterConfigurations);
    }

    [Serializable]
    public class CharacterData
    {
        public string characterConfigName;
        public bool hasCharacter = false;
        public int level = 1;

        public CharacterConfig GetConfig(string characterName)
        {
            var get = PersistentConfigurationDataHolder.Instance.characterConfigs.characterConfigs.GetValueOrDefault(characterName);
            if(get == null) Debug.LogError($"No character config found for {characterName}"); return get;
        }
    }

    [ReadOnly, SerializeField] CharacterConfigurations currentCharacterConfigurations;
    [ReadOnly] public List<CharacterData> charactersData = new();

    public CharacterData GetCharacterData(CharacterConfig character) 
        => GetCharacterData(character.occupantName);

    public CharacterData GetCharacterData(string characterName)
        => charactersData.Find(c => c.characterConfigName == characterName);

    [Button] public void AddCharacterFromConfigurations(CharacterConfigurations configs)
    {
        this.currentCharacterConfigurations = configs;
        charactersData.Clear();
        foreach (var character in configs.characterConfigs)
        {
            var data = new CharacterData { characterConfigName = character.Key };
            charactersData.Add(data);
        }
    }
}