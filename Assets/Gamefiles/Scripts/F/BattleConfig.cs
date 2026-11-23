using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "ARPG/SO/BattleConfig")]
public class BattleConfig : ScriptableObject
{
    const int MAX_ENEMIES = 7 * 3;
    
    BattleConfiguererData configuerer => PersistentConfigurationDataHolder.Instance.battleConfiguererData;
    
    public int experienceGiven = 1;

    public int GetExperience()
    {
        if (amountOfEnemies <= 1) return experienceGiven;

        // Amount of enemies 
        float amountOfEnScaler_ADDITIVE = 1f;
        for (int i = amountOfEnemies; i > 1; i--)
        {
            // this value is usualy something like 1.5, always greater than 1, so we sub -1
            amountOfEnScaler_ADDITIVE += (configuerer.enemyCountExpGainScalarAdditive - 1f);
        }
        
        // Avg level of enemies
        float averageEnemyLevel = (float)sumOfEnemyLevels / amountOfEnemies;
        float percOfMax = (averageEnemyLevel - 1f) / 59f;        
        float enemyLevelsSumScalar = (configuerer.levelExpGainCurve.Evaluate(percOfMax));
        
        
        float finalScalar = amountOfEnScaler_ADDITIVE * enemyLevelsSumScalar;
        
        return Mathf.RoundToInt(experienceGiven * finalScalar);
    }
    
    // the more enemies, the higher the exp given, scaled by battleconfigurer
    [ShowInInspector, ReadOnly]
    public int sumOfEnemyLevels
    {
        get
        {
            int ret = 0;
            foreach (Row row in rows)
            {
                foreach (BattlePosition pos in row.cols)
                    ret += pos.level;
            }
            return ret;
        }
    }
    

    [ShowInInspector, ReadOnly]
    public int amountOfEnemies
    {
        get
        {
            int ret = 0;
            foreach (Row row in rows)
            {
                foreach (BattlePosition pos in row.cols)
                    if(pos.occupant is EnemyConfig) ret++;
            }
            return ret;
        }
    }
    
    [InfoBox("Left is Player side, Right is Enemy Side")]
    [TableList(ShowIndexLabels = false, AlwaysExpanded = true)]
    public Row[] rows = [new(0), new(1), new(2)];

    public int GetLevelOfEnemy(string nameOfEnemy)
    {
        foreach (var r in rows)
        {
            foreach (var pos in r.cols)
            {
                if(pos.occupant is not EnemyConfig ec) continue;
                if(ec.occupantName == nameOfEnemy) return pos.level;
            }
        }
        Debug.LogError("Enemy not found");
        return 1;
    }


    [Serializable]
    public class Row
    {
        public List<BattlePosition> cols => colPositions;
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
        [ShowIf("hasEnemy")] public int level = 1;
        bool hasEnemy => occupant is EnemyConfig;
        
        public int col, row;
        [PreviewField(nameof(Icon), 50)]
        [HideLabel] public OccupantCfg occupant;
        
        Sprite Icon => occupant != null ? occupant.icon : null;

        public BattlePosition(int _col, int _row)
        {
            col = _col;
            row = _row;
        }
    }
}