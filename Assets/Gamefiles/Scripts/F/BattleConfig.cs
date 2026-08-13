using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "ARPG/SO/BattleConfig")]
public class BattleConfig : ScriptableObject
{
    [InfoBox("Left is Player side, Right is Enemy Side")]
    [TableList(ShowIndexLabels = false, AlwaysExpanded = true)]
    public Row[] rows = [new(0), new(1), new(2)];


    [Serializable]
    public class Row
    {
        [HideInInspector] public List<BattlePosition> colPositions = new();
        [HideInInspector] public int rowNum;
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos0 { get => colPositions?[0]; set => colPositions[0] = value;}
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos1 { get => colPositions?[1]; set => colPositions[1] = value;}
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos2 { get => colPositions?[2]; set => colPositions[2] = value;}
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos3 { get => colPositions?[3]; set => colPositions[3] = value;}
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos4 { get => colPositions?[4]; set => colPositions[4] = value;}
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos5 { get => colPositions?[5]; set => colPositions[5] = value;}
        [HideLabel] [ShowInInspector] [InlineProperty] public BattlePosition colPos6 { get => colPositions?[6]; set => colPositions[6] = value;}
        public Row(int _rowNum)
        {
            rowNum = _rowNum;
            colPositions.Add(new BattlePosition(0, rowNum));
            colPositions.Add(new BattlePosition(1, rowNum));
            colPositions.Add(new BattlePosition(2, rowNum));
            colPositions.Add(new BattlePosition(3, rowNum));
            colPositions.Add(new BattlePosition(4, rowNum));
            colPositions.Add(new BattlePosition(5, rowNum));
            colPositions.Add(new BattlePosition(6, rowNum));
        }
    }


    [Serializable]
    public class BattlePosition
    {
        public int col, row;
        [PreviewField(nameof(Icon), 50)]
        [HideLabel] public BattlePositionOccupantConfig occupant;
        
        Sprite Icon => occupant != null ? occupant.icon : null;

        public BattlePosition(int _col, int _row)
        {
            col = _col;
            row = _row;
        }
    }
}