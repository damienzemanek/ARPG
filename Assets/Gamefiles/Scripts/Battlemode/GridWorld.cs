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
                t.section = BattlefieldSection.Right;

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
        TargetingCfg targetingCfgInstance,
        bool reverse)
    {
        if (start == null) { Debug.LogWarning("[BFS] Start is null."); return null; }
        if (end == null) { Debug.LogWarning("[BFS] End is null."); return null; }
        if (start == end) { Debug.Log("[BFS] Start and end are the same tile. No movement required."); return null; }

        var queue = new Queue<BattleTile>();
        var visited = new HashSet<BattleTile>();
        var firstMove = new Dictionary<BattleTile, BattleTile>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == end) return firstMove[current];
            foreach (var neighbor in GetNeighbors(current))
            {
                if (visited.Contains(neighbor)) continue;
                if (neighbor.occupied && neighbor != end) continue; // Occupied tiles are obstacles.
                if (neighbor.colRank is BattleTile.ColRank.Center or BattleTile.ColRank.Left1 or BattleTile.ColRank.Left2 or BattleTile.ColRank.Left3) 
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
            $"[BFS] No attack position found from [{start.row},{start.col}] " +
            $"against [{end.row},{end.col}].");

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
    public bool IsInRange(TargetingCfg targetingCfgInstance, BattleTile actingTile, BattleTile targetTile) 
        => IsActionUsable(targetingCfgInstance, actingTile) && IsTargetTargettable(targetingCfgInstance, targetTile);

    public bool IsActionUsable(TargetingCfg targetingCfgInstance, BattleTile actingTile)
    {
        bool colUsable = targetingCfgInstance.usableInColRanks.HasFlag(actingTile.colRank) || targetingCfgInstance.targetCurrentCol;
        bool rowUsable = targetingCfgInstance.usableInRowRanks.HasFlag(actingTile.rowRank) || targetingCfgInstance.targetCurrentRow;
        return colUsable || rowUsable;
    }

    public bool IsTargetTargettable(TargetingCfg targetingCfgInstance, BattleTile targetTile)
    {
        bool colTargeted = targetingCfgInstance.targetColRanks.HasFlag(targetTile.colRank);
        bool rowTargeted = targetingCfgInstance.targetRowRanks.HasFlag(targetTile.rowRank);
        return colTargeted || rowTargeted;
    }
    
    // Actor to Target
    public List<BattleTile> GetInRangeTiles(TargetingCfg targetingCfgInstance, BattleTile actingTile)
    {
        var inRangeTiles = new List<BattleTile>();
        var allTiles = GetAllTiles();

        foreach (var t in allTiles)
            t.HideAndMakeUnSelectable();
        
                       
        if (targetingCfgInstance.usableInRowRanks.HasFlag(actingTile.rowRank))
        {
            var targetRanks = targetingCfgInstance.targetRowRanks;

            inRangeTiles.AddRange(
                allTiles.Where(t => targetRanks.HasFlag(t.rowRank)));
        }

                
        if (targetingCfgInstance.usableInColRanks.HasFlag(actingTile.colRank))
        {
            var targetRanks = targetingCfgInstance.targetColRanks;

            inRangeTiles.AddRange(
                allTiles.Where(t => targetRanks.HasFlag(t.colRank)));
        }
    
        if (targetingCfgInstance.targetCurrentCol)
            inRangeTiles.AddRange(allTiles.Where(potentialCurrentColTile => potentialCurrentColTile.col == actingTile.col));
        
        if (targetingCfgInstance.targetCurrentRow)
            inRangeTiles.AddRange(allTiles.Where(potentialCurrentRowTile => potentialCurrentRowTile.row == actingTile.row));
        
        Debug.Log("[Grid] GetInRangeTiles() Found: " + string.Join(", ", inRangeTiles.Select(t => $"[{t.row}, {t.col}]")));
        
        int row = actingTile.row;
        int col = actingTile.col;
        // Movement Patterns
        switch (targetingCfgInstance.targetingPatternAdditive)
        {
            case TargetingPattern.None: break;
            case TargetingPattern.Self:
                    inRangeTiles.Add(actingTile);
                break;
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
        
        
        return inRangeTiles;
    }

    public enum BattlefieldSection
    {
        Left, 
        Contested,
        Right,
        OutOfBounds,
    }
    
    public void HideAllDisplays() => GetAllTiles().ForEach(t => t.HideDisplay());
    public void ClearAllTiles() => GetAllTiles().ForEach(t => t.Clear());
    

    public List<BattleTile> ExcludeSection(BattlefieldSection excludeSection, List<BattleTile> inRangeTiles)
    {
        var ret = new List<BattleTile>(inRangeTiles);
        if (ret == null) throw new ArgumentNullException(nameof(ret));
        

        if (excludeSection == BattlefieldSection.Left)
        {
            foreach (var tile in inRangeTiles)
            {
                if(tile.colRank == BattleTile.ColRank.Left1 || 
                   tile.colRank == BattleTile.ColRank.Left2 || 
                   tile.colRank == BattleTile.ColRank.Left3)
                    ret.Remove(tile);
                if(tile.section == BattlefieldSection.OutOfBounds) ret.Remove(tile);
                if(tile.section == excludeSection) ret.Remove(tile);
            }
            
            return ret;
        }

        if (excludeSection == BattlefieldSection.Contested)
        {
            foreach (var tile in inRangeTiles)
            {
                if(tile.colRank == BattleTile.ColRank.Center) ret.Remove(tile);
                if(tile.section == BattlefieldSection.OutOfBounds) ret.Remove(tile);
                if(tile.section == excludeSection) ret.Remove(tile);
            }

            return ret;
        }

        if (excludeSection == BattlefieldSection.Right)
        {
            foreach (var tile in inRangeTiles)
            {
                if(tile.colRank == BattleTile.ColRank.Right1 || 
                   tile.colRank == BattleTile.ColRank.Right2 || 
                   tile.colRank == BattleTile.ColRank.Right3)
                    ret.Remove(tile);
                if(tile.section == BattlefieldSection.OutOfBounds) ret.Remove(tile);
                if(tile.section == excludeSection) ret.Remove(tile);
            }
            return ret;
        }
        
        return ret;
    }
    
    public enum TileDirection { Left, Right, Up, Down, DiagUpLeft, DiagUpRight, DiagDownLeft, DiagDownRight }

    public BattleTile GetTileToThe(TileDirection dir, int row, int col, int amount = 1)
    {
        switch (dir)
        {
            case TileDirection.Left: if (col - amount >= 0) return gridRows[row].tiles[col - amount]; break;
            case TileDirection.Right: if (col + amount < gridRows[row].tiles.Count) return gridRows[row].tiles[col + amount]; break;
            case TileDirection.Up: if (row - amount >= 0) return gridRows[row - amount].tiles[col]; break;
            case TileDirection.Down: if (row + amount < gridRows.Count) return gridRows[row + amount].tiles[col]; break;
            case TileDirection.DiagUpLeft: if (col - amount >= 0 && row - amount >= 0) return gridRows[row - amount].tiles[col - amount]; break;
            case TileDirection.DiagUpRight: if (col + amount < gridRows[row].tiles.Count && row - amount >= 0) return gridRows[row - amount].tiles[col + amount]; break;
            case TileDirection.DiagDownLeft: if (col - amount >= 0 && row + amount < gridRows.Count) return gridRows[row + amount].tiles[col - amount]; break;
            case TileDirection.DiagDownRight: if (col + amount < gridRows[row].tiles.Count && row + amount < gridRows.Count) return gridRows[row + amount].tiles[col + amount]; break;
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
    

    public BattleTile GetClosestTargetTile(
        (int myCol, int myRow) loc,
        Role actionTarget,
        OccupantCfg compareCfg)
    {
        var tiles = GetAllTiles();

        BattleTile closest = null;
        int closestDistance = int.MaxValue;

        foreach (var tile in tiles)
        {
            if (tile.occupied) continue;

            int distance =
                Mathf.Abs(tile.col - loc.myCol) +
                Mathf.Abs(tile.row - loc.myRow);

            if (distance >= closestDistance) continue;
            if (!tile.IsSameRoleTarget(actionTarget, compareCfg)) continue;
            closestDistance = distance;
            closest = tile;
        }

        return closest;
    }
    
    public BattleTile GetClosestUsableTile(
        TargetingCfg targetingCfgInstance,
        (int myCol, int myRow) loc)
    {
        var tiles = GetAllTiles();

        BattleTile closest = null;
        int closestDistance = int.MaxValue;

        foreach (var tile in tiles)
        {
            if (tile.occupied) continue;
            if (!targetingCfgInstance.usableInRowRanks.HasFlag(tile.rowRank)
                && !targetingCfgInstance.usableInColRanks.HasFlag(tile.colRank)) continue;

            int distance =
                Mathf.Abs(tile.col - loc.myCol) +
                Mathf.Abs(tile.row - loc.myRow);

            if (distance >= closestDistance) continue;
            closestDistance = distance;
            closest = tile;
        }

        return closest;
    }
}
