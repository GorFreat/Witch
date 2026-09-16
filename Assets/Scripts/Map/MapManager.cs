using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Player Tracking")]
    [Tooltip("Player transform for map tracking")]
    [SerializeField] private Transform playerTransform;

    [Header("Chunk Settings")]
    [Tooltip("Size of the chunk")]
    [SerializeField] private float chunkSize = 20f;

    [Tooltip("Chunk radius (1 = grid 3x3, 2 = grid 5x5")]
    [SerializeField] private int viewDistance = 1;

    [Header("Prefabs")]
    [Tooltip("Chunk prefab (tilemap)")]
    [SerializeField] private GameObject chunkPrefab;

    // Mnagager chunk with the Vector2Int for chunk coordinate and MapChunk for chunk
    private Dictionary<Vector2Int, MapChunk> activeChunks = new Dictionary<Vector2Int, MapChunk>();

    // Object Pool for saving chunk and reuse
    private Queue<MapChunk> chunkPool = new Queue<MapChunk>();

    // Current chunk coordinate that player is onto
    private Vector2Int currentChunkCoord;


    private void Start()
    {
        if(playerTransform == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            playerTransform = player.transform;
        }
        if (playerTransform != null)
        {
            currentChunkCoord = GetChunkCoordinateFromPosition(playerTransform.position);
            UpdateLoadedChunks();
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;
        Vector2Int playerCoord = GetChunkCoordinateFromPosition(playerTransform.position);
        // Chỉ cập nhật khi người chơi bước sang ô chunk mới
        if (playerCoord != currentChunkCoord)
        {
            currentChunkCoord = playerCoord;
            UpdateLoadedChunks();
        }
    }
    private void UpdateLoadedChunks()
    {
        HashSet<Vector2Int> visibleCoord = new HashSet<Vector2Int>();

        for (int xOffset = -viewDistance; xOffset <= viewDistance; xOffset++)
        {
            for (int yOffset = -viewDistance; yOffset <= viewDistance; yOffset++)
            {
                visibleCoord.Add(new Vector2Int(currentChunkCoord.x + xOffset, currentChunkCoord.y + yOffset));
            }
        }

        List<Vector2Int> coordsToRemove = new List<Vector2Int>();
        foreach(var kvp in activeChunks)
        {
            if (!visibleCoord.Contains(kvp.Key))
            {
                kvp.Value.DeactiveChunk();
                chunkPool.Enqueue(kvp.Value);
                coordsToRemove.Add(kvp.Key);
            }
        }

        foreach(var coord in coordsToRemove)
        {
            activeChunks.Remove(coord);
        }

        foreach(var coord in visibleCoord)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                MapChunk chunk = GetOrCreateChunk();
                chunk.InitializeChunk(coord, chunkSize);
                activeChunks.Add(coord, chunk);
            }
        }
    }
    private MapChunk GetOrCreateChunk()
    {
        //Get chunk from chunk pool to reuse
        if(chunkPool.Count > 0)
        {
            return chunkPool.Dequeue();
        }

        //Instantiate new chunk if there are no chunks in chunk pool
        GameObject obj = Instantiate(chunkPrefab, transform);
        return obj.GetComponent<MapChunk>();
    }
    private Vector2Int GetChunkCoordinateFromPosition(Vector3 position)
    {
        //Convert world coordinate into chunks coordinate
        int coordX = Mathf.FloorToInt(position.x / chunkSize);
        int coordY = Mathf.FloorToInt(position.y / chunkSize);
        return new Vector2Int(coordX, coordY);
    }
    // Draw chunk grid on the scene view
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        foreach (var kvp in activeChunks)
        {
            Vector3 center = new Vector3((kvp.Key.x + 0.5f) * chunkSize, (kvp.Key.y + 0.5f) * chunkSize, 0);
            Gizmos.DrawWireCube(center, new Vector3(chunkSize, chunkSize, 0.1f));
        }
    }
}