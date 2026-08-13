using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using Sirenix.OdinInspector;

public class PersistentConfigurationDataHolder : PersistantReplacerSingleton<PersistentConfigurationDataHolder>
{
    [Required] public PrefabData prefabData;
    [Required] public LoadIn sceneLoadInMethod;
    [Required] public CharacterConfigurations characterConfigs;
    [Required] public EffectDict effectDict;
}