using System;
using System.Collections.Generic;
using System.Linq;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using static BattlemodeActionConfig;

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

    public void DesignateTileRanks()
    {
        var tiles = GetAllTiles();
        foreach (var t in tiles)
        {
            switch (t.col)
            {
                case 0: t.colRank = BattleTile.ColRank.Left3; break;
                case 1: t.colRank = BattleTile.ColRank.Left2; break;
                case 2: t.colRank = BattleTile.ColRank.Left1; break;
                case 3: t.colRank = BattleTile.ColRank.Center; break;
                case 4: t.colRank = BattleTile.ColRank.Right1; break;
                case 5: t.colRank = BattleTile.ColRank.Right2; break;
                case 6: t.colRank = BattleTile.ColRank.Right3; break;
            }

            if (t.colRank == BattleTile.ColRank.Center)
                t.section = BattlefieldSection.Contested;
            else if (t.colRank == BattleTile.ColRank.Left1 || t.colRank == BattleTile.ColRank.Left2 || t.colRank == BattleTile.ColRank.Left3)
                t.section = BattlefieldSection.Left;
            else if (t.colRank == BattleTile.ColRank.Right1 || t.colRank == BattleTile.ColRank.Right2 || t.colRank == BattleTile.ColRank.Right3)

            switch (t.row)
            {
                case 0: t.rowRank = BattleTile.RowRank.Top; break;
                case 1: t.rowRank = BattleTile.RowRank.Middle; break;
                case 2: t.rowRank = BattleTile.RowRank.Bottom; break;
            }
            
            t.@DefaultOrContestedSetActive(true);
        }
        
    }
    
    public List<BattleTile> GetAllTiles() => gridRows.SelectMany(r => r.tiles).ToList();

    public void UpdateGrid() => GetAllTiles().ForEach(t => t.UpdateTransientStats());
    

    public List<BattleTile> GetAvaliableTargetTiles(
        Role myTarget,
        List<BattleTile> inRangeTiles,
        BattleTile myTile)
    {
        var ret = new List<BattleTile>(inRangeTiles);
        foreach (var tile in inRangeTiles)
        {
            switch (myTarget)
            {
                case Role.Enemy: 
                    if (!tile.IsEnemy())
                    {
                        tile.SetUnselectable(true);
                        ret.Remove(tile);
                    }
                    break;
                case Role.Self:
                    if (!tile.IsSelf(myTile.occupantCtx.cfg))
                    {
                        tile.SetUnselectable(true);
                        ret.Remove(tile);
                    }
                    break;
                case Role.Ally:
                    if (!tile.IsAlly(myTile.occupantCtx.cfg))
                    {
                        ret.Remove(tile);
                        tile.SetUnselectable(true);
                    }
                    break;
                case Role.EmptyTile: 
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


    public BattleTile GetNextMoveTile(
        BattleTile start,
        BattleTile end,
        InRangeCheckCtx rangeCtx,
        bool reverse)
    {
        if (start == null)
        {
            Debug.LogWarning("[BFS] Start is null.");
            return null;
        }

        if (end == null)
        {
            Debug.LogWarning("[BFS] End is null.");
            return null;
        }

        if (start == end)
        {
            Debug.Log("[BFS] Start and end are the same tile. No movement required.");
            return null;
        }

        var queue = new Queue<BattleTile>();
        var visited = new HashSet<BattleTile>();
        var firstMove = new Dictionary<BattleTile, BattleTile>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // Find the closest tile from which the target can be attacked.
            if (IsTargetInRange(rangeCtx, current, end))
            {
                var result = current == start
                    ? null
                    : firstMove[current];

                Debug.Log(
                    $"[BFS] Found attack position [{current.col},{current.row}] " +
                    $"for target [{end.col},{end.row}]. " +
                    $"Next move = [{result?.col},{result?.row}]");

                return result;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor))
                    continue;

                // Occupied tiles are obstacles.
                if (neighbor.occupied)
                    continue;

                visited.Add(neighbor);

                firstMove[neighbor] =
                    current == start
                        ? neighbor
                        : firstMove[current];

                queue.Enqueue(neighbor);
            }
        }

        Debug.LogWarning(
            $"[BFS] No attack position found from [{start.col},{start.row}] " +
            $"against [{end.col},{end.row}].");

        return null;

        
        IEnumerable<BattleTile> GetNeighbors(BattleTile tile)
        {
            // Left
            if (tile.col > 0)
                yield return gridRows[tile.row].tiles[tile.col - 1];

            // Right
            if (tile.col < gridRows[tile.row].tiles.Count - 1)
                yield return gridRows[tile.row].tiles[tile.col + 1];

            // Up
            if (tile.row > 0)
                yield return gridRows[tile.row - 1].tiles[tile.col];

            // Down
            if (tile.row < gridRows.Count - 1)
                yield return gridRows[tile.row + 1].tiles[tile.col];
        }
    }
    
    // Target to Actor
    public bool IsTargetInRange(InRangeCheckCtx rangeCtx, BattleTile actingTile, BattleTile targetTile)
    {
        bool colUsable = rangeCtx.targetingCfg.usableInColRanks.HasFlag(actingTile.colRank) || rangeCtx.targetingCfg.targetCurrentCol;
        bool colTargeted = rangeCtx.targetingCfg.targetColRanks.HasFlag(targetTile.colRank);
        bool rowUsable = rangeCtx.targetingCfg.usableInRowRanks.HasFlag(actingTile.rowRank) || rangeCtx.targetingCfg.targetCurrentRow;
        bool rowTargeted = rangeCtx.targetingCfg.targetRowRanks.HasFlag(targetTile.rowRank);
        return (colUsable && colTargeted) || (rowUsable && rowTargeted);
    }
    
    // Actor to Target
    public List<BattleTile> GetInRangeTiles(InRangeCheckCtx rangeCtx, BattleTile actingTile)
    {
        var inRangeTiles = new List<BattleTile>();
        var allTiles = GetAllTiles();

        foreach (var t in allTiles)
            t.HideAndMakeUnSelectable();
        
                
        if (rangeCtx.targetingCfg.usableInColRanks.HasFlag(actingTile.colRank))
        {
            var targetRanks = rangeCtx.targetingCfg.targetColRanks;

            inRangeTiles.AddRange(
                allTiles.Where(t => targetRanks.HasFlag(t.colRank)));
        }
    
        if (rangeCtx.targetingCfg.targetCurrentCol)
            inRangeTiles.AddRange(allTiles.Where(potentialCurrentColTile => potentialCurrentColTile.col == actingTile.col));
        
        if (rangeCtx.targetingCfg.targetCurrentRow)
            inRangeTiles.AddRange(allTiles.Where(potentialCurrentRowTile => potentialCurrentRowTile.row == actingTile.row));
        
        foreach (var t in inRangeTiles)
            Debug.Log("Found: [" + t.row + ", " + t.col + "]");

        
        int row = actingTile.row;
        int col = actingTile.col;
        // Movement Patterns
        switch (rangeCtx.targetingCfg.targetingPatternAdditive)
        {
            case TargetingPattern.None: break;
            case TargetingPattern.Cross:
                    inRangeTiles.Add(GetTileToThe(TileDirection.Left, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.Right, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.Up, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.Down, row, col));
                break;
            case TargetingPattern.Box:
                    inRangeTiles.Add(GetTileToThe(TileDirection.Left, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.Right, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.Up, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.Down, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.DiagUpLeft, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.DiagUpRight, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.DiagDownLeft, row, col));
                    inRangeTiles.Add(GetTileToThe(TileDirection.DiagDownRight, row, col));
                break;
        }
        
        // Null / Duplicate Handling
        inRangeTiles = inRangeTiles
            .Where(t => t != null)
            .Distinct()
            .ToList();
        
        foreach (var t in inRangeTiles)
            t.InRange();

        
        return inRangeTiles;
    }

    public enum BattlefieldSection
    {
        Left, 
        Contested,
        Right,
        OutOfBounds,
    }

    public List<BattleTile> ExcludeSection(BattlefieldSection section, List<BattleTile> inRangeTiles)
    {
        var ret = new List<BattleTile>(inRangeTiles);
        if (ret == null) throw new ArgumentNullException(nameof(ret));
        

        if (section == BattlefieldSection.Left)
        {
            foreach (var tile in inRangeTiles)
            {
                if(tile.colRank == BattleTile.ColRank.Left1 || 
                   tile.colRank == BattleTile.ColRank.Left2 || 
                   tile.colRank == BattleTile.ColRank.Left3)
                    ret.Remove(tile);
                if(tile.section == BattlefieldSection.OutOfBounds) ret.Remove(tile);
            }
            
            return ret;
        }

        if (section == BattlefieldSection.Contested)
        {
            foreach (var tile in inRangeTiles)
            {
                if(tile.colRank == BattleTile.ColRank.Center) ret.Remove(tile);
                if(tile.section == BattlefieldSection.OutOfBounds) ret.Remove(tile);
            }

            return ret;
        }

        if (section == BattlefieldSection.Right)
        {
            foreach (var tile in inRangeTiles)
            {
                if(tile.colRank == BattleTile.ColRank.Right1 || 
                   tile.colRank == BattleTile.ColRank.Right2 || 
                   tile.colRank == BattleTile.ColRank.Right3)
                    ret.Remove(tile);
                if(tile.section == BattlefieldSection.OutOfBounds) ret.Remove(tile);
            }
            return ret;
        }
        
        return ret;
    }
    
    public enum TileDirection { Left, Right, Up, Down, DiagUpLeft, DiagUpRight, DiagDownLeft, DiagDownRight }

    public BattleTile GetTileToThe(TileDirection dir, int row, int col)
    {
        switch (dir)
        {
            case TileDirection.Left: if(col > 0) return gridRows[row].tiles[col - 1]; break;
            case TileDirection.Right: if(col < gridRows[row].tiles.Count - 1) return gridRows[row].tiles[col + 1]; break;
            case TileDirection.Up: if(row > 0) return gridRows[row - 1].tiles[col]; break;
            case TileDirection.Down: if(row < gridRows.Count - 1) return gridRows[row + 1].tiles[col]; break;
            case TileDirection.DiagUpLeft: if(col > 0 && row > 0) return gridRows[row - 1].tiles[col - 1]; break;
            case TileDirection.DiagUpRight: if(col < gridRows[row].tiles.Count - 1 && row > 0) return gridRows[row - 1].tiles[col + 1]; break;
            case TileDirection.DiagDownLeft: if(col > 0 && row < gridRows.Count - 1) return gridRows[row + 1].tiles[col - 1]; break;
            case TileDirection.DiagDownRight: if(col < gridRows[row].tiles.Count - 1 && row < gridRows.Count - 1) return gridRows[row + 1].tiles[col + 1]; break;
        }
        return null;
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
        
        if(opponentTiles.Count == 0) Debug.LogWarning("No opponent tiles found");
        if(playerTiles.Count == 0) Debug.LogWarning("No player tiles found");
    }

    public void RefreshAllApsAndIntentions()
    {
        GetBattlerTiles(out var opponentTiles, out var playerTiles);
        foreach (var t in opponentTiles)
        {
            var enemyOccupantCtx = t.occupantCtx as EnemyOccupantCtx;
            if(enemyOccupantCtx != null)
                enemyOccupantCtx.currentIntentUsageCtx.intentionsAmount = enemyOccupantCtx.enemyCfg.intentions;
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
        foreach (var t in gridRows.SelectMany(r => r.tiles)) t.Unhide();
    }

    public struct InRangeCheckCtx
    {
        public int myCol;
        public int myRow;
        public TargetingCfg targetingCfg;
    }

    public BattleTile GetClosestTargetTile(InRangeCheckCtx ctx, Role actionTarget, OccupantCfg compareCfg)
    {
        var tiles = GetAllTiles();

        BattleTile closest = null;
        int closestDistance = int.MaxValue;

        foreach (var tile in tiles)
        {
            if (tile.occupied) continue;

            int distance =
                Mathf.Abs(tile.col - ctx.myCol) +
                Mathf.Abs(tile.row - ctx.myRow);

            if (distance >= closestDistance) continue;
            if (!tile.IsSameRoleTarget(actionTarget, compareCfg)) continue;
            closestDistance = distance;
            closest = tile;
        }

        return closest;
    }
}
