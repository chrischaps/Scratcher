# Level Configuration System Guide

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Prerequisites](#prerequisites)
4. [Creating Level Configurations](#creating-level-configurations)
5. [Configuration Properties](#configuration-properties)
6. [Grid Configuration](#grid-configuration)
7. [Using Level Configurations](#using-level-configurations)
8. [Advanced Workflows](#advanced-workflows)
9. [Testing and Debugging](#testing-and-debugging)
10. [Troubleshooting](#troubleshooting)
11. [API Reference](#api-reference)

---

## Overview

The Level Configuration System provides a ScriptableObject-based approach to creating reusable, designer-friendly level presets for Scratcher. Each configuration stores complete level settings including terrain generation parameters, tile assets, grid settings, and environmental conditions.

### Key Features
- **ScriptableObject-Based**: Reusable assets that can be version controlled
- **Complete Level Definitions**: All parameters in one place
- **Designer-Friendly**: No coding required to create level variants
- **Runtime Loadable**: Switch levels programmatically during gameplay
- **Grid System Integration**: Configures isometric/square/hexagonal grids
- **Terrain Generation**: Controls procedural terrain creation
- **Environment Setup**: Default weather, start time, and ambient settings

### Core Components
- **LevelConfiguration**: ScriptableObject containing all level data
- **GridConfiguration**: Nested configuration for Unity Grid component
- **TerrainGenerator**: Consumes level config for terrain creation
- **GridType Enum**: Defines grid layout types

---

## System Architecture

### File Locations

**Core Scripts:**
```
Assets/Scripts/Data/LevelConfiguration.cs
Assets/Scripts/Data/GridType.cs
Assets/Scripts/Terrain/TerrainGenerator.cs
Assets/Scripts/Player/GridBasedPlayerController.cs
```

**Configuration Assets:**
```
Assets/Data/Configurations/LevelConfiguration.asset (example)
Assets/Data/Configurations/ForestLevel.asset
Assets/Data/Configurations/DesertLevel.asset
```

**Tile Assets Referenced:**
```
Assets/World/Tiles/ (grass, stone, sand, water tiles)
```

### Data Structure

```
LevelConfiguration (ScriptableObject)
├─ Level Info
│  ├─ levelName: "Forest Valley"
│  └─ levelDescription: "A peaceful forest area..."
│
├─ Grid Configuration
│  ├─ gridType: Isometric
│  ├─ cellSize: Vector3(1.0, 0.5, 1.0)
│  ├─ cellGap: Vector3(0, 0, 0)
│  └─ cellSwizzle: XYZ
│
├─ Terrain Generation
│  ├─ mapSize: Vector2Int(50, 50)
│  ├─ noiseScale: 0.1
│  ├─ waterThreshold: 0.3
│  ├─ stoneThreshold: 0.7
│  └─ smoothingPasses: 2
│
├─ Lake Generation
│  ├─ generateLakes: true
│  ├─ lakeCount: 3
│  ├─ minLakeSize: 3
│  └─ maxLakeSize: 8
│
├─ Tile Assets
│  ├─ grassTiles: NormalTile[]
│  ├─ stoneTiles: NormalTile[]
│  ├─ sandTiles: NormalTile[]
│  └─ waterTiles: WaterTile[]
│
└─ Environment
   ├─ defaultWeather: Sunny
   └─ startTime: Morning
```

### System Flow

```
Level Load Request
    └─> Assign LevelConfiguration to TerrainGenerator
        └─> TerrainGenerator.GenerateTerrain()
            └─> ApplyLevelConfiguration()
                ├─> Extract terrain parameters
                ├─> Extract tile assets
                ├─> Extract grid settings
                └─> Apply to Grid component
            └─> GenerateBaseTerrain()
            └─> GenerateLakes()
            └─> SmoothTerrain()
            └─> ConfigurePlayerForLevel()
                └─> Set player movement parameters
```

---

## Prerequisites

### 1. Required Systems

Ensure these systems are set up:

**Terrain Generator:**
- Grid GameObject with TerrainGenerator component
- TerrainLayerManager component
- Base and Water tilemaps

**Tile Assets:**
- At least one NormalTile for grass
- At least one WaterTile for lakes
- Optional: stone and sand tiles

### 2. Unity Setup

**Project Requirements:**
- Unity 6000.2.5f1 or later
- 2D Tilemap package installed
- Universal Render Pipeline (URP) configured

### 3. Scene Structure

**Required GameObjects:**
```
Grid
├─ BaseTerrain (Tilemap)
├─ WaterLayer (Tilemap)
├─ DecorationLayer (Tilemap)
└─ CollisionLayer (Tilemap)

Grid (components)
├─ Grid component
├─ TerrainGenerator
└─ TerrainLayerManager
```

---

## Creating Level Configurations

### Step 1: Create Level Configuration Asset

1. **Right-click in Project window:**
   ```
   Assets > Create > Fishing Game > Level Configuration
   ```

2. **Choose save location:**
   ```
   Recommended: Assets/Data/Configurations/
   ```

3. **Name the asset:**
   ```
   Examples:
   - ForestLevel.asset
   - DesertLevel.asset
   - OceanLevel.asset
   - MountainLevel.asset
   ```

### Step 2: Configure Basic Information

Select your new Level Configuration asset:

```
Level Configuration
└─ Level Info
   ├─ Level Name: "Peaceful Forest"
   └─ Level Description: "A serene woodland area with winding streams and abundant wildlife."
```

**Naming conventions:**
- Use descriptive names (Forest Valley, Rocky Peaks, Sandy Beach)
- Keep it concise but clear
- Description can be longer for UI display

### Step 3: Configure Grid Settings

Set up the grid type and dimensions:

```
Grid Configuration
├─ Grid Type: Isometric (dropdown)
├─ Cell Size: (1.0, 0.5, 1.0)
├─ Cell Gap: (0, 0, 0)
└─ Cell Swizzle: XYZ
```

**Grid Type Options:**
- **Isometric**: 2.5D perspective (recommended for Scratcher)
- **Rectangle**: Standard top-down grid
- **Hexagon**: Hexagonal grid layout

**Cell Size:**
- Isometric: typically (1.0, 0.5, 1.0)
- Rectangle: typically (1.0, 1.0, 1.0)
- Hexagon: varies based on tile size

**See [Grid Configuration](#grid-configuration) section for details**

### Step 4: Configure Terrain Generation

Set procedural generation parameters:

```
Terrain Generation
├─ Map Size: (50, 50)
├─ Noise Scale: 0.1
├─ Water Threshold: 0.3
├─ Stone Threshold: 0.7
└─ Smoothing Passes: 2
```

**Quick presets:**

**Small test level:**
```
Map Size: (25, 25)
Noise Scale: 0.15
Stone Threshold: 0.75
Smoothing: 1
```

**Standard level:**
```
Map Size: (50, 50)
Noise Scale: 0.1
Stone Threshold: 0.7
Smoothing: 2
```

**Large open world:**
```
Map Size: (100, 100)
Noise Scale: 0.08
Stone Threshold: 0.65
Smoothing: 3
```

### Step 5: Configure Lake Generation

Set up water feature creation:

```
Lake Generation
├─ Generate Lakes: ✓ true
├─ Lake Count: 3
├─ Min Lake Size: 3
└─ Max Lake Size: 8
```

**Level-specific examples:**

**Forest (multiple ponds):**
```
Generate Lakes: true
Lake Count: 4
Min Size: 2
Max Size: 5
```

**Desert (single oasis):**
```
Generate Lakes: true
Lake Count: 1
Min Size: 4
Max Size: 7
```

**Ocean (no lakes, all water handled separately):**
```
Generate Lakes: false
```

### Step 6: Assign Tile Assets

Drag and drop tile assets into the arrays:

```
Tile Assets
├─ Grass Tiles:
│  ├─ [0] GrassTile_01
│  ├─ [1] GrassTile_02
│  └─ [2] GrassTile_03
│
├─ Stone Tiles:
│  ├─ [0] StoneTile_01
│  └─ [1] StoneTile_02
│
├─ Sand Tiles:
│  └─ [0] SandTile_01
│
└─ Water Tiles:
   └─ [0] PondWater
```

**Tips:**
- More tile variants = more visual variety
- At least 3 grass tiles recommended
- Can leave sand/stone empty if not needed
- Water tiles define fishing zones

### Step 7: Set Environment Defaults

Configure initial environmental conditions:

```
Environment
├─ Default Weather: Sunny (dropdown)
└─ Start Time: Morning (dropdown)
```

**Weather Options:**
- Sunny
- Cloudy
- Rainy
- Foggy

**Start Time Options:**
- Dawn (5-8 AM)
- Morning (8-12 PM)
- Afternoon (12-6 PM)
- Evening (6-10 PM)
- Night (10 PM-5 AM)

---

## Configuration Properties

### Level Info

#### Level Name
```csharp
public string levelName = "New Level";
```

Displayed in UI, menus, and debug logs.

**Examples:**
- "Forest Valley"
- "Desert Oasis"
- "Rocky Mountains"
- "Coastal Beach"

#### Level Description
```csharp
[TextArea(2, 4)]
public string levelDescription;
```

Longer description for level select screens or tooltips.

**Examples:**
```
"A peaceful forest area with winding streams.
Perfect for catching freshwater fish."
```

```
"A harsh desert environment with a single oasis.
Only the hardiest fish survive here."
```

### Terrain Generation Properties

#### Map Size
```csharp
public Vector2Int mapSize = new Vector2Int(50, 50);
```

Dimensions of the generated terrain in tiles.

**Considerations:**
- Larger maps = more generation time
- Affects playable area
- Consider player movement speed
- 50x50 = good default
- 25x25 = small, quick to test
- 100x100 = large, open world

#### Noise Scale
```csharp
public float noiseScale = 0.05f;
```

Controls frequency of terrain features in Perlin noise.

**Effects:**
- **0.01-0.05**: Large, smooth features
- **0.05-0.15**: Medium variation (recommended)
- **0.15-0.30**: Small, detailed features
- **0.30+**: Very noisy, chaotic

**Examples:**
- Rolling hills: 0.08
- Varied terrain: 0.12
- Rocky/jagged: 0.20

#### Water Threshold
```csharp
public float waterThreshold = 0.2f;
```

Currently reserved for future noise-based water generation. Lakes use separate system.

#### Stone Threshold
```csharp
public float stoneThreshold = 0.8f;
```

Perlin noise values above this become stone tiles.

**Landscape types:**
- **0.5-0.6**: Rocky, mountainous (lots of stone)
- **0.7-0.8**: Mixed terrain (default)
- **0.85-0.95**: Grassy with occasional stone
- **0.95+**: Almost no stone

#### Smoothing Passes
```csharp
public int smoothingPasses = 2;
```

Number of smoothing iterations to reduce noise.

**Effects:**
- **0**: Raw noise, very jagged
- **1-2**: Light smoothing (natural)
- **3-4**: Medium smoothing
- **5+**: Heavy smoothing (uniform)

### Lake Generation Properties

#### Generate Lakes
```csharp
public bool generateLakes = true;
```

Enable/disable procedural lake generation.

#### Lake Count
```csharp
public int lakeCount = 2;
```

Number of lakes to generate.

**Guidelines:**
- 1-2: Sparse water
- 3-5: Moderate (default)
- 6+: Water-rich environment

#### Lake Size Range
```csharp
public int minLakeSize = 3;
public int maxLakeSize = 8;
```

Radius of generated lakes in tiles.

**Size reference:**
- 2-3: Small ponds (~15-30 tiles)
- 4-6: Medium lakes (~50-100 tiles)
- 7-10: Large lakes (~150-300 tiles)
- 10+: Very large bodies of water

### Tile Assets

#### Grass/Stone/Sand Tiles
```csharp
public NormalTile[] grassTiles;
public NormalTile[] stoneTiles;
public NormalTile[] sandTiles;
```

Arrays of tile variants for each terrain type.

**Recommendations:**
- **Grass**: 3-5 variants for variety
- **Stone**: 2-3 variants
- **Sand**: 1-2 variants (if used)

#### Water Tiles
```csharp
public WaterTile[] waterTiles;
```

Array of water tile types (Pond, River, Lake, Ocean).

**Typical setup:**
- 1 water tile per level (consistent water type)
- Multiple tiles if level has varied water zones

### Environment Properties

#### Default Weather
```csharp
public WeatherCondition defaultWeather = WeatherCondition.Sunny;
```

Initial weather condition when level loads.

**Options:**
- Sunny: Clear skies, good fishing
- Cloudy: Overcast, neutral
- Rainy: Wet conditions, affects fish
- Foggy: Limited visibility

#### Start Time
```csharp
public TimeOfDay startTime = TimeOfDay.Morning;
```

Time of day when level begins.

**Strategic choices:**
- **Dawn**: Beautiful lighting, morning fish
- **Morning**: Standard, active gameplay
- **Afternoon**: Peak activity time
- **Evening**: Dramatic lighting, evening fish
- **Night**: Challenging, nocturnal fish

---

## Grid Configuration

### GridConfiguration Class

```csharp
[System.Serializable]
public class GridConfiguration
{
    public GridType gridType = GridType.Isometric;
    public Vector3 cellSize = new Vector3(1f, 0.5f, 1f);
    public Vector3 cellGap = Vector3.zero;
    public GridSwizzle cellSwizzle = GridSwizzle.XYZ;
}
```

### Grid Types

#### Isometric Grid
```
Grid Type: Isometric
Cell Size: (1.0, 0.5, 1.0)
Use Case: 2.5D perspective games (Scratcher default)
```

**Visual appearance:**
- Diamond-shaped tiles
- 2:1 aspect ratio (width:height)
- Appears 3D from angled view

**Movement:**
- Diagonal world-space movement
- Isometric player controller required

#### Rectangle Grid
```
Grid Type: Rectangle
Cell Size: (1.0, 1.0, 1.0)
Use Case: Top-down games, retro style
```

**Visual appearance:**
- Square tiles
- Standard top-down view
- Simpler movement

#### Hexagon Grid
```
Grid Type: Hexagon
Cell Size: Based on hexagon dimensions
Use Case: Strategy games, unique movement
```

**Visual appearance:**
- Six-sided tiles
- Natural-looking organic terrain
- Complex pathfinding

### Cell Size

The dimensions of each grid cell:

```csharp
public Vector3 cellSize = new Vector3(1f, 0.5f, 1f);
```

**Isometric typical:**
```
Cell Size: (1.0, 0.5, 1.0)
- X: 1.0 (horizontal width)
- Y: 0.5 (vertical height - half for perspective)
- Z: 1.0 (depth, usually 1.0)
```

**Rectangle typical:**
```
Cell Size: (1.0, 1.0, 1.0)
- Square tiles
```

**Scaling:**
```
Larger tiles:  (2.0, 1.0, 2.0) - double size isometric
Smaller tiles: (0.5, 0.25, 0.5) - half size isometric
```

### Cell Gap

Spacing between cells:

```csharp
public Vector3 cellGap = Vector3.zero;
```

**Typically zero** for seamless tiling.

**Non-zero use cases:**
- Visible grid lines: (0.1, 0.1, 0)
- Separated tiles: (0.2, 0.2, 0)

### Cell Swizzle

Axis remapping for different perspectives:

```csharp
public GridSwizzle cellSwizzle = GridSwizzle.XYZ;
```

**Options:**
- XYZ: Standard (X=right, Y=up, Z=forward)
- XZY: Z and Y swapped
- YXZ: X and Y swapped
- YZX: Complex remapping
- etc.

**Typically use XYZ** unless you need specific 3D integration.

### Applying Grid Configuration

The configuration automatically applies to the Grid component:

```csharp
public void ApplyToGrid(Grid targetGrid)
{
    if (targetGrid == null) return;

    targetGrid.cellLayout = gridConfig.GetCellLayout();
    targetGrid.cellSize = gridConfig.cellSize;
    targetGrid.cellGap = gridConfig.cellGap;
    targetGrid.cellSwizzle = gridConfig.GetCellSwizzle();
}
```

**Called automatically** by TerrainGenerator during generation.

---

## Using Level Configurations

### Method 1: Assign to TerrainGenerator

**In Unity Editor:**

1. **Select Grid GameObject** in Hierarchy

2. **Find TerrainGenerator component**

3. **Assign Level Configuration:**
   ```
   TerrainGenerator
   └─ Level Configuration: [Drag ForestLevel asset here]
   ```

4. **Generate terrain:**
   ```
   Right-click TerrainGenerator > Generate Terrain
   ```

**Result:**
- Terrain generates using config parameters
- Grid configured automatically
- Player controller configured
- Environment settings applied

### Method 2: Runtime Level Loading

Load levels dynamically during gameplay:

```csharp
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private TerrainGenerator terrainGenerator;

    public void LoadLevel(LevelConfiguration config)
    {
        // Assign configuration
        terrainGenerator.SetLevelConfiguration(config);

        // Generate terrain
        terrainGenerator.GenerateTerrain();

        Debug.Log($"Loaded level: {config.levelName}");
    }

    public void LoadLevelByName(string levelName)
    {
        // Load from Resources
        LevelConfiguration config = Resources.Load<LevelConfiguration>($"Levels/{levelName}");

        if (config != null)
        {
            LoadLevel(config);
        }
        else
        {
            Debug.LogError($"Level not found: {levelName}");
        }
    }
}
```

### Method 3: Level Selection Menu

Create a level select screen:

```csharp
public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] private LevelConfiguration[] availableLevels;
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject levelButtonPrefab;

    private void Start()
    {
        CreateLevelButtons();
    }

    private void CreateLevelButtons()
    {
        foreach (LevelConfiguration level in availableLevels)
        {
            GameObject button = Instantiate(levelButtonPrefab, buttonContainer);
            LevelButton levelButton = button.GetComponent<LevelButton>();
            levelButton.Setup(level, this);
        }
    }

    public void SelectLevel(LevelConfiguration level)
    {
        // Load scene with level
        StartCoroutine(LoadLevelScene(level));
    }

    private IEnumerator LoadLevelScene(LevelConfiguration level)
    {
        // Show loading screen
        yield return new WaitForSeconds(0.5f);

        // Load level
        levelLoader.LoadLevel(level);
    }
}

public class LevelButton : MonoBehaviour
{
    [SerializeField] private Text levelNameText;
    [SerializeField] private Text levelDescriptionText;
    [SerializeField] private Button button;

    private LevelConfiguration levelConfig;
    private LevelSelectUI levelSelectUI;

    public void Setup(LevelConfiguration config, LevelSelectUI ui)
    {
        levelConfig = config;
        levelSelectUI = ui;

        levelNameText.text = config.levelName;
        levelDescriptionText.text = config.levelDescription;

        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        levelSelectUI.SelectLevel(levelConfig);
    }
}
```

### Method 4: Progressive Level System

Unlock levels as player progresses:

```csharp
public class ProgressionSystem : MonoBehaviour
{
    [SerializeField] private LevelConfiguration[] levelSequence;
    [SerializeField] private LevelLoader levelLoader;

    private int currentLevelIndex = 0;

    public void CompleteCurrentLevel()
    {
        currentLevelIndex++;

        if (currentLevelIndex < levelSequence.Length)
        {
            NotificationManager.Instance.ShowNotification(
                "Level Complete!",
                $"Unlocked: {levelSequence[currentLevelIndex].levelName}",
                NotificationManager.NotificationType.Success,
                4f
            );
        }
        else
        {
            Debug.Log("All levels completed!");
        }
    }

    public void LoadCurrentLevel()
    {
        if (currentLevelIndex < levelSequence.Length)
        {
            levelLoader.LoadLevel(levelSequence[currentLevelIndex]);
        }
    }

    public void LoadNextLevel()
    {
        currentLevelIndex++;
        LoadCurrentLevel();
    }

    public bool HasNextLevel()
    {
        return currentLevelIndex < levelSequence.Length - 1;
    }
}
```

---

## Advanced Workflows

### Creating Level Variants

**Workflow: Create variations of a base level**

1. **Duplicate existing configuration:**
   ```
   Right-click ForestLevel > Duplicate
   Rename: ForestLevel_Night
   ```

2. **Modify specific properties:**
   ```
   Environment > Start Time: Night
   Terrain > Noise Scale: 0.12 (slightly different terrain)
   ```

3. **Keep core settings same:**
   ```
   Grid config, map size, tile assets stay identical
   ```

**Result:** Multiple level variants with shared assets but different experiences.

### Seasonal Level Sets

**Workflow: Create seasonal variations**

```
SpringForest.asset:
- Grass Tiles: Bright green variants
- Start Time: Morning
- Default Weather: Sunny

SummerForest.asset:
- Grass Tiles: Warm green/yellow variants
- Start Time: Afternoon
- Default Weather: Sunny

AutumnForest.asset:
- Grass Tiles: Orange/red variants
- Start Time: Evening
- Default Weather: Cloudy

WinterForest.asset:
- Grass Tiles: Snowy variants
- Start Time: Morning
- Default Weather: Foggy
```

### Difficulty Progression

**Workflow: Increase challenge through level config**

```
Level 1 - Easy:
- Map Size: 30x30 (smaller)
- Lake Count: 5 (lots of fishing spots)
- Stone Threshold: 0.85 (minimal obstacles)

Level 2 - Medium:
- Map Size: 50x50
- Lake Count: 3
- Stone Threshold: 0.70

Level 3 - Hard:
- Map Size: 70x70
- Lake Count: 2 (scarce fishing)
- Stone Threshold: 0.60 (many obstacles)
```

### Biome System

**Workflow: Create distinct biomes**

**Forest Biome:**
```
Grass Tiles: Forest grass (dark green)
Stone Tiles: Mossy rocks
Water Tiles: Lake water (clear)
Lake Count: 4
Stone Threshold: 0.75
```

**Desert Biome:**
```
Grass Tiles: Sand tiles (tan)
Stone Tiles: Desert rocks (brown)
Water Tiles: Oasis water (blue-green)
Lake Count: 1
Stone Threshold: 0.60 (rocky)
```

**Mountain Biome:**
```
Grass Tiles: Alpine grass (grey-green)
Stone Tiles: Mountain rocks (grey)
Water Tiles: Mountain lake (crystal clear)
Lake Count: 3
Stone Threshold: 0.50 (very rocky)
```

### Random Level Generation

**Workflow: Procedurally select configurations**

```csharp
public class RandomLevelGenerator : MonoBehaviour
{
    [SerializeField] private LevelConfiguration[] levelPool;
    [SerializeField] private LevelLoader levelLoader;

    public void LoadRandomLevel()
    {
        int randomIndex = Random.Range(0, levelPool.Length);
        LevelConfiguration randomLevel = levelPool[randomIndex];

        levelLoader.LoadLevel(randomLevel);

        Debug.Log($"Loaded random level: {randomLevel.levelName}");
    }

    public void LoadRandomLevelWithSeed(int seed)
    {
        Random.InitState(seed);
        LoadRandomLevel();
    }
}
```

### Level Configuration Templates

**Workflow: Base templates for quick creation**

**Create template asset:**
```
_Template_StandardLevel.asset:
- Map Size: 50x50
- Noise Scale: 0.1
- Stone Threshold: 0.7
- Smoothing: 2
- Lake Count: 3
- Lake Sizes: 3-8
- All tile arrays: Empty (to be filled)
```

**Usage:**
1. Duplicate template
2. Rename for specific level
3. Assign tile assets
4. Adjust specific parameters
5. Done!

---

## Testing and Debugging

### Verify Configuration Loading

Test if configuration is applied:

```csharp
// Add to TerrainGenerator.ApplyLevelConfiguration()
Debug.Log($"Applying configuration: {levelConfig.levelName}");
Debug.Log($"Map Size: {levelConfig.mapSize}");
Debug.Log($"Noise Scale: {levelConfig.noiseScale}");
Debug.Log($"Grid Type: {levelConfig.gridConfig.gridType}");
```

### Compare Configurations

Quickly compare multiple configs:

```csharp
[ContextMenu("Print Configuration Summary")]
private void PrintConfigSummary()
{
    Debug.Log($"=== {levelName} ===");
    Debug.Log($"Map: {mapSize.x}x{mapSize.y}");
    Debug.Log($"Lakes: {lakeCount} ({minLakeSize}-{maxLakeSize})");
    Debug.Log($"Grass Tiles: {grassTiles.Length}");
    Debug.Log($"Stone Tiles: {stoneTiles.Length}");
    Debug.Log($"Water Tiles: {waterTiles.Length}");
}
```

### Rapid Testing Workflow

**Quick iteration cycle:**

1. **Create test configuration:**
   ```
   TestLevel.asset with small map (25x25)
   ```

2. **Assign to TerrainGenerator**

3. **Generate terrain:**
   ```
   Context menu > Generate Terrain
   ```

4. **Evaluate results:**
   - Too much stone? Increase stone threshold
   - Not enough variety? Adjust noise scale
   - Lakes too small? Increase size range

5. **Regenerate and repeat**

### Debug Visualization

Add visual debug info:

```csharp
public class LevelConfigDebugger : MonoBehaviour
{
    [SerializeField] private LevelConfiguration config;

    private void OnDrawGizmos()
    {
        if (config == null) return;

        // Draw map bounds
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        Vector3 size = new Vector3(
            config.mapSize.x * config.gridConfig.cellSize.x,
            config.mapSize.y * config.gridConfig.cellSize.y,
            0
        );
        Gizmos.DrawWireCube(center, size);

        // Draw expected lake positions (approximation)
        Gizmos.color = Color.blue;
        for (int i = 0; i < config.lakeCount; i++)
        {
            Vector3 lakePos = center + new Vector3(
                Random.Range(-size.x/2, size.x/2),
                Random.Range(-size.y/2, size.y/2),
                0
            );
            Gizmos.DrawWireSphere(lakePos, config.maxLakeSize);
        }
    }
}
```

---

## Troubleshooting

### Configuration Not Applied

**Symptom:** Terrain generates but doesn't match config settings

**Solutions:**

1. **Verify assignment:**
   ```
   TerrainGenerator > Level Configuration: [Must be assigned]
   ```

2. **Check ApplyLevelConfiguration() is called:**
   ```csharp
   // Should be first line in GenerateTerrain()
   ApplyLevelConfiguration();
   ```

3. **Confirm values copied:**
   ```csharp
   // Add to ApplyLevelConfiguration()
   Debug.Log($"Noise scale set to: {noiseScale}");
   ```

4. **Verify no hardcoded overrides:**
   Check if any script sets values after config applied

### Missing Tile References

**Symptom:** "Tile asset is null" errors

**Solutions:**

1. **Check tile arrays in config:**
   ```
   Tile Assets > Grass Tiles: Size must be > 0
   At least one tile assigned
   ```

2. **Verify tile asset types:**
   ```
   Grass/Stone/Sand: Must be NormalTile type
   Water: Must be WaterTile type
   ```

3. **Confirm tiles exist:**
   ```
   Tile assets haven't been deleted or moved
   ```

4. **Test with minimal setup:**
   ```
   Single grass tile, single water tile
   Generate to verify pipeline works
   ```

### Grid Not Configured

**Symptom:** Grid settings don't match config

**Solutions:**

1. **Ensure ApplyToGrid() called:**
   ```csharp
   // In ApplyLevelConfiguration()
   Grid grid = GetComponentInParent<Grid>();
   if (grid != null)
   {
       levelConfig.ApplyToGrid(grid);
   }
   ```

2. **Check Grid component exists:**
   ```
   Grid GameObject must have Grid component
   ```

3. **Verify GetCellLayout() works:**
   ```csharp
   // In GridConfiguration
   public GridLayout.CellLayout GetCellLayout()
   {
       return gridType switch
       {
           GridType.Isometric => GridLayout.CellLayout.Isometric,
           GridType.Rectangle => GridLayout.CellLayout.Rectangle,
           GridType.Hexagon => GridLayout.CellLayout.Hexagon,
           _ => GridLayout.CellLayout.Rectangle
       };
   }
   ```

### Level Loads Incorrectly at Runtime

**Symptom:** Works in Editor but not in build

**Solutions:**

1. **Use Resources folder:**
   ```
   Move config to: Assets/Resources/Levels/
   Load via: Resources.Load<LevelConfiguration>("Levels/ForestLevel")
   ```

2. **Include in build:**
   ```
   Add to Addressables, or
   Ensure scene references config directly
   ```

3. **Check serialization:**
   ```
   Config should be [Serializable]
   All fields should serialize correctly
   ```

---

## API Reference

### LevelConfiguration Properties

```csharp
[Header("Level Info")]
public string levelName = "New Level";
[TextArea(2, 4)] public string levelDescription;

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
```

### LevelConfiguration Methods

#### ApplyToGrid(Grid targetGrid)
```csharp
public void ApplyToGrid(Grid targetGrid)
```

Applies grid configuration settings to Unity Grid component.

**Parameters:**
- `targetGrid`: The Grid component to configure

**Example:**
```csharp
LevelConfiguration config = Resources.Load<LevelConfiguration>("ForestLevel");
Grid grid = GetComponentInParent<Grid>();
config.ApplyToGrid(grid);
```

### GridConfiguration Class

```csharp
[System.Serializable]
public class GridConfiguration
{
    public GridType gridType = GridType.Isometric;
    public Vector3 cellSize = new Vector3(1f, 0.5f, 1f);
    public Vector3 cellGap = Vector3.zero;
    public GridSwizzle cellSwizzle = GridSwizzle.XYZ;

    public GridLayout.CellLayout GetCellLayout();
    public GridLayout.CellSwizzle GetCellSwizzle();
}
```

#### GetCellLayout()
```csharp
public GridLayout.CellLayout GetCellLayout()
```

Converts GridType enum to Unity's GridLayout.CellLayout.

**Returns:** GridLayout.CellLayout (Isometric, Rectangle, or Hexagon)

#### GetCellSwizzle()
```csharp
public GridLayout.CellSwizzle GetCellSwizzle()
```

Returns Unity's GridLayout.CellSwizzle for axis remapping.

### GridType Enum

```csharp
public enum GridType
{
    Isometric,
    Rectangle,
    Hexagon
}
```

---

## Quick Reference Checklist

### Creating a Configuration
- [ ] Create new LevelConfiguration asset
- [ ] Set level name and description
- [ ] Configure grid type and cell size
- [ ] Set map size and noise parameters
- [ ] Configure lake generation settings
- [ ] Assign grass tile assets (at least 1)
- [ ] Assign water tile assets (at least 1)
- [ ] Set environment defaults
- [ ] Save asset

### Using a Configuration
- [ ] Assign to TerrainGenerator
- [ ] Generate terrain (context menu)
- [ ] Verify terrain appears correctly
- [ ] Check grid settings applied
- [ ] Test player movement
- [ ] Verify lakes generated

### Testing
- [ ] Try generating multiple times
- [ ] Compare with different configurations
- [ ] Test at runtime if using dynamic loading
- [ ] Verify all tile references valid
- [ ] Check Console for errors

---

**Last Updated:** 2025-10-03
**Unity Version:** 6000.2.5f1
**Project:** Scratcher - 2D Isometric Fishing Game
