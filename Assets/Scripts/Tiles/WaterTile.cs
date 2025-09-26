using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "WaterTile", menuName = "Fishing Game/Water Tile")]
public class WaterTile : NormalTile
{
    [Header("Water Properties")]
    public WaterType waterType = WaterType.Pond;
    public float fishingSuccessModifier = 1f;
    public bool generateWaterZone = true;

    [Header("Animation")]
    public Sprite[] animationSprites;
    public float animationSpeed = 1f;
    public bool randomStartFrame = true;

    private void Awake()
    {
        // Ensure water tiles are marked correctly
        isWater = true;
        isWalkable = false;
        terrainType = TerrainType.Water;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        // Get base tile data
        base.GetTileData(position, tilemap, ref tileData);

        // Handle animation
        if (animationSprites != null && animationSprites.Length > 0)
        {
            int frameIndex = GetAnimationFrame(position);
            tileData.sprite = animationSprites[frameIndex];
        }
    }

    private int GetAnimationFrame(Vector3Int position)
    {
        if (animationSprites == null || animationSprites.Length == 0)
            return 0;

        float time = Time.time * animationSpeed;

        if (randomStartFrame)
        {
            // Use position to offset animation timing
            float offset = (position.x + position.y) * 0.1f;
            time += offset;
        }

        return Mathf.FloorToInt(time) % animationSprites.Length;
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);

        // Refresh animation
        if (animationSprites != null && animationSprites.Length > 1)
        {
            tilemap.RefreshTile(position);
        }
    }

    public WaterType GetWaterType() => waterType;
    public float GetFishingSuccessModifier() => fishingSuccessModifier;
    public bool ShouldGenerateWaterZone() => generateWaterZone;
}