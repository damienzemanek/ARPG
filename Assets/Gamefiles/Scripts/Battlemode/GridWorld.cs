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


    public void HideNotContaining(BattleConfig battleConfig, BattlemodeActionConfig.Role actionTarget, OccupantCfg queuedOccupant)
    {
        for(int c = 0; c < battleConfig.rows.Length; c++)
        {
            for(int r = 0; r < battleConfig.rows[c].colPositions.Count; r++)
            {
                var battlePosData = battleConfig.rows[c].colPositions[r];
                var cfgOccupiedPos = battlePosData.occupant;
                var tile = gridRows[c].tiles[r];
                if (cfgOccupiedPos == null || tile.occupantCtx == null)
                {
                    tile.HideCompletely();
                    continue;
                }
                    
                tile.ShowOnly(actionTarget, queuedOccupant);
            }
        }
    }

    public void GetTiles(out List<BattleTile> opponentTiles, out List<BattleTile> playerTiles)
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
        {
            t.Clear();
            t.Unhide();
        }
    }
}
