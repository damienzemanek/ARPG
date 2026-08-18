using System;
using System.Collections.Generic;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

public class Grid3D : MonoBehaviour
{
    public enum GridAlignment
    {
        Center,
        Left,
        Right
    }
    
    [SerializeField, HideInInspector] float _scale = 1f;
    [SerializeField, HideInInspector] int _x = 1;
    [SerializeField, HideInInspector] int _y = 1;
    [SerializeField, HideInInspector] float _xGap = 1f;
    [SerializeField, HideInInspector] float _zGap = 1f;
    [SerializeField, HideInInspector] GridAlignment _xAlignment = GridAlignment.Center;
    [SerializeField, HideInInspector] GridAlignment _zAlignment = GridAlignment.Center;
    
    
    [ShowInInspector, PropertyOrder(-1)] public GridAlignment xAlignment
    {
        get => _xAlignment;
        set { _xAlignment = value; GenerateGrid(); }
    }

    [ShowInInspector, PropertyOrder(-1)] public GridAlignment zAlignment
    {
        get => _zAlignment;
        set { _zAlignment = value; GenerateGrid(); }
    }
    
    [ShowInInspector, PropertyOrder(-1)] public float scale
    {
        get => _scale;
        set
        {
            _scale = value;
            foreach (Transform child in transform) child.localScale = new Vector3(value, value, value);
        }
    }

    [ShowInInspector, PropertyOrder(-1)] public int x
    {
        get => _x;
        set { _x = value; GenerateGrid(true); }
    }
    
    [ShowInInspector, PropertyOrder(-1)] public int y
    {
        get => _y;
        set { _y = value; GenerateGrid(true); }
    }

    [ShowInInspector, PropertyOrder(-1)] public float xGap
    {
        get => _xGap;
        set { _xGap = value; GenerateGrid(); }
    }

    [ShowInInspector, PropertyOrder(-1)] public float zGap
    {
        get => _zGap;
        set { _zGap = value; GenerateGrid(); }
    }

    [Required] public GameObject tilePrefab;
    [ReadOnly] public List<GridRow> rows;

    [Serializable]
    public class GridRow
    {
        [ReadOnly] public List<GridPos> rowPositions;
    }
    
    [Serializable]
    public struct GridPos
    {
        public bool activeInRow;
        public GameObject created;
        public int x, y;
    }

    public GridRow GetRow(int index)
    {
        var ret = rows[index];
        return ret ?? throw new Exception("Row not found or DNE");
    }
    
    [Button]
    public void GenerateGrid(bool destroyExisting = true)
    {
        if (destroyExisting)
        {
            rows = new List<GridRow>(y);
            foreach (var row in rows) row.rowPositions = new List<GridPos>(x);
            rows.Clear();
            transform.Children().DestroyAll();
        }

        for (int r = 0; r < y; r++)
        {
            rows.Add(new GridRow());
            rows[r].rowPositions = new List<GridPos>(x);

            for (int c = 0; c < x; c++)
            {
                float xOffset = _xAlignment switch
                {
                    GridAlignment.Center => (c - (x - 1) / 2f) * xGap,
                    GridAlignment.Left => c * xGap,
                    GridAlignment.Right => (c - (x - 1)) * xGap,
                    _ => 0f
                };

                float zOffset = _zAlignment switch
                {
                    GridAlignment.Center => -(r - (y - 1) / 2f) * zGap,
                    GridAlignment.Left => -r * zGap,
                    GridAlignment.Right => -(r - (y - 1)) * zGap,
                    _ => 0f
                };

                var tile = Instantiate(
                    tilePrefab,
                    transform.position + new Vector3(xOffset, 0, zOffset),
                    Quaternion.identity,
                    transform);

                rows[r].rowPositions.Add(new GridPos { created = tile, x = c, y = r, activeInRow = true });
                tile.transform.parent = transform;
                tile.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
    
    public void DeactivateInRow(int rowPos)
    {
        if (rowPos < 0 || rowPos >= x)
            throw new ArgumentOutOfRangeException(nameof(rowPos));

        foreach (var row in rows)
        {
            var deactivated = row.rowPositions[rowPos];

            if (!deactivated.activeInRow)
                continue;

            deactivated.activeInRow = false;
            deactivated.created.SetActive(false);

            var active = new List<GridPos>();

            foreach (var gridPos in row.rowPositions)
            {
                if (gridPos.activeInRow)
                    active.Add(gridPos);
            }

            var activeCount = active.Count;

            for (int i = 0; i < activeCount; i++)
            {
                var targetIndex = _xAlignment switch
                {
                    GridAlignment.Left => i,
                    GridAlignment.Right => x - activeCount + i,
                    GridAlignment.Center => (x - activeCount) / 2 + i,
                    _ => i
                };

                var target = row.rowPositions[targetIndex];
                var tile = active[i];

                tile.created.transform.position = target.created.transform.position;
            }
        }
    }
}
