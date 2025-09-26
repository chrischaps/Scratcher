using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Tile", menuName = "Fishing Game/Normal Tile")]
public class NormalTile : TileBase
{
    [Header("Tile Properties")]
    public Sprite tileSprite;
    public Color tileColor = Color.white;
    public Matrix4x4 tileTransform = Matrix4x4.identity;

    [Header("Gameplay Properties")]
    public bool isWalkable = true;
    public bool isWater = false;
    public TerrainType terrainType = TerrainType.Grass;
    public float movementSpeedModifier = 1f;

    [Header("Visual Effects")]
    public GameObject tilePrefab; // For 3D decorations
    public bool useRandomRotation = false;
    public bool useRandomFlip = false;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = tileSprite;
        tileData.color = tileColor;
        tileData.transform = GetTileTransform(position);
        tileData.flags = TileFlags.LockTransform;
        tileData.colliderType = isWalkable ? Tile.ColliderType.None : Tile.ColliderType.Sprite;
    }

    private Matrix4x4 GetTileTransform(Vector3Int position)
    {
        Matrix4x4 transform = tileTransform;

        if (useRandomRotation)
        {
            // Use position as seed for consistent randomization
            Random.State oldState = Random.state;
            Random.InitState(position.x + position.y * 1000 + position.z * 1000000);

            float rotation = Random.Range(0, 4) * 90f;
            transform *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, rotation));

            Random.state = oldState;
        }

        if (useRandomFlip)
        {
            Random.State oldState = Random.state;
            Random.InitState(position.x + position.y * 1000 + position.z * 1000000 + 12345);

            bool flipX = Random.value > 0.5f;
            bool flipY = Random.value > 0.5f;

            Vector3 scale = new Vector3(flipX ? -1 : 1, flipY ? -1 : 1, 1);
            transform *= Matrix4x4.Scale(scale);

            Random.state = oldState;
        }

        return transform;
    }

    public bool IsWalkable() => isWalkable;
    public bool IsWater() => isWater;
    public TerrainType GetTerrainType() => terrainType;
    public float GetMovementSpeedModifier() => movementSpeedModifier;
}

public enum TerrainType
{
    Grass,
    Stone,
    Sand,
    Water,
    Dirt,
    Wood
}