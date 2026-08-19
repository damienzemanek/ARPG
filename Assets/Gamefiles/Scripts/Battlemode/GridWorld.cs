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
        
        for (int row = 0; row < battleConfig.rows.Length; row++)
        {
            for (int col = 0; col < battleConfig.rows[row].colPositions.Count; col++)
            {
                var occupant = battleConfig.rows[row].colPositions[col].occupant;
                if (occupant == null) continue;
                populationCount++;
                Debug.Log("[BattleConfig] Occupant found at: " + row + ", " + col + " = " + occupant + ", Populating...");
                gridRows[row].tiles[col].Init(col, row, occupant);
                gridRows[row].tiles[col].Unhide();
                Debug.Log("[BattleConfig] Population complete");
            }
        }
        
        Debug.Log("[BattleConfig] Population complete, " + populationCount + " tiles populated");
    }
    
    public List<BattleTile> GetAllTiles() => gridRows.SelectMany(r => r.tiles).ToList();

    public void UpdateGrid() => GetAllTiles().ForEach(t => t.UpdateTransientStats());
    

    public List<BattleTile> GetAvaliableTargetTiles(BattleConfig battleConfig,
        BattlemodeActionConfig.Role actionTarget,
        List<BattleTile> inRangeTiles)
    {
        var ret = new List<BattleTile>(inRangeTiles);
        foreach (var tile in inRangeTiles)
        {
            switch (actionTarget)
            {
                case BattlemodeActionConfig.Role.Enemy: 
                    if (!tile.IsEnemy())
                    {
                        ret.Remove(tile);
                    }
                    break;
                case BattlemodeActionConfig.Role.EmptyTile: 
                    if (!tile.IsEmptyTile())
                    {
                        tile.HideAndMakeUnSelectable();
                        ret.Remove(tile);
                    }
                    break;
            }
        }
        return ret;
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

    public void RefreshAllApsAndIntentions()
    {
        GetBattlerTiles(out var opponentTiles, out var playerTiles);
        foreach (var t in opponentTiles)
        {
            var enemyOccupantCtx = t.occupantCtx as EnemyOccupantCtx;
            if(enemyOccupantCtx != null)
                enemyOccupantCtx.currentIntentions = enemyOccupantCtx.intentions;
        }
        foreach (var t in playerTiles)
        {
            var characterOccupantCtx = t.occupantCtx as CharacterOccupantCtx;
            if(characterOccupantCtx != null)
                characterOccupantCtx.currentAP = characterOccupantCtx.maxAP;
        }
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
    
    public List<BattleTile> GetInRangeTiles(InRangeCheckCtx ctx, BattlemodeActionConfig.Role actionTarget)
    {
        List<BattleTile> tilesInRange = new();

        // Same row
        if (ctx.fwdRange > 0)
        {
            var row = gridRows[ctx.myRow].tiles;
            for (int col = 0; col < row.Count; col++)
            {
                int diff = Mathf.Abs(col - ctx.myCol);

                if (diff <= ctx.fwdRange) tilesInRange.Add(row[col]);
                else row[col].NotInRange();
            }
        }

        // Row above
        bool bellowTopRow = ctx.upRange > 0;
        if (ctx.myRow > 0 && bellowTopRow)
        {
            var row = gridRows[ctx.myRow - 1].tiles;
            for (int col = 0; col < row.Count; col++)
            {
                int diff = Mathf.Abs(col - ctx.myCol);

                if (diff < ctx.upRange) tilesInRange.Add(row[col]);
                else row[col].NotInRange();
            }
        }


        // Row below
        bool abouveBottomRow = ctx.downRange > 0;
        if (ctx.myRow < gridRows.Count - 1 && abouveBottomRow)
        {
            var row = gridRows[ctx.myRow + 1].tiles;
            for (int col = 0; col < row.Count; col++)
            {
                int diff = Mathf.Abs(col - ctx.myCol);

                if (diff < ctx.downRange) tilesInRange.Add(row[col]);
                else row[col].NotInRange();
            }
        }

        if (ctx.myRow == 0)
        {
            var bottomRow = gridRows[gridRows.Count - 1].tiles;
            foreach (var tile in bottomRow) tile.NotInRange();
        }

        if (ctx.myRow == gridRows.Count - 1)
        {
            var topRow = gridRows[0].tiles;
            foreach (var tile in topRow) tile.NotInRange();
        }

        return tilesInRange;
    }
}
