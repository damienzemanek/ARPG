using AYellowpaper.SerializedCollections;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterConfigurations", menuName = "ARPG/SO/CharacterConfigurations")]
public class CharacterConfigurations : ScriptableObject
{
    [DrawWithUnity] 
    public SerializedDictionary<string, CharacterConfig> characterConfigs = new();
}