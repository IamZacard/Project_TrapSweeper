using System.Collections.Generic;
using UnityEngine;

public class ShardManager : MonoBehaviour
{
    [Header("Shard Settings")]
    [SerializeField] private List<GameObject> shardPrefabs; // List of shard prefabs
    [SerializeField] private Transform shardParent; // Optional, to organize in hierarchy
    [SerializeField] private float spawnChance = .5f;
    private List<GameObject> spawnedShards = new List<GameObject>();

    // Called when a cell is revealed
    public void TrySpawnShard(Cell cell, int gridWidth, int gridHeight)
    {
        // 50% chance to spawn a shard
        if (Random.value <= spawnChance)
        {
            // Spawn shard at a random position within the grid
            Vector2Int spawnPosition = GetRandomPosition(gridWidth, gridHeight);
            if (IsWithinGridBounds(spawnPosition, gridWidth, gridHeight))
            {
                Vector3 worldPosition = new Vector3(spawnPosition.x + 0.5f, spawnPosition.y + 0.5f, 0);
                GameObject randomShardPrefab = shardPrefabs[Random.Range(0, shardPrefabs.Count)];
                GameObject shard = Instantiate(randomShardPrefab, worldPosition, Quaternion.identity, shardParent);
                spawnedShards.Add(shard);
            }
            else
            {
                Debug.LogWarning("Calculated spawn position is out of grid bounds.");
            }
        }
    }

    private Vector2Int GetRandomPosition(int width, int height)
    {
        // Randomly select a position within the grid
        int x = Random.Range(0, width);
        int y = Random.Range(0, height);
        return new Vector2Int(x, y);
    }

    private bool IsWithinGridBounds(Vector2Int position, int gridWidth, int gridHeight)
    {
        return position.x >= 0 && position.x < gridWidth &&
               position.y >= 0 && position.y < gridHeight;
    }

    public void ClearSpawnedShards()
    {
        foreach (GameObject shard in spawnedShards)
        {
            if (shard != null)
            {
                Destroy(shard);
            }
        }
        spawnedShards.Clear();
    }
}
