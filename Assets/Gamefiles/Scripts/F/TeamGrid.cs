using System;
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

    [Required] public GameObject lbl_currentlyEquipping;
    [Required] public GameObject lbl_selectAnOpenTile;
    [Required]  public Transform gridParent;
    public List<CharacterSelectPortraitSquare> allSquares = new();
    public CharacterSelectPortraitSquare currentlySelectedSquare = null;
    [ReadOnly] public CurrentOperation currentOperation = CurrentOperation.None;
    [ReadOnly] public CharacterConfig currentEquippingCharacter = null;
    
    
    [Button] public void InitGrid() => gridParent.GetComponentsInChildren(allSquares);

    void OnEnable() => StopEquipping();

    public void SelectPortrait(CharacterSelectPortraitSquare square)
    {
        currentlySelectedSquare = square;
    }


    public void UnequipFromGrid(CharacterConfig cfg)
    {
        var matchSquare = allSquares.First(s => s.character == cfg);
        matchSquare.UpdateState(CharacterSelectPortraitSquareState.None);
    }

    public void InteractWithCharacterSelectPortrait(CharacterConfig cfg)
    {
        if (currentOperation == CurrentOperation.None) StartEquipping(cfg);
        else if(currentOperation == CurrentOperation.Equipping) StopEquipping();

    }

    void StartEquipping(CharacterConfig cfg)
    {
        currentOperation = CurrentOperation.Equipping;
        currentEquippingCharacter = cfg;
        lbl_currentlyEquipping.SetActive(true);
        lbl_selectAnOpenTile.SetActive(true);
    }

    void StopEquipping()
    {
        currentOperation = CurrentOperation.None;
        currentEquippingCharacter = null;
        lbl_currentlyEquipping.SetActive(false);
        lbl_selectAnOpenTile.SetActive(false);
    }
    
    
    
}
