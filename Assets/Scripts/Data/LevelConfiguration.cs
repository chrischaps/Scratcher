using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfiguration", menuName = "Fishing Game/Level Configuration")]
public class LevelConfiguration : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "New Level";
    [TextArea(2, 4)]
    public string levelDescription;

    [Header("Grid Configuration")]
    public GridConfiguration gridConfig = new GridConfiguration();

    [Header("Terrain Generation")]
    public Vector2Int mapSize = new Vector2Int(50, 50);
    public float noiseScale = 0.05f;
    public float waterThreshold = 0.2f;
    public float stoneThreshold = 0.8f;
    public int smoothingPasses = 2;

    [Header("Lake Generation")]
    public bool generateLakes = true;
    public int lakeCount = 2;
    public int minLakeSize = 3;
    public int maxLakeSize = 8;

    [Header("Tile Assets")]
    public NormalTile[] grassTiles;
    public NormalTile[] stoneTiles;
    public NormalTile[] sandTiles;
    public WaterTile[] waterTiles;

    [Header("Environment")]
    public WeatherCondition defaultWeather = WeatherCondition.Sunny;
    public TimeOfDay startTime = TimeOfDay.Morning;

    public void ApplyToGrid(Grid targetGrid)
    {
        if (targetGrid == null) return;

        targetGrid.cellLayout = gridConfig.GetCellLayout();
        targetGrid.cellSize = gridConfig.cellSize;
        targetGrid.cellGap = gridConfig.cellGap;
        targetGrid.cellSwizzle = gridConfig.GetCellSwizzle();
    }
}