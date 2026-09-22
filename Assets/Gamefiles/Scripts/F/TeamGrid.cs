using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using static CharacterSelectPortraitSquare;

public class TeamGrid : MonoBehaviour
{

    public enum CurrentOperation
    {
        None,
        Equipping
    }

    public Transform gridParent;
    public List<CharacterSelectPortraitSquare> allSquares = new();
    [Button] public void InitGrid() => gridParent.GetComponentsInChildren(allSquares);
    public CharacterSelectPortraitSquare currentlySelectedSquare = null;
    public CurrentOperation currentOperation = CurrentOperation.None;
    [ReadOnly] public CharacterConfig currentEquippingCharacter = null;

    public void SelectPortrait(CharacterSelectPortraitSquare square)
    {
        currentlySelectedSquare = square;
    }


    public void UnequipFromGrid(CharacterConfig cfg)
    {
        var matchSquare = allSquares.First(s => s.character == cfg);
        matchSquare.UpdateState(CharacterSelectPortraitSquareState.None);
    }

    public void StartEquipFromPortraits(CharacterConfig cfg)
    {
        currentEquippingCharacter = cfg;
    }
    
    
    
}
