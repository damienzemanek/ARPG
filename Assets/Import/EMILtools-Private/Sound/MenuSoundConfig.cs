using System;
using UnityEngine;

[CreateAssetMenu( fileName = "MenuSoundEnumSO", menuName = "ScriptableObjects/SoundEnums/Menu")]
public class MenuSoundsEnumSO : EnumSO
{
    public enum MenuSounds
    {
        Ambience,
    }
    public override string[] GetEnumVals() => Enum.GetNames(typeof(MenuSounds));
}