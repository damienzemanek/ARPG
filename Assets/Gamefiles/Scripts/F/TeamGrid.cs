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

    [ReadOnly] public CurrentOperation currentOperation = CurrentOperation.None;
    public CharacterSelectPortraitSquare currentlySelectedSquare = null;
    public CharacterSelectPortrait currentlyEquippingPortrait = null;
    
    [Required] public GameObject lbl_currentlyEquipping;
    [Required] public GameObject lbl_moving;
    [Required] public GameObject lbl_selectAnOpenTile;
    [Required]  public Transform gridParent;
    [Required]  public Transform portraitsParent;
    [Required] public List<CharacterSelectPortraitSquare> allSquares = new();
    [Required] public List<CharacterSelectPortrait> allPortraits = new();
    

    [Button]
    public void InitGrid()
    {
        gridParent.GetComponentsInChildren(allSquares);
        portraitsParent.GetComponentsInChildren(allPortraits);
    }
    

    void OnEnable()
    {
        allSquares.ForEach(s => s.InitFromTeamGrid());
        ResetState();
        OpenTeamGrid();
    }
    
    // public void UnequipFromGrid(CharacterConfig cfg)
    // {
    //     var matchSquare = allSquares.First(s => s.character == cfg);
    //     matchSquare.UpdateSquareState(CharacterSelectPortraitSquareState.None);
    // }

    public void ResetState()
    {
        currentOperation = CurrentOperation.None;
        currentlyEquippingPortrait = null;
        currentlySelectedSquare = null;
        lbl_moving.SetActive(false);
        lbl_currentlyEquipping.SetActive(false);
        lbl_selectAnOpenTile.SetActive(false);
    }
    
    public void OpenTeamGrid()
    {
        SaverService.Instance.GetSaverAndData<CharactersData_SavedDataSO>(out _, out var charData);
        foreach (var sqaure in allSquares)
        {
            var row = sqaure.row;
            var col = sqaure.col;
            Debug.Log("Team Grid Setting Tile: " + row + ", " + col + "");
            var match = charData.charactersData.FirstOrDefault(c => c.row == row && c.col == col);
            if (match == null || !match.hasCharacter) continue;
            Debug.Log("Team Grid Found Match: " + match.characterConfigName + "");
            var potentialCharacter = match.characterConfigName;
            var matchingPortrait = allPortraits.First(p => p.characterCfg.occupantName == potentialCharacter); // has to have a match
            Debug.Log("Team Grid Found Corrosponding Char Portrait: " + matchingPortrait.characterCfg.occupantName + "");
            matchingPortrait.UpdateState(CharacterSelectPortrait.CharacterSelectPortraitState.Equipped);
            sqaure.currentlyEquippedPortrait = matchingPortrait;
            sqaure.UpdateSquareState(CharacterSelectPortraitSquareState.Unselected);
        }
    }

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
