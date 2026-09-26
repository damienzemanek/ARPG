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
        Equipping,
        MovingOrDequipping,
    }

    [Required] public GameObject lbl_currentlyEquipping;
    [Required] public GameObject lbl_moving;
    [Required] public GameObject lbl_selectAnOpenTile;
    [Required]  public Transform gridParent;
    public List<CharacterSelectPortraitSquare> allSquares = new();
    public CharacterSelectPortraitSquare currentlySelectedSquare = null;
    public CharacterSelectPortrait currentlyEquippingPortrait = null;
    [ReadOnly] public CurrentOperation currentOperation = CurrentOperation.None;
    
    [Button] public void InitGrid() => gridParent.GetComponentsInChildren(allSquares);

    void OnEnable() => ResetState();
    
    // public void UnequipFromGrid(CharacterConfig cfg)
    // {
    //     var matchSquare = allSquares.First(s => s.character == cfg);
    //     matchSquare.UpdateSquareState(CharacterSelectPortraitSquareState.None);
    // }

    public void InteractWithCharacterSelectPortrait(CharacterSelectPortrait portrait)
    {
        if (currentOperation == CurrentOperation.None) StartEquipping(portrait);
        else if(currentOperation == CurrentOperation.Equipping) ResetState();
    }

    public void InteractWithCharacterSelectPortraitSquare(CharacterSelectPortraitSquare square)
    {
        if (currentOperation == CurrentOperation.None) StartMovingOrDequipping(square);
        else if (currentOperation == CurrentOperation.MovingOrDequipping) ResetState();
    }
    
    public void StartEquipping(CharacterSelectPortrait portrait)
    {
        currentOperation = CurrentOperation.Equipping;
        currentlyEquippingPortrait = portrait;
        lbl_moving.SetActive(false);
        lbl_currentlyEquipping.SetActive(true);
        lbl_selectAnOpenTile.SetActive(true);
    }

    public void ResetState()
    {
        currentOperation = CurrentOperation.None;
        currentlyEquippingPortrait = null;
        currentlySelectedSquare = null;
        lbl_moving.SetActive(false);
        lbl_currentlyEquipping.SetActive(false);
        lbl_selectAnOpenTile.SetActive(false);
    }

    public void StartMovingOrDequipping(CharacterSelectPortraitSquare square)
    {
        currentOperation = CurrentOperation.MovingOrDequipping;
        currentlySelectedSquare = square;
        lbl_moving.SetActive(true);
        lbl_currentlyEquipping.SetActive(false);
        lbl_selectAnOpenTile.SetActive(false);
    }
    
    public void EquipCharacter()
    {
        currentlyEquippingPortrait.UpdateState(CharacterSelectPortrait.CharacterSelectPortraitState.Equipped);
        ResetState();
    }
    
    
    
    
}
