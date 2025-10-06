using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TerrainLayerManager : MonoBehaviour
{
    [Header("Tilemap Layers")]
    [SerializeField] private Tilemap baseTerrain;
    [SerializeField] private Tilemap waterLayer;
    [SerializeField] private Tilemap decorationLayer;
    [SerializeField] private Tilemap collisionLayer;

    [Header("Layer Settings")]
    [SerializeField] private int baseTerrainSortOrder = 0;
    [SerializeField] private int waterLayerSortOrder = 1;
    [SerializeField] private int decorationSortOrder = 2;
    [SerializeField] private int collisionSortOrder = 3;

    [Header("Auto-Generation")]
    [SerializeField] private bool autoGenerateWaterZones = true;
    [SerializeField] private bool autoGenerateColliders = true;

    private Dictionary<Vector3Int, TerrainTileInfo> tileInfoCache = new Dictionary<Vector3Int, TerrainTileInfo>();
    private GameObject waterZonesParent;

    private void Start()
    {
        SetupLayers();

        // Delay water zone generation to ensure tilemaps are fully loaded
        if (autoGenerateWaterZones)
        {
            Invoke(nameof(GenerateWaterZones), 0.1f);
        }

        if (autoGenerateColliders)
        {
            Invoke(nameof(UpdateCollisionLayer), 0.2f);
        }
    }

    private void SetupLayers()
    {
        // Set up sorting orders for proper layering
        if (baseTerrain != null)
            baseTerrain.GetComponent<TilemapRenderer>().sortingOrder = baseTerrainSortOrder;

        if (waterLayer != null)
            waterLayer.GetComponent<TilemapRenderer>().sortingOrder = waterLayerSortOrder;

        if (decorationLayer != null)
            decorationLayer.GetComponent<TilemapRenderer>().sortingOrder = decorationSortOrder;

        if (collisionLayer != null)
            collisionLayer.GetComponent<TilemapRenderer>().sortingOrder = collisionSortOrder;

        Debug.Log("Terrain layers setup complete!");
    }

    public TerrainTileInfo GetTileInfo(Vector3Int position)
    {
        if (tileInfoCache.ContainsKey(position))
            return tileInfoCache[position];

        TerrainTileInfo info = new TerrainTileInfo();

        // Check base terrain
        if (baseTerrain != null)
        {
            var baseTile = baseTerrain.GetTile(position) as NormalTile;
            if (baseTile != null)
            {
                info.isWalkable = baseTile.IsWalkable();
                info.terrainType = baseTile.GetTerrainType();
                info.speedModifier = baseTile.GetMovementSpeedModifier();
            }
        }

        // Check water layer
        if (waterLayer != null)
        {
            var waterTile = waterLayer.GetTile(position) as WaterTile;
            if (waterTile != null)
            {
                info.isWater = true;
                info.waterType = waterTile.GetWaterType();
                info.fishingModifier = waterTile.GetFishingSuccessModifier();
                info.isWalkable = false; // Water overrides walkability
            }
        }

        tileInfoCache[position] = info;
        return info;
    }

    public bool IsWalkable(Vector3Int position)
    {
        return GetTileInfo(position).isWalkable;
    }

    public bool IsWalkable(Vector3 worldPosition)
    {
        Vector3Int cellPosition = baseTerrain.WorldToCell(worldPosition);
        return IsWalkable(cellPosition);
    }

    public bool IsWater(Vector3Int position)
    {
        return GetTileInfo(position).isWater;
    }

    public bool IsWater(Vector3 worldPosition)
    {
        Vector3Int cellPosition = waterLayer != null ? waterLayer.WorldToCell(worldPosition) : baseTerrain.WorldToCell(worldPosition);
        return IsWater(cellPosition);
    }

    public WaterType GetWaterType(Vector3 worldPosition)
    {
        Vector3Int cellPosition = waterLayer != null ? waterLayer.WorldToCell(worldPosition) : baseTerrain.WorldToCell(worldPosition);
        return GetTileInfo(cellPosition).waterType;
    }

    public float GetMovementSpeedModifier(Vector3 worldPosition)
    {
        Vector3Int cellPosition = baseTerrain.WorldToCell(worldPosition);
        return GetTileInfo(cellPosition).speedModifier;
    }

    private void GenerateWaterZones()
    {
        if (waterLayer == null)
        {
            Debug.LogWarning("Water layer not assigned, skipping water zone generation");
            return;
        }

        // Clear existing water zones first
        ClearWaterZones();

        // Create parent GameObject for organization
        waterZonesParent = new GameObject("WaterZones");
        waterZonesParent.transform.SetParent(transform);

        BoundsInt bounds = waterLayer.cellBounds;
        List<Vector3Int> waterTiles = new List<Vector3Int>();

        Debug.Log($"Searching for water tiles in bounds: {bounds}");

        // Find all water tiles
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tile = waterLayer.GetTile(position);

                if (tile is WaterTile waterTile && waterTile.ShouldGenerateWaterZone())
                {
                    waterTiles.Add(position);
                }
            }
        }

        Debug.Log($"Found {waterTiles.Count} water tiles");

        if (waterTiles.Count == 0)
        {
            Debug.Log("No water tiles found for zone generation");
            return;
        }

        // Group connected water tiles into zones
        var waterZones = GroupWaterTiles(waterTiles);

        Debug.Log($"Grouped into {waterZones.Count} separate water zones");

        // Create WaterZone components for each group (temporarily allow all sizes)
        int createdZones = 0;
        foreach (var zone in waterZones)
        {
            Debug.Log($"Creating water zone with {zone.Count} tiles");
            CreateWaterZone(zone);
            createdZones++;
        }

        Debug.Log($"Generated {createdZones} water zones total");
    }

    private List<List<Vector3Int>> GroupWaterTiles(List<Vector3Int> waterTiles)
    {
        List<List<Vector3Int>> groups = new List<List<Vector3Int>>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        foreach (var tile in waterTiles)
        {
            if (!visited.Contains(tile))
            {
                List<Vector3Int> group = new List<Vector3Int>();
                FloodFillWaterGroup(tile, waterTiles, visited, group);
                if (group.Count > 0)
                    groups.Add(group);
            }
        }

        return groups;
    }

    private void FloodFillWaterGroup(Vector3Int start, List<Vector3Int> allWaterTiles, HashSet<Vector3Int> visited, List<Vector3Int> currentGroup)
    {
        if (visited.Contains(start) || !allWaterTiles.Contains(start))
            return;

        visited.Add(start);
        currentGroup.Add(start);

        // Check adjacent tiles (4-directional for isometric)
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        foreach (var direction in directions)
        {
            FloodFillWaterGroup(start + direction, allWaterTiles, visited, currentGroup);
        }
    }

    private void CreateWaterZone(List<Vector3Int> waterTiles)
    {
        if (waterTiles.Count == 0) return;

        // Calculate center position
        Vector3 centerWorld = Vector3.zero;
        foreach (var tile in waterTiles)
        {
            centerWorld += waterLayer.CellToWorld(tile);
        }
        centerWorld /= waterTiles.Count;

        // Create water zone GameObject
        GameObject waterZoneObj = new GameObject($"WaterZone_{waterTiles.Count}tiles");
        waterZoneObj.transform.position = centerWorld;

        // Parent it to the WaterZones container
        if (waterZonesParent != null)
        {
            waterZoneObj.transform.SetParent(waterZonesParent.transform);
        }

        // Add WaterZone component
        WaterZone waterZone = waterZoneObj.AddComponent<WaterZone>();

        // Set up collider to match the water area
        PolygonCollider2D collider = waterZoneObj.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;

        // Create collider points from water tiles (relative to centerWorld)
        List<Vector2> points = CreateColliderFromTiles(waterTiles, centerWorld);
        collider.points = points.ToArray();

        // Configure water zone based on first water tile properties
        if (waterLayer.GetTile(waterTiles[0]) is WaterTile waterTile)
        {
            // Set water zone properties (you'll need to add these methods to WaterZone)
            waterZoneObj.name = $"WaterZone_{waterTile.GetWaterType()}";
        }
    }

    private List<Vector2> CreateColliderFromTiles(List<Vector3Int> tiles, Vector3 centerWorld)
    {
        // Simple implementation - create a bounding box
        // For more complex shapes, you'd implement proper polygon generation

        Vector3Int min = tiles[0];
        Vector3Int max = tiles[0];

        foreach (var tile in tiles)
        {
            if (tile.x < min.x) min.x = tile.x;
            if (tile.y < min.y) min.y = tile.y;
            if (tile.x > max.x) max.x = tile.x;
            if (tile.y > max.y) max.y = tile.y;
        }

        // Convert to world positions and create rectangle
        Vector3 worldMin = waterLayer.CellToWorld(min);
        Vector3 worldMax = waterLayer.CellToWorld(max + Vector3Int.one);

        // Make points relative to centerWorld (local coordinates)
        return new List<Vector2>
        {
            new Vector2(worldMin.x - centerWorld.x, worldMin.y - centerWorld.y),
            new Vector2(worldMax.x - centerWorld.x, worldMin.y - centerWorld.y),
            new Vector2(worldMax.x - centerWorld.x, worldMax.y - centerWorld.y),
            new Vector2(worldMin.x - centerWorld.x, worldMax.y - centerWorld.y)
        };
    }

    private void UpdateCollisionLayer()
    {
        if (collisionLayer == null || baseTerrain == null) return;

        BoundsInt bounds = baseTerrain.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                if (!IsWalkable(position))
                {
                    // Place collision tile
                    // You can create a simple collision tile asset for this
                    // collisionLayer.SetTile(position, collisionTileAsset);
                }
            }
        }
    }

    public void RefreshTileCache()
    {
        tileInfoCache.Clear();
    }

    public void ClearWaterZones()
    {
        // Destroy the parent and all children
        if (waterZonesParent != null)
        {
            if (Application.isPlaying)
                Destroy(waterZonesParent);
            else
                DestroyImmediate(waterZonesParent);
            waterZonesParent = null;
        }

        // Fallback: Find and destroy any orphaned water zones
        WaterZone[] existingZones = FindObjectsOfType<WaterZone>();
        foreach (var zone in existingZones)
        {
            if (Application.isPlaying)
                Destroy(zone.gameObject);
            else
                DestroyImmediate(zone.gameObject);
        }
    }

    public void RefreshWaterZones()
    {
        ClearWaterZones();
        GenerateWaterZones();
    }

#if UNITY_EDITOR
    [ContextMenu("Refresh All")]
    public void RefreshAll()
    {
        RefreshTileCache();
        RefreshWaterZones();
        UpdateCollisionLayer();
    }
#endif
}

[System.Serializable]
public class TerrainTileInfo
{
    public bool isWalkable = true;
    public bool isWater = false;
    public TerrainType terrainType = TerrainType.Grass;
    public WaterType waterType = WaterType.Pond;
    public float speedModifier = 1f;
    public float fishingModifier = 1f;
}