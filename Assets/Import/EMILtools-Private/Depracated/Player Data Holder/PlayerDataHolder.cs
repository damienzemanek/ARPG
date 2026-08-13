using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPatterns.CreationalPatterns;
using Sirenix.OdinInspector;

public class PlayerDataHolder : Singleton<PlayerDataHolder>
{
    [SerializeField, InlineEditor] PlayerDataDepracted _data;
    public PlayerDataDepracted data { get => _data; set => _data = value; }
}
