using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class GridWorld : MonoBehaviour
{
    [SerializeField, HideInInspector] Vector2 _size;
    [SerializeField, HideInInspector] float _xGap = 1f;
    [SerializeField, HideInInspector] float _zGap = 1f;

    [ShowInInspector, PropertyOrder(-1)]
    public Vector2 size
    {
        get => _size;
        set
        {
            _size = value;
            GenerateGrid(true);
        }
    }

    [ShowInInspector, PropertyOrder(-1)]
    public float xGap
    {
        get => _xGap;
        set
        {
            _xGap = value;
            GenerateGrid();
        }
    }

    [ShowInInspector, PropertyOrder(-1)]
    public float zGap
    {
        get => _zGap;
        set
        {
            _zGap = value;
            GenerateGrid();
        }
    }

    [Required] public GameObject tilePrefab;
    [ReadOnly] public List<BattleTileGridRow> gridRows = new List<BattleTileGridRow>(3);

    [Serializable]
    public class BattleTileGridRow
    {
        [ReadOnly] public List<BattleTile> tiles = new List<BattleTile>(7);
    }




    [Button]
    public void GenerateGrid(bool destroyExisting = true)
    {
        if (destroyExisting)
        {
            gridRows.Clear();
            transform.Children().DestroyAll();
        }

        for (int r = 0; r < size.y; r++)
        {
            gridRows.Add(new BattleTileGridRow());
            for (int c = 0; c < size.x; c++)
            {
                var tile = Instantiate(
                        tilePrefab,
                        transform.position + new Vector3(c * xGap, 0, -r * zGap),
                        Quaternion.identity,
                        transform)
                    .Get<BattleTile>();

                tile.Init(c, r);
                gridRows[r].tiles.Add(tile);
                tile.transform.parent = transform;
            }

        }
    }

    public void PopulateGrid(BattleConfig battleConfig)
    {
        int populationCount = 0;
        
        for (int c = 0; c < battleConfig.rows.Length; c++)
        {
            for (int r = 0; r < battleConfig.rows[c].colPositions.Count; r++)
            {
                var occupant = battleConfig.rows[c].colPositions[r].occupant;
                if (occupant == null) continue;
                populationCount++;
                Debug.Log("[BattleConfig] Occupant found at: " + c + ", " + r + " = " + occupant + ", Populating...");
                gridRows[c].tiles[r].Init(c, r, occupant);
                gridRows[c].tiles[r].Unhide();
                Debug.Log("[BattleConfig] Population complete");
            }
        }
        
        Debug.Log("[BattleConfig] Population complete, " + populationCount + " tiles populated");
    }

    public void UpdateGrid()
    {
        foreach (var t in gridRows.SelectMany(r => r.tiles))
            t.UpdateTransientStats();
    }


    public void HideUnoccupiedTiles_GetAvaliableTargetTiles(BattleConfig battleConfig,
        BattlemodeActionConfig.Role actionTarget,
        OccupantCfg queuedOccupant,
        out List<BattleTile> avaliableTargetsTiles)
    {
        avaliableTargetsTiles = new List<BattleTile>();
        for(int c = 0; c < battleConfig.rows.Length; c++)
        {
            for(int r = 0; r < battleConfig.rows[c].colPositions.Count; r++)
            {
                var battlePosData = battleConfig.rows[c].colPositions[r];
                var cfgOccupiedPos = battlePosData.occupant;
                var tile = gridRows[c].tiles[r];
                if (cfgOccupiedPos == null || tile.occupantCtx == null)
                {
                    tile.HideAndMakeUnSelectable();
                    continue;
                }
                    
                avaliableTargetsTiles.Add(tile);
                tile.ShowOnly(actionTarget, queuedOccupant);
            }
        }
    }

    public void GetBattlerTiles(out List<BattleTile> opponentTiles, out List<BattleTile> playerTiles)
    {
        opponentTiles = new List<BattleTile>();
        playerTiles = new List<BattleTile>();
        
        foreach (var t in gridRows.SelectMany(r => r.tiles))
            switch (t.occupantCtx)
            {
                case null: continue;
                case EnemyOccupantCtx: opponentTiles.Add(t); break;
                case CharacterOccupantCtx: playerTiles.Add(t); break;
            }
        
        if(opponentTiles.Count == 0) Debug.LogError("No opponent tiles found");
        if(playerTiles.Count == 0) Debug.LogError("No player tiles found");
    }
    
    public void UnSelectAll()
    {
        foreach (var t in gridRows.SelectMany(r => r.tiles))
            t.Unhide();
    }

    public struct InRangeCheckCtx
    {
        public int myCol;
        public int myRow;
        
        public int upRange;
        public int downRange;
        public int fwdRange;
    }
    
    public void ShowInRangeTiles(InRangeCheckCtx ctx, List<BattleTile> avaliableTargetsTiles)
    {
        List<BattleTile> tilesInRange = new();

        foreach (var targetTile in avaliableTargetsTiles)
        {
            // targ on the same row
            if (ctx.myRow == targetTile.row && ctx.fwdRange > 0)
            {
                for (int sameRowCheckCol = ctx.myCol + 1;
                     sameRowCheckCol < gridRows[ctx.myRow].tiles.Count;
                     sameRowCheckCol++)
                {
                    Debug.Log("Checking: " + sameRowCheckCol);
                    
                    var distDifference = Mathf.Abs(sameRowCheckCol - ctx.myCol);

                    if (distDifference > ctx.fwdRange)
                    {
                        gridRows[ctx.myRow].tiles[sameRowCheckCol].NotInRange();
                        continue;
                    }
                    
                    tilesInRange.Add(gridRows[ctx.myRow].tiles[sameRowCheckCol]);
                }
            }
            
            //up 1 row
            // is not at the top
            if (ctx.myRow > 0 && ctx.upRange > 0)
            {
                // targ on the row above
                if (ctx.myRow - 1 == targetTile.row - 1)
                {
                    for (int upRowCheckCol = ctx.myCol;
                         upRowCheckCol < gridRows[ctx.myRow - 1].tiles.Count;
                         upRowCheckCol++)
                    {
                        var distDifference = Mathf.Abs(upRowCheckCol - ctx.myCol);
                        if (distDifference >= ctx.upRange)
                        {
                            gridRows[ctx.myRow - 1].tiles[upRowCheckCol].NotInRange();
                            continue;
                        }
                        tilesInRange.Add(gridRows[ctx.myRow - 1].tiles[upRowCheckCol]);
                    }
                }
            }
                
            //down 1 row
            // is not at the bottom
            if (ctx.myRow < gridRows.Count - 1 && ctx.downRange > 0)
            {
                // targ on the row bellow
                if (ctx.myRow + 1 == targetTile.row + 1)
                {
                    for (int downRowCheckCol = ctx.myCol;
                         downRowCheckCol < gridRows[ctx.myRow + 1].tiles.Count;
                         downRowCheckCol++)
                    {
                        var distDifference = Mathf.Abs(downRowCheckCol - ctx.myCol);
                        if (distDifference >= ctx.downRange)
                        {
                            gridRows[ctx.myRow + 1].tiles[downRowCheckCol].NotInRange();
                            continue;
                        }
                        tilesInRange.Add(gridRows[ctx.myRow + 1].tiles[downRowCheckCol]);
                    }
                }
                
            }
        }
        
        
        foreach (var t in tilesInRange)
            t.InRange();
    }
}
