using System.Collections.Generic;
using UnityEngine;

public class SpatialHashGrid : MonoBehaviour
{
    public static SpatialHashGrid Instance { get; private set; }

    [Header("Grid Settings")]
    [Tooltip("The length of one side of the grid ")]
    [SerializeField] private float _cellSize = 4;

    [Header("Debug")]
    [Tooltip("Turn on to display the grids and the number of enemies on screen")]
    [SerializeField] private bool _drawDebugGizmos = false;

    //HashTable: Cell's coordinate -> Queue of Enemies inside the cell
    private readonly Dictionary<Vector2Int, List<EnemyCore>> _cells = new Dictionary<Vector2Int, List<EnemyCore>>();

    //Consulting table: Enemy -> Cell's Coordinate (for Updating, Unregisting with O(1) complexity
    private readonly Dictionary<EnemyCore, Vector2Int> _entityCellMap= new Dictionary<EnemyCore, Vector2Int>();

    //Object pool for lists inside cell (prevent 100% GC Alloc when create a new list)
    private readonly Queue<List<EnemyCore>> _listPool = new Queue<List<EnemyCore>>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    //Conver world coordinate to cell coordinate
    private Vector2Int WorldToCell(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x/_cellSize),
            Mathf.FloorToInt(worldPos.y/_cellSize)
            );
    }

    //Register enemy to the grid cell
    public void Register(EnemyCore enemy)
    {
        if (enemy == null) return;
        Vector2Int cell = WorldToCell(enemy.transform.position);
        AddEntityToCell(enemy, cell);
        _entityCellMap[enemy] = cell;
    }

    public void UpdatePosition(EnemyCore enemy)
    {
        if(enemy == null) return;
        if(!_entityCellMap.TryGetValue(enemy, out Vector2Int oldCell))
        {
            Register(enemy);
            return;
        }

        Vector2Int newCell = WorldToCell(enemy.transform.position);

        if (newCell == oldCell) return;
        RemoveEntityFromCell(enemy, oldCell);
        AddEntityToCell(enemy, newCell);
        _entityCellMap[enemy] = newCell;
    }

    //Unregister enemy of cell when the enemy is dead
    public void Unregistered (EnemyCore enemy)
    {
        if(enemy == null) return;

        if(_entityCellMap.TryGetValue(enemy, out Vector2Int cell))
        {
            RemoveEntityFromCell(enemy, cell);
            _entityCellMap.Remove(enemy);
        }
    }

    //Spartial Hashing
    public void QueryRadius(Vector2 center, float radius, List<EnemyCore> results)
    {
        results.Clear();

        int minX = Mathf.FloorToInt((center.x - radius) / _cellSize);
        int maxX = Mathf.FloorToInt((center.x + radius) / _cellSize);
        int minY = Mathf.FloorToInt((center.y - radius) / _cellSize);
        int maxY = Mathf.FloorToInt((center.y + radius) / _cellSize);

        float sqrRadius = radius * radius;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if(_cells.TryGetValue(cell, out List<EnemyCore> enemiesInCell))
                {
                    for (int i = 0; i < enemiesInCell.Count; i++)
                    {
                        EnemyCore enemy = enemiesInCell[i];
                        if (enemy != null && enemy.gameObject.activeInHierarchy)
                        {
                            float sqrDist = ((Vector2)enemy.transform.position - center).sqrMagnitude;
                            if(sqrDist < sqrRadius)
                            {
                                results.Add(enemy);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///SOFT SEPARATION: calculate a soft force between enemies to separate them, 
    ///                 so they can't be overlaped to each other
    /// </summary>
    public Vector2 GetSeparationForce(EnemyCore currentEnemy, float separationRadius = 1f)
    {
        if(currentEnemy == null) return Vector2.zero;

        if(!_entityCellMap.TryGetValue(currentEnemy, out Vector2Int cell))
        {
            return Vector2.zero;
        }
        if(!_cells.TryGetValue(cell, out List<EnemyCore> neighbors))
        {
            return Vector2.zero;
        }

        Vector2 separation = Vector2.zero;
        Vector2 currentEnemyPos = currentEnemy.transform.position;
        float sqrRadius = separationRadius * separationRadius;

        for (int i = 0; i < Mathf.Min(neighbors.Count, 8); i++) 
        {
            EnemyCore other = neighbors[i];
            if(other == null || other == currentEnemy || !other.gameObject.activeInHierarchy) continue;

            Vector2 diff = currentEnemyPos - (Vector2)other.transform.position;
            float sqrDist = diff.sqrMagnitude;

            if(sqrDist < sqrRadius && sqrDist > 0.001f)
            {
                float dist = Mathf.Sqrt(sqrDist);
                separation += (diff / dist) * (1 - dist / separationRadius);
            }
        }
        return separation;
    }
    #region CELL POOLING AND INTERNAL HELPERS
    private void AddEntityToCell (EnemyCore enemy, Vector2Int cell)
    {
        if(!_cells.TryGetValue(cell, out List<EnemyCore> enemiesList))
        {
            enemiesList = _listPool.Count > 0 ? _listPool.Dequeue() : new List<EnemyCore>(8);
            _cells[cell] = enemiesList;
        }
        enemiesList.Add(enemy);
    }

    private void RemoveEntityFromCell(EnemyCore enemy, Vector2Int cell)
    {
        if(_cells.TryGetValue(cell, out List<EnemyCore> enemiesList))
        {
            enemiesList.Remove(enemy);
            if(enemiesList.Count == 0)
            {
                _cells.Remove(cell);
                _listPool.Enqueue(enemiesList);
            }
        }
    }
    #endregion

    #region DEBUG GIZMOS
    private void OnDrawGizmos()
    {
        if(!_drawDebugGizmos) return;
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.2f);
        foreach(var pair in _cells)
        {
            Vector3 cellCenter = new Vector3((pair.Key.x + 0.5f) * _cellSize, (pair.Key.y + 0.5f) * _cellSize, 0f);
            Gizmos.DrawWireCube(cellCenter, new Vector3(_cellSize, _cellSize, 0.1f));
        }
    }
    #endregion
}
