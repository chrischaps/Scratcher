using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [SerializeField] private int mapWidth = 50;
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private Vector3Int startPosition = Vector3Int.zero;

    [Header("Level Configuration")]
    [SerializeField] private LevelConfiguration levelConfig;

    [Header("Tile Assets")]
    [SerializeField] private NormalTile[] grassTiles;
    [SerializeField] private NormalTile[] stoneTiles;
    [SerializeField] private NormalTile[] sandTiles;
    [SerializeField] private WaterTile[] waterTiles;

    [Header("Generation Parameters")]
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float waterThreshold = 0.3f;
    [SerializeField] private float stoneThreshold = 0.7f;
    [SerializeField] private int smoothingPasses = 2;

    [Header("Target Tilemaps")]
    [SerializeField] private Tilemap baseTerrain;
    [SerializeField] private Tilemap waterLayer;

    [Header("Lake Generation")]
    [SerializeField] private bool generateLakes = true;
    [SerializeField] private int minLakeSize = 3;
    [SerializeField] private int maxLakeSize = 8;
    [SerializeField] private int lakeCount = 3;

    private TerrainLayerManager terrainManager;

#if UNITY_EDITOR
    [ContextMenu("Generate Terrain")]
    public void GenerateTerrain()
    {
        if (baseTerrain == null)
        {
            Debug.LogError("Base terrain tilemap not assigned!");
            return;
        }

        // Apply level configuration if available
        ApplyLevelConfiguration();

        ClearTerrain();
        GenerateBaseTerrain();

        if (generateLakes && waterLayer != null)
        {
            GenerateLakes();
        }

        SmoothTerrain();

        // Refresh terrain manager
        terrainManager = FindObjectOfType<TerrainLayerManager>();
        if (terrainManager != null)
        {
            terrainManager.RefreshAll();
        }

        // Configure player for this level
        ConfigurePlayerForLevel();

        Debug.Log("Terrain generation complete!");
    }

    private void ApplyLevelConfiguration()
    {
        if (levelConfig == null) return;

        // Apply terrain settings from level config
        mapWidth = levelConfig.mapSize.x;
        mapHeight = levelConfig.mapSize.y;
        noiseScale = levelConfig.noiseScale;
        waterThreshold = levelConfig.waterThreshold;
        stoneThreshold = levelConfig.stoneThreshold;
        smoothingPasses = levelConfig.smoothingPasses;

        // Apply lake settings
        generateLakes = levelConfig.generateLakes;
        lakeCount = levelConfig.lakeCount;
        minLakeSize = levelConfig.minLakeSize;
        maxLakeSize = levelConfig.maxLakeSize;

        // Apply tile assets
        if (levelConfig.grassTiles.Length > 0) grassTiles = levelConfig.grassTiles;
        if (levelConfig.stoneTiles.Length > 0) stoneTiles = levelConfig.stoneTiles;
        if (levelConfig.sandTiles.Length > 0) sandTiles = levelConfig.sandTiles;
        if (levelConfig.waterTiles.Length > 0) waterTiles = levelConfig.waterTiles;

        // Apply grid configuration
        Grid grid = GetComponentInParent<Grid>();
        if (grid != null)
        {
            levelConfig.ApplyToGrid(grid);
        }

        Debug.Log($"Applied level configuration: {levelConfig.levelName}");
    }

    private void ConfigurePlayerForLevel()
    {
        if (levelConfig == null) return;

        GridBasedPlayerController player = FindObjectOfType<GridBasedPlayerController>();
        if (player != null)
        {
            player.ApplyLevelConfiguration(levelConfig);
            Debug.Log($"Configured player for {levelConfig.gridConfig.gridType} grid type");
        }
    }

    [ContextMenu("Clear Terrain")]
    public void ClearTerrain()
    {
        if (baseTerrain != null)
        {
            baseTerrain.SetTilesBlock(new BoundsInt(-mapWidth/2, -mapHeight/2, 0, mapWidth, mapHeight, 1), new TileBase[mapWidth * mapHeight]);
        }

        if (waterLayer != null)
        {
            waterLayer.SetTilesBlock(new BoundsInt(-mapWidth/2, -mapHeight/2, 0, mapWidth, mapHeight, 1), new TileBase[mapWidth * mapHeight]);
        }

        // Clear water zones when clearing terrain
        if (terrainManager != null)
        {
            terrainManager.ClearWaterZones();
        }
        else
        {
            terrainManager = FindObjectOfType<TerrainLayerManager>();
            if (terrainManager != null)
            {
                terrainManager.ClearWaterZones();
            }
        }
    }

    private void GenerateBaseTerrain()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int position = startPosition + new Vector3Int(x - mapWidth/2, y - mapHeight/2, 0);

                float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
                var tileToPlace = GetTileFromNoise(noiseValue);

                if (tileToPlace != null)
                {
                    baseTerrain.SetTile(position, tileToPlace);
                }
            }
        }
    }

    private NormalTile GetTileFromNoise(float noiseValue)
    {
        // Debug the noise values and tile selection
        if (Application.isEditor && UnityEngine.Random.value < 0.01f) // Only log 1% of the time to avoid spam
        {
            Debug.Log($"Noise value: {noiseValue:F2}, Water threshold: {waterThreshold:F2}, Stone threshold: {stoneThreshold:F2}");
        }

        if (noiseValue > stoneThreshold && stoneTiles.Length > 0)
        {
            return GetRandomTile(stoneTiles);
        }
        else if (grassTiles.Length > 0)
        {
            return GetRandomTile(grassTiles);
        }

        return null;
    }

    private void GenerateLakes()
    {
        Debug.Log($"Generating {lakeCount} lakes...");

        for (int i = 0; i < lakeCount; i++)
        {
            Vector3Int lakeCenter = startPosition + new Vector3Int(
                Random.Range(-mapWidth/2, mapWidth/2),
                Random.Range(-mapHeight/2, mapHeight/2),
                0
            );

            int lakeSize = Random.Range(minLakeSize, maxLakeSize + 1);
            Debug.Log($"Generating lake {i+1} at {lakeCenter} with size {lakeSize}");
            GenerateLake(lakeCenter, lakeSize);
        }

        Debug.Log("Lake generation complete!");
    }

    private void GenerateLake(Vector3Int center, int size)
    {
        List<Vector3Int> lakePositions = new List<Vector3Int>();

        // Generate circular lake
        for (int x = -size; x <= size; x++)
        {
            for (int y = -size; y <= size; y++)
            {
                Vector3Int position = center + new Vector3Int(x, y, 0);
                float distance = Vector2.Distance(Vector2.zero, new Vector2(x, y));

                if (distance <= size && Random.value > (distance / size) * 0.5f)
                {
                    lakePositions.Add(position);
                }
            }
        }

        Debug.Log($"Generated {lakePositions.Count} water tile positions for lake at {center}");

        // Place water tiles
        WaterTile waterTile = GetRandomWaterTile();
        if (waterTile == null)
        {
            Debug.LogError("No water tile available! Make sure you assign water tiles to the TerrainGenerator.");
            return;
        }

        int placedTiles = 0;
        foreach (var position in lakePositions)
        {
            if (IsInBounds(position))
            {
                waterLayer.SetTile(position, waterTile);
                placedTiles++;
            }
        }

        Debug.Log($"Placed {placedTiles} water tiles in lake at {center}");
    }

    private void SmoothTerrain()
    {
        for (int pass = 0; pass < smoothingPasses; pass++)
        {
            SmoothTerrainPass();
        }
    }

    private void SmoothTerrainPass()
    {
        Dictionary<Vector3Int, TileBase> tilesToUpdate = new Dictionary<Vector3Int, TileBase>();

        for (int x = -mapWidth/2; x < mapWidth/2; x++)
        {
            for (int y = -mapHeight/2; y < mapHeight/2; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase currentTile = baseTerrain.GetTile(position);

                // Count neighboring tile types
                Dictionary<System.Type, int> neighborCounts = CountNeighbors(position);

                // Find most common neighbor type
                System.Type mostCommonType = null;
                int maxCount = 0;

                foreach (var kvp in neighborCounts)
                {
                    if (kvp.Value > maxCount)
                    {
                        maxCount = kvp.Value;
                        mostCommonType = kvp.Key;
                    }
                }

                // Replace tile if it's outnumbered
                if (maxCount >= 4 && mostCommonType != currentTile?.GetType())
                {
                    TileBase newTile = GetTileOfType(mostCommonType);
                    if (newTile != null)
                    {
                        tilesToUpdate[position] = newTile;
                    }
                }
            }
        }

        // Apply updates
        foreach (var kvp in tilesToUpdate)
        {
            baseTerrain.SetTile(kvp.Key, kvp.Value);
        }
    }

    private Dictionary<System.Type, int> CountNeighbors(Vector3Int position)
    {
        Dictionary<System.Type, int> counts = new Dictionary<System.Type, int>();

        Vector3Int[] neighbors = {
            position + Vector3Int.up,
            position + Vector3Int.down,
            position + Vector3Int.left,
            position + Vector3Int.right,
            position + Vector3Int.up + Vector3Int.left,
            position + Vector3Int.up + Vector3Int.right,
            position + Vector3Int.down + Vector3Int.left,
            position + Vector3Int.down + Vector3Int.right
        };

        foreach (var neighborPos in neighbors)
        {
            TileBase neighborTile = baseTerrain.GetTile(neighborPos);
            if (neighborTile != null)
            {
                System.Type tileType = neighborTile.GetType();
                if (counts.ContainsKey(tileType))
                    counts[tileType]++;
                else
                    counts[tileType] = 1;
            }
        }

        return counts;
    }

    private TileBase GetTileOfType(System.Type tileType)
    {
        if (tileType == typeof(NormalTile))
        {
            return GetRandomTile(grassTiles);
        }
        else if (grassTiles.Length > 0 && grassTiles[0].GetType() == tileType)
        {
            return GetRandomTile(grassTiles);
        }
        else if (stoneTiles.Length > 0 && stoneTiles[0].GetType() == tileType)
        {
            return GetRandomTile(stoneTiles);
        }
        else if (sandTiles.Length > 0 && sandTiles[0].GetType() == tileType)
        {
            return GetRandomTile(sandTiles);
        }

        return GetRandomTile(grassTiles); // Fallback
    }

    private NormalTile GetRandomTile(NormalTile[] tiles)
    {
        if (tiles.Length == 0) return null;
        return tiles[Random.Range(0, tiles.Length)];
    }

    private WaterTile GetRandomWaterTile()
    {
        if (waterTiles.Length == 0) return null;
        return waterTiles[Random.Range(0, waterTiles.Length)];
    }

    private bool IsInBounds(Vector3Int position)
    {
        int minX = startPosition.x - mapWidth/2;
        int maxX = startPosition.x + mapWidth/2;
        int minY = startPosition.y - mapHeight/2;
        int maxY = startPosition.y + mapHeight/2;

        return position.x >= minX && position.x < maxX &&
               position.y >= minY && position.y < maxY;
    }

    [ContextMenu("Generate Sample Tiles")]
    public void GenerateSampleTiles()
    {
        // This method helps create sample tile assets
        Debug.Log("To create sample tiles:");
        Debug.Log("1. Right-click in Project → Create → Fishing Game → Isometric Tile");
        Debug.Log("2. Right-click in Project → Create → Fishing Game → Water Tile");
        Debug.Log("3. Assign sprites and configure properties");
        Debug.Log("4. Assign tiles to this TerrainGenerator component");
    }
#endif

    private void OnDrawGizmosSelected()
    {
        // Draw generation bounds
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + new Vector3(startPosition.x, startPosition.y, 0);
        Vector3 size = new Vector3(mapWidth, mapHeight, 0);
        Gizmos.DrawWireCube(center, size);

        // Draw lake positions if generating
        if (generateLakes)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < lakeCount; i++)
            {
                Vector3 lakePos = center + new Vector3(
                    Random.Range(-mapWidth/2, mapWidth/2),
                    Random.Range(-mapHeight/2, mapHeight/2),
                    0
                );
                Gizmos.DrawWireSphere(lakePos, maxLakeSize);
            }
        }
    }
}