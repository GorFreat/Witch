using System.Security.Cryptography;
using UnityEngine;

public class MapChunk : MonoBehaviour
{
    //Chunk coordinate in grid (ex: (0,1), (1,1)
    public Vector2Int chunkCoord { get; private set; }

    //Active chunk and put in the world position
    public void InitializeChunk(Vector2Int coord, float chunkSize)
    {
        chunkCoord = coord;
        transform.position = new Vector3((coord.x + 0.5f) * chunkSize, (coord.y + 0.5f) * chunkSize, 0);
        gameObject.SetActive(true);
    }

    //Deactive chunk and put into the Pool
    public void DeactiveChunk()
    {
        gameObject.SetActive(false);
    }
}