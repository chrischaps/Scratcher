# Terrain Generation System Guide

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Prerequisites](#prerequisites)
4. [Setting Up Terrain Generation](#setting-up-terrain-generation)
5. [Configuration Options](#configuration-options)
6. [Advanced Features](#advanced-features)
7. [Testing and Debugging](#testing-and-debugging)
8. [Troubleshooting](#troubleshooting)
9. [API Reference](#api-reference)

---

## Overview

The Terrain Generation System in Scratcher provides procedural terrain creation for 2D isometric worlds using Unity's Tilemap system. It generates varied landscapes with grass, stone, water features, and configurable lake systems using Perlin noise algorithms.

### Key Features
- **Procedural Generation**: Noise-based terrain with customizable parameters
- **Multiple Terrain Types**: Grass, stone, sand, and water tiles
- **Automatic Lake Generation**: Creates realistic water bodies with flood-fill algorithms
- **Level Configuration Integration**: ScriptableObject-based preset system
- **Terrain Smoothing**: Multi-pass algorithm for natural-looking terrain transitions
- **Water Zone Auto-Generation**: Automatic fishing zone creation from water tiles

### Core Components
- **TerrainGenerator**: Main generation logic and procedural algorithms
- **TerrainLayerManager**: Layer management, water zone generation, tile info caching
- **LevelConfiguration**: ScriptableObject for storing terrain presets
- **Custom Tiles**: NormalTile and WaterTile with terrain properties

---

## System Architecture

### File Locations

**Core Scripts:**
```
Assets/Scripts/Terrain/TerrainGenerator.cs
Assets/Scripts/Terrain/TerrainLayerManager.cs
Assets/Scripts/Data/LevelConfiguration.cs
Assets/Scripts/Tiles/NormalTile.cs
Assets/Scripts/Tiles/WaterTile.cs
Assets/Scripts/Data/GridType.cs
```

**Configuration Assets:**
```
Assets/Data/Configurations/LevelConfiguration.asset
Assets/World/Tiles/ (tile assets)
```

### Component Relationships

```
LevelConfiguration (ScriptableObject)
    └─> TerrainGenerator
        ├─> Tilemap Layers (Base, Water, Decoration, Collision)
        ├─> Tile Assets (Grass, Stone, Sand, Water)
        └─> TerrainLayerManager
            ├─> Water Zone Generation
            ├─> Tile Info Caching
            └─> Walkability System
```

### Generation Pipeline

```
1. Apply Level Configuration
   └─> Load parameters from ScriptableObject

2. Clear Existing Terrain
   └─> Reset all tilemap layers

3. Generate Base Terrain
   └─> Perlin noise → Tile selection → Placement

4. Generate Lakes (Optional)
   └─> Random positions → Circular generation → Water tile placement

5. Smooth Terrain
   └─> Multi-pass neighbor analysis → Replace outliers

6. Post-Processing
   └─> Generate water zones
   └─> Update collision layer
   └─> Configure player movement
```

---

## Prerequisites

### 1. Unity Packages

Ensure these packages are installed (already included in Scratcher):
- **2D Tilemap Editor** (com.unity.2d.tilemap)
- **Universal Render Pipeline** (URP) 17.2.0+
- **Input System** 1.14.2+

### 2. Scene Setup

Your scene should have a Grid GameObject hierarchy:

```
Grid (Grid component)
├─ BaseTerrain (Tilemap + TilemapRenderer)
├─ WaterLayer (Tilemap + TilemapRenderer)
├─ DecorationLayer (Tilemap + TilemapRenderer)
└─ CollisionLayer (Tilemap + TilemapRenderer)
```

**To create this hierarchy:**
1. Hierarchy → Right-click → 2D Object → Tilemap → Rectangular
2. This creates Grid with one Tilemap child
3. Duplicate the Tilemap 3 times and rename to match above
4. Set sorting orders: Base=0, Water=1, Decoration=2, Collision=3

### 3. Create Tile Assets

Before generating terrain, create tile assets:

**Normal Tiles (Grass, Stone, Sand):**
```
1. Right-click in Project → Create → Fishing Game → Normal Tile
2. Name: GrassTile_01
3. Configure:
   - Tile Sprite: [grass sprite]
   - Terrain Type: Grass
   - Movement Speed Modifier: 1.0
   - Is Walkable: ✓ true
```

**Water Tiles:**
```
1. Right-click in Project → Create → Fishing Game → Water Tile
2. Name: PondWater
3. Configure:
   - Tile Sprite: [water sprite]
   - Water Type: Pond
   - Fishing Success Modifier: 1.0
   - Generate Water Zone: ✓ true
   - Is Walkable: false (auto-set)
```

**Recommended Assets:**
- 3-5 grass tile variants for variety
- 2-3 stone tile variants
- 1-2 sand tile variants
- 1 water tile per water type (Pond, River, Lake, Ocean)

---

## Setting Up Terrain Generation

### Step 1: Add TerrainGenerator Component

1. **Select Grid GameObject** in Hierarchy
2. **Add Component** → TerrainGenerator
3. **Assign Tilemap References:**
   ```
   TerrainGenerator
   ├─ Base Terrain: [BaseTerrain]
   └─ Water Layer: [WaterLayer]
   ```

### Step 2: Add TerrainLayerManager Component

1. **Still on Grid GameObject**
2. **Add Component** → TerrainLayerManager
3. **Assign All Tilemaps:**
   ```
   TerrainLayerManager
   ├─ Base Terrain: [BaseTerrain]
   ├─ Water Layer: [WaterLayer]
   ├─ Decoration Layer: [DecorationLayer]
   ├─ Collision Layer: [CollisionLayer]
   ├─ Auto Generate Water Zones: ✓ true
   └─ Auto Generate Colliders: ✓ true
   ```

### Step 3: Assign Tile Assets to TerrainGenerator

Drag your created tile assets into the TerrainGenerator:

```
TerrainGenerator > Tile Assets
├─ Grass Tiles: [GrassTile_01, GrassTile_02, GrassTile_03]
├─ Stone Tiles: [StoneTile_01, StoneTile_02]
├─ Sand Tiles: [SandTile_01]
└─ Water Tiles: [PondWater]
```

### Step 4: Configure Generation Parameters

Set basic generation parameters:

```
TerrainGenerator > Generation Settings
├─ Map Width: 50
├─ Map Height: 50
└─ Start Position: (0, 0, 0)

TerrainGenerator > Generation Parameters
├─ Noise Scale: 0.1
├─ Water Threshold: 0.3
├─ Stone Threshold: 0.7
└─ Smoothing Passes: 2

TerrainGenerator > Lake Generation
├─ Generate Lakes: ✓ true
├─ Lake Count: 3
├─ Min Lake Size: 3
└─ Max Lake Size: 8
```

### Step 5: Generate Your First Terrain

**In Unity Editor:**
1. Select Grid GameObject
2. Right-click TerrainGenerator component header
3. Click **"Generate Terrain"**
4. Watch Console for progress messages
5. Your terrain appears in Scene view!

**What Happens:**
- Base terrain generates using Perlin noise
- Lakes spawn at random positions
- Terrain smooths over multiple passes
- Water zones automatically generate
- Player controller configured for terrain

---

## Configuration Options

### Generation Settings

#### Map Dimensions
```csharp
[SerializeField] private int mapWidth = 50;
[SerializeField] private int mapHeight = 50;
```

**Guidelines:**
- **Small maps**: 25x25 - Quick generation, good for testing
- **Medium maps**: 50x50 - Standard game levels
- **Large maps**: 100x100 - Open world areas (slower generation)
- **Very large**: 200x200+ - Consider chunked generation

#### Start Position
```csharp
[SerializeField] private Vector3Int startPosition = Vector3Int.zero;
```

The world-space offset for terrain generation. Usually (0,0,0).

### Noise Parameters

#### Noise Scale
```csharp
[SerializeField] private float noiseScale = 0.1f;
```

Controls terrain feature size:
- **0.01 - 0.05**: Very large, smooth terrain features
- **0.1 - 0.2**: Medium terrain variation (recommended)
- **0.3 - 0.5**: Small, detailed features
- **0.5+**: Very noisy, chaotic terrain

#### Water Threshold
```csharp
[SerializeField] private float waterThreshold = 0.3f;
```

**Currently unused** (lakes generate separately), but reserved for future noise-based water generation.

**Planned behavior:**
- 0.0 - 0.2: Lots of water
- 0.3 - 0.4: Moderate water coverage
- 0.5+: Minimal water

#### Stone Threshold
```csharp
[SerializeField] private float stoneThreshold = 0.7f;
```

Perlin values above this become stone tiles:
- **0.5 - 0.6**: Lots of stone (rocky terrain)
- **0.7 - 0.8**: Moderate stone (recommended)
- **0.9+**: Very little stone (mostly grass)

#### Smoothing Passes
```csharp
[SerializeField] private int smoothingPasses = 2;
```

Number of smoothing iterations:
- **0**: Raw noise (very jagged)
- **1-2**: Light smoothing (natural variation)
- **3-5**: Heavy smoothing (uniform regions)
- **6+**: Over-smoothed (too uniform)

### Lake Generation

#### Enable/Disable Lakes
```csharp
[SerializeField] private bool generateLakes = true;
```

Toggle procedural lake generation.

#### Lake Count
```csharp
[SerializeField] private int lakeCount = 3;
```

Number of lakes to generate:
- **1-2**: Sparse water features
- **3-5**: Moderate water coverage (recommended)
- **6-10**: Water-heavy terrain
- **10+**: Risk of overlapping lakes

#### Lake Size Range
```csharp
[SerializeField] private int minLakeSize = 3;
[SerializeField] private int maxLakeSize = 8;
```

**Size in tiles radius:**
- **minLakeSize: 2-3**: Small ponds
- **minLakeSize: 5-7**: Medium lakes
- **minLakeSize: 10+**: Large water bodies

- **maxLakeSize: 5-8**: Reasonable variation
- **maxLakeSize: 10-15**: Large lakes possible
- **maxLakeSize: 20+**: Massive water features

**Lake tile counts:**
- Size 3: ~20-30 tiles
- Size 5: ~50-70 tiles
- Size 8: ~120-150 tiles
- Size 10: ~200-250 tiles

### Layer Configuration

#### Sorting Orders
```csharp
[SerializeField] private int baseTerrainSortOrder = 0;
[SerializeField] private int waterLayerSortOrder = 1;
[SerializeField] private int decorationSortOrder = 2;
[SerializeField] private int collisionSortOrder = 3;
```

Controls rendering order. Higher values render on top.

**Standard setup:**
- Base: 0 (ground tiles)
- Water: 1 (above ground)
- Decoration: 2 (objects/plants)
- Collision: 3 (debug visualization)
- Player: 5 (above all terrain)

---

## Advanced Features

### Level Configuration ScriptableObjects

Create reusable terrain presets for different game levels.

#### Creating a Level Configuration

1. **Create the asset:**
   ```
   Assets > Create > Fishing Game > Level Configuration
   ```

2. **Name it descriptively:**
   ```
   Assets/Data/Configurations/ForestLevel.asset
   ```

3. **Configure properties:**
   ```
   Level Configuration
   ├─ Level Info
   │  ├─ Level Name: "Peaceful Forest"
   │  └─ Description: "A serene woodland with winding streams"
   │
   ├─ Grid Configuration
   │  ├─ Grid Type: Isometric
   │  ├─ Cell Size: (1.0, 0.5, 1.0)
   │  └─ Cell Gap: (0, 0, 0)
   │
   ├─ Terrain Generation
   │  ├─ Map Size: (60, 60)
   │  ├─ Noise Scale: 0.12
   │  ├─ Water Threshold: 0.3
   │  ├─ Stone Threshold: 0.75
   │  └─ Smoothing Passes: 3
   │
   ├─ Lake Generation
   │  ├─ Generate Lakes: true
   │  ├─ Lake Count: 2
   │  ├─ Min Lake Size: 4
   │  └─ Max Lake Size: 9
   │
   ├─ Tile Assets
   │  ├─ Grass Tiles: [Forest grass variants]
   │  ├─ Stone Tiles: [Rocky variants]
   │  └─ Water Tiles: [Lake water]
   │
   └─ Environment
      ├─ Default Weather: Cloudy
      └─ Start Time: Morning
   ```

4. **Assign to TerrainGenerator:**
   ```
   TerrainGenerator > Level Configuration
   └─ [ForestLevel asset]
   ```

5. **Generate:**
   When you call "Generate Terrain", it applies the configuration automatically.

#### Level Configuration Benefits

- **Reusability**: Create once, use in multiple scenes
- **Version Control**: Easy to track changes to level designs
- **Designer-Friendly**: Non-programmers can create variants
- **Runtime Loading**: Can load different configurations programmatically

#### Example Configurations

**Desert Level:**
```
Map Size: 80x80
Noise Scale: 0.08
Stone Threshold: 0.65 (more stone)
Generate Lakes: true
Lake Count: 1 (oasis)
Min/Max Size: 3-5 (small water source)
```

**Mountain Level:**
```
Map Size: 100x100
Noise Scale: 0.15 (rougher terrain)
Stone Threshold: 0.5 (lots of stone)
Generate Lakes: true
Lake Count: 5
Min/Max Size: 2-6 (mountain lakes)
```

**Water World:**
```
Map Size: 70x70
Noise Scale: 0.1
Generate Lakes: true
Lake Count: 8
Min/Max Size: 6-12 (large water bodies)
```

### Manual Terrain Editing

After generation, you can manually edit:

1. **Open Tile Palette:**
   ```
   Window > 2D > Tile Palette
   ```

2. **Select a Tilemap layer** in Hierarchy

3. **Paint/Erase tiles** as needed

4. **Refresh water zones if editing water:**
   ```
   Select Grid > TerrainLayerManager > Right-click > Refresh All
   ```

### Programmatic Generation

Generate terrain at runtime:

```csharp
using UnityEngine;

public class DynamicTerrainLoader : MonoBehaviour
{
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private LevelConfiguration[] levelConfigs;

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelConfigs.Length)
            return;

        // Assign configuration
        terrainGenerator.levelConfig = levelConfigs[levelIndex];

        // Generate terrain
        terrainGenerator.GenerateTerrain();
    }

    public void LoadRandomLevel()
    {
        int randomIndex = Random.Range(0, levelConfigs.Length);
        LoadLevel(randomIndex);
    }
}
```

### Custom Tile Placement

Override specific areas with custom tiles:

```csharp
public void PlaceCustomArea(Vector3Int position, int width, int height, TileBase customTile)
{
    Tilemap baseTerrain = GetComponent<Tilemap>();

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Vector3Int tilePos = position + new Vector3Int(x, y, 0);
            baseTerrain.SetTile(tilePos, customTile);
        }
    }
}
```

### Terrain Smoothing Algorithm

The smoothing system uses neighbor analysis:

```
For each tile position:
  1. Count neighbor types (8 directions)
  2. Find most common type
  3. If 4+ neighbors match and current tile doesn't:
     → Replace with most common type
  4. Repeat for N passes
```

**Effect:**
- Removes single-tile "islands"
- Smooths jagged edges
- Creates natural-looking transitions
- Preserves large features

### Water Zone Auto-Generation

TerrainLayerManager automatically creates fishing zones:

**Process:**
1. Scan WaterLayer tilemap
2. Find all WaterTile instances with `generateWaterZone = true`
3. Group connected tiles using flood-fill (4-directional)
4. For each group:
   - Create GameObject at center
   - Add WaterZone component
   - Add PolygonCollider2D matching water shape
   - Name based on water type and size
5. Configure water type based on tile properties

**Manual Refresh:**
```
Grid > TerrainLayerManager > Right-click > Refresh All
```

---

## Testing and Debugging

### Visual Testing

#### Scene View Gizmos

When Grid is selected, TerrainGenerator draws:
- **Yellow wireframe**: Generation bounds
- **Blue wire spheres**: Lake spawn positions (when generating)

```csharp
private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    Vector3 center = transform.position;
    Vector3 size = new Vector3(mapWidth, mapHeight, 0);
    Gizmos.DrawWireCube(center, size);
}
```

#### Verifying Tile Placement

**Check tile distribution:**
1. Select BaseTerrain tilemap
2. Window → Analysis → Profiler
3. Look at tile counts in Inspector

**Expected distributions (typical):**
- Grass: 60-80% of base terrain
- Stone: 15-25% of base terrain
- Water: 10-20% of total area (if lakes enabled)

### Console Logging

TerrainGenerator logs generation progress:

```
Applied level configuration: Peaceful Forest
Generated 23 water tile positions for lake at (15, -8, 0)
Placed 23 water tiles in lake at (15, -8, 0)
Lake generation complete!
Terrain generation complete!
```

**Enable detailed logging:**
```csharp
// In TerrainGenerator.GetTileFromNoise()
Debug.Log($"Noise: {noiseValue:F2}, Threshold: {stoneThreshold:F2}");
```

### Performance Testing

**Measure generation time:**
```csharp
private void GenerateTerrain()
{
    float startTime = Time.realtimeSinceStartup;

    // ... generation code ...

    float duration = Time.realtimeSinceStartup - startTime;
    Debug.Log($"Terrain generated in {duration:F2} seconds");
}
```

**Typical generation times:**
- 25x25 map: 0.1-0.2s
- 50x50 map: 0.3-0.5s
- 100x100 map: 1-2s
- 200x200 map: 5-10s

### Testing Walkability

Verify TerrainLayerManager walkability system:

```csharp
void TestWalkability()
{
    TerrainLayerManager manager = FindObjectOfType<TerrainLayerManager>();

    Vector3 testPosition = new Vector3(5, 5, 0);
    bool walkable = manager.IsWalkable(testPosition);
    bool water = manager.IsWater(testPosition);

    Debug.Log($"Position {testPosition}: Walkable={walkable}, Water={water}");
}
```

### Context Menu Testing

Quick generation tests via context menu:

```csharp
[ContextMenu("Generate Small Test")]
private void GenerateSmallTest()
{
    int originalWidth = mapWidth;
    int originalHeight = mapHeight;

    mapWidth = 20;
    mapHeight = 20;

    GenerateTerrain();

    mapWidth = originalWidth;
    mapHeight = originalHeight;
}
```

---

## Troubleshooting

### No Terrain Generates

**Symptom:** "Generate Terrain" does nothing

**Solutions:**

1. **Check tilemap assignment:**
   ```
   TerrainGenerator > Base Terrain: [Must be assigned]
   ```

2. **Verify tile assets:**
   ```
   TerrainGenerator > Tile Assets > Grass Tiles: [At least 1 tile]
   ```

3. **Check Console for errors:**
   ```
   "Base terrain tilemap not assigned!"
   "No water tile available!"
   ```

4. **Ensure Editor mode:**
   Generation only works in Editor (check `#if UNITY_EDITOR` guards)

### Terrain Looks Too Uniform

**Symptom:** All grass or all stone, no variation

**Solutions:**

1. **Adjust noise scale:**
   ```
   Try: 0.1 to 0.2 for medium variation
   ```

2. **Check threshold values:**
   ```
   Stone Threshold: Should be 0.6-0.8
   Too low: All stone
   Too high: All grass
   ```

3. **Reduce smoothing:**
   ```
   Smoothing Passes: 1-2 instead of 5+
   ```

4. **Add tile variety:**
   ```
   Assign 3-5 different grass tiles for visual variety
   ```

### No Lakes Appear

**Symptom:** Generate Lakes enabled but no water

**Solutions:**

1. **Verify water layer:**
   ```
   TerrainGenerator > Water Layer: [Must be assigned]
   ```

2. **Check water tile assets:**
   ```
   TerrainGenerator > Water Tiles: [At least 1 tile]
   ```

3. **Increase lake size:**
   ```
   Min Lake Size: 3+
   Max Lake Size: 8+
   ```

4. **Check Console:**
   ```
   Look for: "Generating X lakes..."
   Look for: "Placed X water tiles in lake"
   ```

5. **Verify water tile configuration:**
   ```
   WaterTile asset > Generate Water Zone: true
   ```

### Lakes Outside Map Bounds

**Symptom:** Water tiles in unexpected locations

**Solutions:**

1. **Check map dimensions:**
   ```
   Ensure mapWidth/mapHeight match your intended area
   ```

2. **Verify start position:**
   ```
   Start Position: Usually (0, 0, 0)
   ```

3. **Lake positions are random:**
   This is expected behavior - lakes spawn randomly within bounds

### No Water Zones Generated

**Symptom:** Lakes exist but no WaterZone GameObjects

**Solutions:**

1. **Enable auto-generation:**
   ```
   TerrainLayerManager > Auto Generate Water Zones: ✓ true
   ```

2. **Check WaterTile setting:**
   ```
   WaterTile asset > Generate Water Zone: ✓ true
   ```

3. **Manual refresh:**
   ```
   Grid > TerrainLayerManager > Right-click > Refresh All
   ```

4. **Check Console:**
   ```
   Should see: "Found X water tiles"
   Should see: "Generated X water zones total"
   ```

5. **Verify water tiles are WaterTile type:**
   Regular tiles won't generate zones

### Performance Issues / Slow Generation

**Symptom:** Generation takes very long or freezes

**Solutions:**

1. **Reduce map size:**
   ```
   50x50 instead of 200x200 for testing
   ```

2. **Decrease smoothing passes:**
   ```
   Smoothing Passes: 1-2 instead of 5+
   ```

3. **Limit lake count:**
   ```
   Lake Count: 2-3 instead of 10+
   ```

4. **Simplify tile variety:**
   Too many tile variants can slow selection

5. **Profile the generation:**
   ```
   Window > Analysis > Profiler
   Check which method takes longest
   ```

### Tiles Don't Match Sprites

**Symptom:** Wrong sprites appear or tiles are blank

**Solutions:**

1. **Check tile sprite assignment:**
   ```
   Select tile asset > Tile Sprite: [Must be assigned]
   ```

2. **Verify sprite import settings:**
   ```
   Sprite Mode: Single or Multiple
   Pixels Per Unit: Match your game (usually 16 or 32)
   Texture Type: Sprite (2D and UI)
   ```

3. **Ensure tile assets are correct type:**
   ```
   Must be NormalTile or WaterTile, not base TileBase
   ```

4. **Refresh tilemap:**
   ```csharp
   baseTerrain.RefreshAllTiles();
   ```

### Player Walks on Water

**Symptom:** Player can move over water tiles

**Solutions:**

1. **Check water tile walkability:**
   ```
   WaterTile sets IsWalkable = false automatically
   ```

2. **Verify TerrainLayerManager setup:**
   ```
   Player must have TerrainAwarePlayerController
   Player controller must reference TerrainLayerManager
   ```

3. **Test walkability system:**
   ```csharp
   bool walkable = terrainManager.IsWalkable(playerPosition);
   Debug.Log($"Player position walkable: {walkable}");
   ```

4. **Check player controller:**
   Ensure player uses TerrainAwarePlayerController, not basic IsometricPlayerController

### Level Configuration Not Applied

**Symptom:** Changes to LevelConfiguration don't affect generation

**Solutions:**

1. **Verify assignment:**
   ```
   TerrainGenerator > Level Configuration: [Drag asset here]
   ```

2. **Check ApplyLevelConfiguration() call:**
   Should run before GenerateBaseTerrain()

3. **Re-assign the asset:**
   Sometimes Unity loses reference - reassign it

4. **Ensure you're editing the correct asset:**
   Check file path matches expected location

---

## API Reference

### TerrainGenerator Methods

#### GenerateTerrain()
```csharp
[ContextMenu("Generate Terrain")]
public void GenerateTerrain()
```

Main terrain generation method. Executes full pipeline:
- Apply level configuration
- Clear existing terrain
- Generate base terrain with noise
- Generate lakes (if enabled)
- Smooth terrain
- Refresh terrain manager
- Configure player

**Usage:**
```csharp
TerrainGenerator generator = GetComponent<TerrainGenerator>();
generator.GenerateTerrain();
```

#### ClearTerrain()
```csharp
[ContextMenu("Clear Terrain")]
public void ClearTerrain()
```

Removes all tiles from base terrain and water layer.

**Usage:**
```csharp
generator.ClearTerrain();
```

### TerrainLayerManager Methods

#### GetTileInfo(Vector3Int position)
```csharp
public TerrainTileInfo GetTileInfo(Vector3Int position)
```

Returns comprehensive tile information at grid position.

**Returns:** TerrainTileInfo containing:
- `isWalkable`: Can player walk here
- `isWater`: Is this water
- `terrainType`: TerrainType enum value
- `waterType`: WaterType enum value
- `speedModifier`: Movement speed multiplier
- `fishingModifier`: Fishing success multiplier

**Usage:**
```csharp
TerrainLayerManager manager = FindObjectOfType<TerrainLayerManager>();
Vector3Int gridPos = new Vector3Int(10, 5, 0);
TerrainTileInfo info = manager.GetTileInfo(gridPos);

Debug.Log($"Walkable: {info.isWalkable}, Speed: {info.speedModifier}");
```

#### IsWalkable(Vector3 worldPosition)
```csharp
public bool IsWalkable(Vector3 worldPosition)
```

Checks if world position is walkable. Converts to grid coordinates internally.

**Parameters:**
- `worldPosition`: World-space position to check

**Returns:** `true` if position is walkable, `false` if blocked or water

**Usage:**
```csharp
Vector3 playerPos = player.transform.position;
if (manager.IsWalkable(playerPos))
{
    // Safe to move
}
```

#### IsWater(Vector3 worldPosition)
```csharp
public bool IsWater(Vector3 worldPosition)
```

Checks if world position contains water.

**Usage:**
```csharp
if (manager.IsWater(bobberPosition))
{
    // Can fish here
}
```

#### GetWaterType(Vector3 worldPosition)
```csharp
public WaterType GetWaterType(Vector3 worldPosition)
```

Returns water type at position (Pond, River, Lake, Ocean).

**Usage:**
```csharp
WaterType type = manager.GetWaterType(playerPosition);
Debug.Log($"Fishing in: {type}");
```

#### GetMovementSpeedModifier(Vector3 worldPosition)
```csharp
public float GetMovementSpeedModifier(Vector3 worldPosition)
```

Returns movement speed multiplier for terrain at position.

**Returns:**
- `1.0`: Normal speed (grass)
- `0.7-0.9`: Slower (sand, rough terrain)
- `1.2-1.5`: Faster (roads, smooth surfaces)
- `0.0-0.5`: Very slow (unwalkable)

**Usage:**
```csharp
float speedMod = manager.GetMovementSpeedModifier(player.position);
float actualSpeed = baseSpeed * speedMod;
```

#### RefreshTileCache()
```csharp
public void RefreshTileCache()
```

Clears internal tile info cache. Call after manually editing tiles.

#### RefreshWaterZones()
```csharp
public void RefreshWaterZones()
```

Regenerates all water zones from current water tiles.

#### RefreshAll()
```csharp
[ContextMenu("Refresh All")]
public void RefreshAll()
```

Refreshes everything: cache, water zones, collision layer.

**Usage:**
```
In Editor: Right-click component > Refresh All
```

### LevelConfiguration Methods

#### ApplyToGrid(Grid targetGrid)
```csharp
public void ApplyToGrid(Grid targetGrid)
```

Applies grid configuration settings to a Grid component.

**Parameters:**
- `targetGrid`: Unity Grid component to configure

**Usage:**
```csharp
LevelConfiguration config = Resources.Load<LevelConfiguration>("ForestLevel");
Grid grid = GetComponentInParent<Grid>();
config.ApplyToGrid(grid);
```

### Custom Tile Methods

#### NormalTile
```csharp
public bool IsWalkable()
public TerrainType GetTerrainType()
public float GetMovementSpeedModifier()
```

#### WaterTile
```csharp
public WaterType GetWaterType()
public float GetFishingSuccessModifier()
public bool ShouldGenerateWaterZone()
```

---

## Example Workflows

### Workflow 1: Creating a New Level

**Goal:** Create a forest level with streams

```
1. Create Level Configuration:
   Assets > Create > Fishing Game > Level Configuration
   Name: ForestLevel

2. Configure settings:
   Map Size: 60x60
   Noise Scale: 0.12
   Stone Threshold: 0.75
   Generate Lakes: true
   Lake Count: 2
   Lake Sizes: 4-6 (streams)

3. Assign tiles:
   Grass: Forest grass variants
   Stone: Rock variants
   Water: Stream water tile

4. Assign to TerrainGenerator:
   Generator > Level Configuration: [ForestLevel]

5. Generate:
   Grid > TerrainGenerator > Generate Terrain

6. Manual refinement:
   Use Tile Palette to add details
   Refresh water zones if needed
```

### Workflow 2: Testing Different Noise Scales

**Goal:** Find the perfect terrain variation

```
1. Create test configurations:
   - NoiseTest_Small: scale 0.05
   - NoiseTest_Medium: scale 0.15
   - NoiseTest_Large: scale 0.3

2. For each configuration:
   - Assign to TerrainGenerator
   - Generate Terrain
   - Take screenshot
   - Note visual impression

3. Select winner and refine:
   - Adjust stone threshold
   - Tweak smoothing passes
   - Generate final version
```

### Workflow 3: Runtime Level Loading

**Goal:** Load different levels during gameplay

```csharp
public class LevelLoader : MonoBehaviour
{
    [SerializeField] private TerrainGenerator generator;
    [SerializeField] private LevelConfiguration[] levels;
    private int currentLevel = 0;

    public void LoadNextLevel()
    {
        currentLevel = (currentLevel + 1) % levels.Length;
        generator.levelConfig = levels[currentLevel];
        generator.GenerateTerrain();

        Debug.Log($"Loaded level: {levels[currentLevel].levelName}");
    }

    public void LoadLevel(string levelName)
    {
        LevelConfiguration config = System.Array.Find(
            levels,
            l => l.levelName == levelName
        );

        if (config != null)
        {
            generator.levelConfig = config;
            generator.GenerateTerrain();
        }
    }
}
```

---

## Performance Considerations

### Optimization Tips

1. **Generate at appropriate times:**
   - During loading screens
   - Before scene transition
   - Not during active gameplay

2. **Cache tile info:**
   TerrainLayerManager automatically caches - don't query every frame

3. **Use appropriate map sizes:**
   - 50x50 for contained levels
   - 100x100 for open areas
   - 200x200+ requires optimization

4. **Limit smoothing passes:**
   Each pass iterates entire map - 1-3 passes usually sufficient

5. **Reuse level configurations:**
   Don't generate unique terrain every time

### Memory Usage

**Approximate memory per tile:**
- TileBase reference: 8 bytes
- Cached TileInfo: 32 bytes

**For 100x100 map:**
- Tilemap data: ~80 KB
- Tile info cache: ~320 KB
- Total: <500 KB (negligible)

### Async Generation (Future)

For very large maps, consider async generation:

```csharp
public async Task GenerateTerrainAsync()
{
    await Task.Run(() => {
        // Heavy generation logic
    });

    // UI update on main thread
}
```

---

## Quick Reference Checklist

### Initial Setup
- [ ] Create Grid with 4 tilemap layers
- [ ] Set tilemap sorting orders (0,1,2,3)
- [ ] Create tile assets (grass, stone, water)
- [ ] Add TerrainGenerator to Grid
- [ ] Add TerrainLayerManager to Grid
- [ ] Assign all tilemap references
- [ ] Assign tile assets

### Generation
- [ ] Configure map size and parameters
- [ ] Set noise scale and thresholds
- [ ] Configure lake generation settings
- [ ] (Optional) Create and assign LevelConfiguration
- [ ] Run "Generate Terrain"
- [ ] Verify water zones generated
- [ ] Test player walkability

### Troubleshooting
- [ ] Check Console for errors
- [ ] Verify all references assigned
- [ ] Ensure tile sprites configured
- [ ] Test with smaller map first
- [ ] Refresh water zones if needed

---

**Last Updated:** 2025-10-03
**Unity Version:** 6000.2.5f1
**Project:** Scratcher - 2D Isometric Fishing Game
