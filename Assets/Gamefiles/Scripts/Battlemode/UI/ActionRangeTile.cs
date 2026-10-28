using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ActionRangeTile : MonoBehaviour
{
    public enum TileState
    {
        Default,
        InRange,
        Usable,
        Contested,
    }

    public int row, col;
    [Required] public Image image;
    [Required] public Sprite defaultTile;
    [Required] public Sprite inRangeTile;
    [Required] public Sprite usableTile;
    [Required] public Sprite contestedTile;
    [Required] public GameObject userIndication;
    
    public TileState state;
    
    void Awake() => ResetTiles();

    public void ResetTiles()
    {
        SetActionRangeTileState(TileState.Default);
        SetUserIndication(false);
    }

    [Button]
    public void SetActionRangeTileState(TileState _state)
    {
        state = _state;
        switch (state)
        {
            case TileState.Default: image.sprite = defaultTile; break;
            case TileState.InRange: image.sprite = inRangeTile; break;
            case TileState.Usable: image.sprite = usableTile; break;
            case TileState.Contested: image.sprite = contestedTile; break;
            default: throw new ArgumentOutOfRangeException();
        }
    }
    
    public void SetUserIndication(bool _value) => userIndication.SetActive(_value);
    
    
    
}
