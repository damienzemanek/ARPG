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
    

    public List<BattleTile> GetAvaliableTargetTiles(
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


    public BattleTile GetNextMoveTile(
        BattleTile start,
        BattleTile end,
        InRangeCheckCtx rangeCtx)
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

            // We aren't trying to reach the target anymore.
            // We are trying to find the closest tile from which
            // the target can be attacked.
            if (IsTargetInRange(current, end))
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

                // Every occupied tile is an obstacle.
                // We are looking for an attack position, not the target itself.
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


        bool IsTargetInRange(BattleTile attackerTile, BattleTile targetTile)
        {
            int rowDifference = targetTile.row - attackerTile.row;
            int colDifference = attackerTile.col - targetTile.col;

            // Target is behind us.
            // Enemy AI attacks from right -> left, so the target
            // must be on the same column or to our left.
            if (colDifference < 0)
                return false;

            switch (rowDifference)
            {
                // Same row
                case 0:
                    return colDifference <= rangeCtx.fwdRange;

                // Target is one row above.
                // In your grid, row - 1 is up.
                case -1:
                    return colDifference < rangeCtx.upRange;

                // Target is one row below.
                case 1:
                    return colDifference < rangeCtx.downRange;

                default:
                    return false;
            }
        }


        IEnumerable<BattleTile> GetNeighbors(BattleTile tile)
        {
            // Left = forward for enemies.
            if (tile.col > 0)
                yield return gridRows[tile.row].tiles[tile.col - 1];

            // Right = backwards for enemies.
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

    public bool IsInRange(InRangeCheckCtx ctx, BattleTile tile, out int horizDist, out int vertDist)
    {
        Debug.Log("[Row Check] " + tile.row + " - " + ctx.myRow);
        Debug.Log("[Col Check] " + tile.col + " - " + ctx.myCol);
        vertDist = tile.row - ctx.myRow;
        horizDist = Mathf.Abs(tile.col - ctx.myCol);
        var ret = false;
        switch (vertDist)
        {
            case 0: ret = horizDist <= ctx.fwdRange; break; // Same row
            case 1: ret = horizDist < ctx.upRange; break; // Up
            case -1: ret = horizDist < ctx.downRange; break;// Down
        }
        vertDist = Mathf.Abs(vertDist);
        return ret;
    }

    public BattleTile GetClosestTargetTile(InRangeCheckCtx ctx, BattlemodeActionConfig.Role actionTarget, OccupantCfg compareCfg)
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
    
    public List<BattleTile> GetInRangeTiles(InRangeCheckCtx ctx, BattlemodeActionConfig.Role actionTarget, bool reverse)
    {
        List<BattleTile> tilesInRange = new();

        for (int row = 0; row < gridRows.Count; row++)
        {
            if (row == ctx.myRow)
                ProcessRow(gridRows[row].tiles, ctx.myCol, ctx.fwdRange, 
                                    true, 
                                  reverse, tilesInRange);

            // Row above
            if (ctx.myRow > 0)
            {
                if (row == ctx.myRow - 1) 
                    ProcessRow(gridRows[row].tiles, ctx.myCol, ctx.upRange,
                                        false,
                                         reverse, tilesInRange);

                for (int i = ctx.myRow - 2; i >= 0; i--)
                    gridRows[i].tiles.ForEach(t => t.NotInRange());
            }

            // Row below
            if (ctx.myRow < gridRows.Count - 1)
            {
                if (row == ctx.myRow + 1)
                    ProcessRow(gridRows[row].tiles, ctx.myCol, ctx.downRange,
                                            false,
                                            reverse, tilesInRange);

                for (int i = ctx.myRow + 2; i < gridRows.Count; i++)
                    gridRows[i].tiles.ForEach(t => t.NotInRange());
            }
        }

        return tilesInRange;
        
        void ProcessRow(List<BattleTile> tiles, int myCol, int range, bool inclusive, bool reverse, List<BattleTile> tilesInRange)
        {
            if (range <= 0) { tiles.ForEach(t => t.NotInRange()); return; }

            int start = reverse ? (tiles.Count - 1) : (0);
            int end = reverse   ? (-1)              : (tiles.Count);
            int step = reverse  ? (-1)              : (1);

            for (int col = start; col != end; col += step)
            {
                int diff = Mathf.Abs(col - myCol);
                bool inRange = inclusive ? (diff <= range) : (diff < range);

                if (inRange) tilesInRange.Add(tiles[col]);
                else tiles[col].NotInRange();
            }
        }
    }
}
