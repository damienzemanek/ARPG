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
        BattlemodeActionConfig.Role myTarget,
        List<BattleTile> inRangeTiles,
        BattleTile myTile)
    {
        var ret = new List<BattleTile>(inRangeTiles);
        foreach (var tile in inRangeTiles)
        {
            switch (myTarget)
            {
                case BattlemodeActionConfig.Role.Enemy: 
                    if (!tile.IsEnemy())
                    {
                        tile.SetUnselectable(true);
                        ret.Remove(tile);
                    }
                    break;
                case BattlemodeActionConfig.Role.Self:
                    if (!tile.IsSelf(myTile.occupantCtx.cfg))
                    {
                        tile.SetUnselectable(true);
                        ret.Remove(tile);
                    }
                    break;
                case BattlemodeActionConfig.Role.Ally:
                    if (!tile.IsAlly(myTile.occupantCtx.cfg))
                    {
                        ret.Remove(tile);
                        tile.SetUnselectable(true);
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


        bool IsTargetInRange(BattleTile attackerTile, BattleTile targetTile)
        {
            int rowDifference = targetTile.row - attackerTile.row;

            // Calculate distance in the attacker's forward direction.
            int horizDist = reverse
                ? attackerTile.col - targetTile.col
                : targetTile.col - attackerTile.col;

            // Target is behind the attacker.
            if (horizDist < 0) return false;

            Vector2 range;

            switch (rowDifference)
            {
                case 0:
                    range = rangeCtx.fwdRange;
                    break;

                case -1:
                    range = rangeCtx.upRange;
                    break;

                case 1:
                    range = rangeCtx.downRange;
                    break;

                default:
                    return false;
            }

            return horizDist >= range.x && horizDist <= range.y;
        }


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
        foreach (var t in gridRows.SelectMany(r => r.tiles))
            t.Unhide();
    }

    public struct InRangeCheckCtx
    {
        public int myCol;
        public int myRow;
        
        public Vector2 upRange;
        public Vector2 downRange;
        public Vector2 fwdRange;
    }

    public bool OpponentRangeCheck(InRangeCheckCtx ctx, BattleTile tile, bool reverse,
        out int horizDist,
        out int vertDist)
    {
        if(tile == null) Debug.LogError("Tile is Null for OpponentRangeCheck");
        Debug.Log("[Row Check] " + tile.row + " - " + ctx.myRow);
        Debug.Log("[Col Check] " + tile.col + " - " + ctx.myCol);

        int rowDifference = tile.row - ctx.myRow;

        horizDist = Mathf.Abs(tile.col - ctx.myCol);
        vertDist = Mathf.Abs(rowDifference);

        // Determine whether the tile is in front of us.
        int colDifference = tile.col - ctx.myCol;

        // reverse = facing left, otherwise facing right.
        bool isForward = reverse
            ? colDifference < 0
            : colDifference > 0;

        if (!isForward)
            return false;

        Vector2 range;

        switch (rowDifference)
        {
            // Same row.
            case 0:
                range = ctx.fwdRange;
                break;

            // Row above.
            case -1:
                range = ctx.upRange;
                break;

            // Row below.
            case 1:
                range = ctx.downRange;
                break;

            default:
                return false;
        }

        return horizDist >= range.x && horizDist <= range.y;
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
    
    public List<BattleTile> GetInRangeTiles(
        InRangeCheckCtx ctx,
        BattlemodeActionConfig.Role actionTarget,
        bool reverse)
    {
        List<BattleTile> tilesInRange = new();

        for (int row = 0; row < gridRows.Count; row++)
        {
            if (row == ctx.myRow)
                ProcessRow(gridRows[row].tiles, ctx.myCol, ctx.fwdRange, reverse, tilesInRange, excludeOrigin: true);
            else if (row == ctx.myRow - 1)
                ProcessRow(gridRows[row].tiles, ctx.myCol, ctx.upRange, reverse, tilesInRange, upOrDown: true);
            else if (row == ctx.myRow + 1)
                ProcessRow(gridRows[row].tiles, ctx.myCol, ctx.downRange, reverse, tilesInRange, upOrDown: true);
            else
                gridRows[row].tiles.ForEach(t => t.NotInRange());

        }

        return tilesInRange;

        void ProcessRow(
            List<BattleTile> tiles,
            int myCol,
            Vector2 range,
            bool reverse,
            List<BattleTile> result,
            bool excludeOrigin = false,
            bool upOrDown = false)
        {
            int min = Mathf.RoundToInt(range.x);
            int max = Mathf.RoundToInt(range.y);

            int start = reverse ? tiles.Count - 1 : 0;
            int end = reverse ? -1 : tiles.Count;
            int step = reverse ? -1 : 1;

            for (int col = start; col != end; col += step)
            {
                int distance = Mathf.Abs(col - myCol);
                if (excludeOrigin && distance == 0 && actionTarget != BattlemodeActionConfig.Role.Self)
                {
                    tiles[col].NotInRange(); 
                    continue;
                }
                
                bool inRange = upOrDown 
                    ? (distance >= min && distance < max) 
                    : (distance >= min && distance <= max);

                if (inRange)
                    result.Add(tiles[col]);
                else
                    tiles[col].NotInRange();
            }
        }
    }
}
