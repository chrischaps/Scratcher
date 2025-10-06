# Fishing Zones Configuration Guide

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Prerequisites](#prerequisites)
4. [Method 1: Automatic Water Zone Generation (Tilemap-Based)](#method-1-automatic-water-zone-generation-tilemap-based)
5. [Method 2: Manual Water Zone Creation](#method-2-manual-water-zone-creation)
6. [Configuring Fish Populations](#configuring-fish-populations)
7. [Testing Fishing Zones](#testing-fishing-zones)
8. [Advanced Configuration](#advanced-configuration)
9. [Troubleshooting](#troubleshooting)

---

## Overview

The fishing system in Scratcher consists of three primary components working together:

- **WaterZone**: Defines fishable areas and manages fish populations
- **FishingController**: Handles player fishing interactions and mechanics
- **TerrainLayerManager**: Automatically generates water zones from tilemap data

The system supports two approaches:
1. **Automatic Generation**: Water zones are created from tilemap water tiles
2. **Manual Creation**: Water zones are placed as GameObjects with custom boundaries

---

## System Architecture

### Core Scripts

**File Locations:**
- `Assets/Scripts/Fishing/FishingController.cs` - Player fishing mechanics
- `Assets/Scripts/Fishing/WaterZone.cs` - Water zone management and fish spawning
- `Assets/Scripts/Terrain/TerrainLayerManager.cs` - Automatic zone generation
- `Assets/Scripts/Data/FishData.cs` - Fish data structures and enums
- `Assets/Scripts/Data/FishDatabase.cs` - Central fish database (ScriptableObject)
- `Assets/Scripts/Tiles/WaterTile.cs` - Water tile with fishing properties

### Data Flow

```
Player Enters Water Zone
    └─> FishingController detects water proximity (3f range)
        └─> Player casts line (Attack input)
            └─> WaterZone.TryGetFish() called
                └─> Filters fish by time, weather, water type
                    └─> Weighted random selection based on rarity
                        └─> Success check based on difficulty
                            └─> Returns FishData or null
```

---

## Prerequisites

Before creating fishing zones, ensure you have:

### 1. Fish Database Setup

Create a FishDatabase ScriptableObject:

**Steps:**
1. In Unity: `Assets > Create > Fishing Game > Fish Database`
2. Name it `FishDatabase` and save to `Assets/Data/ScriptableObjects/`
3. Right-click the asset and select `Create Sample Fish` (creates 6 example fish)
4. Alternatively, manually add fish data (see [Configuring Fish Populations](#configuring-fish-populations))

**Reference Location:** `Assets/Data/ScriptableObjects/FishDatabase.asset`

### 2. Game Manager Configuration

Ensure GameManager has references set:

**File:** `Assets/Scripts/Data/GameManager.cs`

The GameManager should load FishDatabase from Resources:
```csharp
// Place FishDatabase.asset in Assets/Resources/ folder
// GameManager will auto-load it in InitializeGame()
```

### 3. Input System Setup

Verify input actions are configured in:
`Assets/Settings/Input/InputSystem_Actions.inputactions`

**Required Actions:**
- **Attack** (Mouse Left Click / Enter) - Cast fishing line
- **Interact** (E key / Gamepad Y) - Cancel fishing

### 4. Player Setup

Your player GameObject needs:
- `FishingController` component
- `GridBasedPlayerController` or compatible movement controller
- Proper layer setup for water detection

---

## Method 1: Automatic Water Zone Generation (Tilemap-Based)

This method automatically creates WaterZone GameObjects from your tilemap water tiles.

### Step 1: Create Water Tiles

1. **Create a WaterTile asset:**
   ```
   Assets > Create > Fishing Game > Water Tile
   ```

2. **Configure the WaterTile:**
   - **File Location:** `Assets/World/Tiles/PondWater.asset` (example)
   - **Tile Sprite:** Assign your water sprite
   - **Water Type:** Select from Pond, River, Lake, or Ocean
   - **Fishing Success Modifier:** 1.0 = normal, higher = better catch rates
   - **Generate Water Zone:** ✓ Checked (enables auto-generation)

   **Example Configuration:**
   ```
   WaterTile: "PondWater"
   ├─ Tile Sprite: water_tile_sprite
   ├─ Water Type: Pond
   ├─ Fishing Success Modifier: 1.0
   ├─ Generate Water Zone: true
   └─ Is Walkable: false (automatically set)
   ```

3. **Create variants for different water types:**
   - `PondWater.asset` (WaterType: Pond)
   - `RiverWater.asset` (WaterType: River)
   - `LakeWater.asset` (WaterType: Lake)
   - `OceanWater.asset` (WaterType: Ocean)

### Step 2: Set Up Tilemap Layers

1. **Create a Grid GameObject** in your scene:
   ```
   Hierarchy > Right-click > 2D Object > Tilemap > Rectangular
   ```

2. **Create required tilemap layers** (as children of Grid):
   - **BaseTerrain** - Ground tiles (grass, dirt, stone)
   - **WaterLayer** - Water tiles only
   - **DecorationLayer** - Trees, rocks, objects
   - **CollisionLayer** - Collision data (optional)

3. **Configure sorting orders:**
   ```
   BaseTerrain:     Sorting Order = 0
   WaterLayer:      Sorting Order = 1
   DecorationLayer: Sorting Order = 2
   CollisionLayer:  Sorting Order = 3
   ```

### Step 3: Add TerrainLayerManager

1. **Add component to Grid GameObject:**
   - Select Grid in Hierarchy
   - Add Component > `TerrainLayerManager`

2. **Assign tilemap references:**
   ```
   TerrainLayerManager
   ├─ Base Terrain: [BaseTerrain Tilemap]
   ├─ Water Layer: [WaterLayer Tilemap]
   ├─ Decoration Layer: [DecorationLayer Tilemap]
   ├─ Collision Layer: [CollisionLayer Tilemap]
   ├─ Auto Generate Water Zones: ✓ true
   └─ Auto Generate Colliders: ✓ true
   ```

### Step 4: Paint Water Tiles

1. **Open Tile Palette:**
   ```
   Window > 2D > Tile Palette
   ```

2. **Create/Select a palette** and add your WaterTile assets

3. **Select WaterLayer tilemap** in the Hierarchy

4. **Paint water areas** - Connected water tiles will be grouped into zones

### Step 5: Generate Water Zones

Water zones generate automatically on scene start. To manually refresh:

**Option A - Play Mode:**
- Enter Play mode - zones generate on Start()

**Option B - Editor Mode:**
- Select Grid GameObject
- Right-click TerrainLayerManager component
- Click `Refresh All`

**What Happens:**
1. TerrainLayerManager scans WaterLayer for WaterTile instances
2. Connected tiles are grouped using flood-fill algorithm
3. A WaterZone GameObject is created for each group
4. PolygonCollider2D is added matching the water area shape
5. Zone is named based on water type and tile count

**Generated Zone Example:**
```
WaterZone_Pond (23 tiles)
├─ Transform: Position at zone center
├─ WaterZone component
│  ├─ Water Type: Pond
│  ├─ Zone Name: "Fishing Spot"
│  ├─ Base Success Rate: 0.7
│  └─ Available Fish: [Empty - needs configuration]
└─ PolygonCollider2D (Is Trigger: true)
```

### Step 6: Configure Generated Zones

After generation, select each WaterZone and configure:

1. **Set Zone Properties:**
   - Zone Name: "Quiet Pond", "Rushing River", etc.
   - Base Success Rate: 0.5-0.9 (higher = easier catches)
   - Current Weather: Match scene/time manager

2. **Add Fish Population:** (see [Configuring Fish Populations](#configuring-fish-populations))

---

## Method 2: Manual Water Zone Creation

Create custom water zones independent of tilemaps.

### Step 1: Create Water Zone GameObject

1. **Create empty GameObject:**
   ```
   Hierarchy > Right-click > Create Empty
   ```
   Name: "WaterZone_MyPond"

2. **Position** the GameObject where you want the fishing zone

### Step 2: Add WaterZone Component

1. **Add component:**
   ```
   Add Component > WaterZone
   ```

2. **Configure properties:**
   ```
   WaterZone
   ├─ Water Type: Pond / River / Lake / Ocean
   ├─ Zone Name: "My Fishing Spot"
   ├─ Base Success Rate: 0.7 (70% base catch chance)
   └─ Current Weather: Sunny
   ```

### Step 3: Add Trigger Collider

The player must be within the collider to fish:

1. **Add collider component:**
   ```
   Add Component > Box Collider 2D
   ```
   OR
   ```
   Add Component > Polygon Collider 2D (for complex shapes)
   ```

2. **Configure collider:**
   - **Is Trigger:** ✓ Checked (required!)
   - **Size/Points:** Define fishable area
   - Ensure collider is on appropriate layer

3. **Visual Guidance:**
   - Select WaterZone in Hierarchy
   - Blue wireframe shows collider boundaries in Scene view
   - Player must be within 3 units (fishing range) of water edge

### Step 4: Layer Configuration (Important!)

Ensure your water zones are detected:

1. **Set GameObject layer:**
   - Select WaterZone GameObject
   - Set Layer to "Water" (create if doesn't exist)

2. **Configure FishingController:**
   - Select Player GameObject
   - Find FishingController component
   - Set `Water Layer Mask` to include "Water" layer

---

## Configuring Fish Populations

Each WaterZone maintains its own fish population with spawn weights and availability.

### Understanding FishSpawnData

```csharp
[System.Serializable]
public class FishSpawnData
{
    public FishData fishData;           // The fish species
    public float spawnWeight = 1f;      // 0-1: Affects spawn probability
    public float currentPopulation = 1f; // 0-1: Depletes with fishing
}
```

### Adding Fish to a Zone

1. **Select WaterZone** in Hierarchy

2. **Expand "Available Fish" list**

3. **Click "+" to add fish entries**

4. **Configure each entry:**

**Example: Bluegill (Common)**
```
Fish Spawn Data [0]
├─ Fish Data: [Bluegill FishData asset]
├─ Spawn Weight: 1.0 (100% weight - very common)
└─ Current Population: 1.0 (fully populated)
```

**Example: Rainbow Trout (Uncommon)**
```
Fish Spawn Data [1]
├─ Fish Data: [Rainbow Trout FishData asset]
├─ Spawn Weight: 0.4 (40% weight - less common)
└─ Current Population: 1.0
```

**Example: Golden Trout (Legendary)**
```
Fish Spawn Data [2]
├─ Fish Data: [Golden Trout FishData asset]
├─ Spawn Weight: 0.05 (5% weight - very rare)
└─ Current Population: 0.8 (slightly depleted)
```

### Creating Fish Data Assets

If you need custom fish beyond the sample database:

1. **Create new FishData:**
   - Fish data is serializable, typically stored in FishDatabase
   - Edit `Assets/Data/ScriptableObjects/FishDatabase.asset`
   - Add entries directly in the Inspector

2. **Manual FishData Creation (alternative):**

While FishData is not a ScriptableObject itself, you can create custom entries:

**Option A - Via FishDatabase Context Menu:**
```
1. Select FishDatabase asset
2. Right-click > Create Sample Fish (generates 6 fish)
3. Modify the generated fish in the Inspector
```

**Option B - Add to Database Manually:**
```
1. Open FishDatabase asset
2. Expand "All Fish" list
3. Click "+" to add new entry
4. Configure fish properties
```

### Fish Property Reference

```
FishData Properties:
├─ Basic Info
│  ├─ Fish Name: "Largemouth Bass"
│  ├─ Fish Sprite: [bass_sprite]
│  └─ Description: "A popular sport fish"
│
├─ Fishing Properties
│  ├─ Rarity: 0.0-1.0 (0=common, 1=legendary)
│  ├─ Difficulty: 1-10 (affects catch success)
│  ├─ Min Weight: 1.0 kg
│  └─ Max Weight: 3.0 kg
│
├─ Value
│  └─ Base Value: 15 coins
│
└─ Conditions
   ├─ Available Times: [Dawn, Evening]
   ├─ Favored Weather: [Cloudy]
   └─ Water Types: [Lake, River]
```

### Spawn Weight Strategy

Spawn weights are relative to each other:

```
Total Weight = Sum of (spawnWeight × currentPopulation) for eligible fish

Example Zone:
- Bluegill:       1.0 weight → 66.7% chance
- Bass:           0.4 weight → 26.7% chance
- Golden Trout:   0.1 weight →  6.6% chance
Total:            1.5 weight
```

**Guidelines:**
- **Common fish:** 0.8 - 1.0
- **Uncommon fish:** 0.3 - 0.6
- **Rare fish:** 0.1 - 0.25
- **Legendary fish:** 0.01 - 0.1

### Environmental Filtering

Fish are filtered BEFORE spawn weight calculation:

**Time of Day Filtering:**
```csharp
// If fish has availableTimes specified, current time must match
Dawn fish won't spawn during Afternoon
```

**Weather Filtering:**
```csharp
// If fish has favoredWeather specified, current weather must match
Rainy-weather fish won't spawn on Sunny days
```

**Water Type Filtering:**
```csharp
// Fish must have matching water type
Ocean fish won't spawn in Ponds
```

**Example Configuration:**

```
Pond Zone (Sunny, Morning)
├─ Bluegill
│  ├─ Times: [Morning, Afternoon] ✓ Eligible
│  ├─ Weather: [Sunny, Cloudy]   ✓ Eligible
│  └─ Waters: [Pond, Lake]       ✓ Eligible
│
├─ Salmon
│  ├─ Times: [Dawn]              ✗ FILTERED OUT
│  ├─ Weather: [Rainy]           ✗ FILTERED OUT
│  └─ Waters: [River, Ocean]     ✗ FILTERED OUT
│
└─ Carp
   ├─ Times: [Morning, Afternoon, Evening] ✓ Eligible
   ├─ Weather: [Sunny, Cloudy]             ✓ Eligible
   └─ Waters: [Lake, River]                ✗ FILTERED OUT (Pond not listed)
```

Only Bluegill would be catchable in this scenario.

---

## Testing Fishing Zones

### In-Editor Testing

1. **Visual Verification:**
   - Select WaterZone GameObject
   - Gizmos show blue wireframe of collider area
   - Collider should cover water visuals

2. **Component Check:**
   - WaterZone component present
   - Collider2D with "Is Trigger" enabled
   - At least one fish in Available Fish list

### Play Mode Testing

**Step-by-step Test:**

1. **Enter Play Mode**

2. **Move player near water:**
   - Player must be within 3 units of water zone
   - Watch for auto-generated zones: Check Hierarchy for `WaterZone_*` objects

3. **Verify Detection:**
   - FishingController should detect water proximity
   - Interaction prompt should appear (if configured)

4. **Cast Fishing Line:**
   - Input: Left Click / Enter
   - Line should cast toward mouse position
   - Bobber should animate to target

5. **Wait for Bite:**
   - Random wait: 2-8 seconds
   - Bobber will bob when fish is biting

6. **Attempt Catch:**
   - Input: Left Click / Enter while bobbing
   - Check Console for catch results
   - FishingController.OnFishCaught event fires

### Debug Console Logging

Add debug logging to WaterZone for testing:

```csharp
// In WaterZone.TryGetFish() method
Debug.Log($"Eligible fish in {zoneName}: {eligibleFish.Count}");
Debug.Log($"Attempting catch in {waterType} zone");

// Add to FishingController.AttemptCatch()
if (caughtFish != null)
    Debug.Log($"Caught: {caughtFish.fishName}, Weight: {weight}kg");
else
    Debug.Log("Catch failed or no fish available");
```

### Common Issues During Testing

**Issue:** "No fish caught" every time
- **Check:** Available Fish list has entries
- **Check:** Fish conditions match current time/weather
- **Check:** Base Success Rate isn't too low
- **Check:** Fish difficulty isn't too high

**Issue:** Fishing doesn't start
- **Check:** Player within fishing range (3 units)
- **Check:** Water Layer Mask includes water zone layer
- **Check:** Input System actions are bound
- **Check:** Player isn't moving (blocks fishing start)

**Issue:** No water zones generated
- **Check:** TerrainLayerManager.autoGenerateWaterZones = true
- **Check:** WaterTile.generateWaterZone = true
- **Check:** WaterLayer tilemap has water tiles painted
- **Check:** Console for "Generated X water zones" message

---

## Advanced Configuration

### Dynamic Weather and Time

Integrate with GameTimeManager for dynamic conditions:

```csharp
// WaterZone already integrates with GameTimeManager
private GameTimeManager timeManager;

void Start()
{
    timeManager = FindObjectOfType<GameTimeManager>();
}

private TimeOfDay GetCurrentTimeOfDay()
{
    if (timeManager != null)
        return timeManager.GetCurrentTimeOfDay();
    return TimeOfDay.Morning; // fallback
}
```

To change weather dynamically:

```csharp
// From any script
WaterZone zone = GetComponent<WaterZone>();
zone.SetWeather(WeatherCondition.Rainy);
```

### Fish Population Depletion

Each successful catch reduces population:

```csharp
// In WaterZone.TryGetFish()
fishSpawn.currentPopulation = Mathf.Max(0.1f, fishSpawn.currentPopulation - 0.05f);
// Reduces by 5% per catch, minimum 10%
```

**Restore population over time:**

```csharp
// Call from GameManager or time system
void Update()
{
    if (gameTimeManager.IsNewDay())
    {
        foreach (WaterZone zone in FindObjectsOfType<WaterZone>())
            zone.RestockFish();
    }
}

// WaterZone.RestockFish() increases all populations by 10%
```

### Multiple Zones in One Area

Create layered fishing experiences:

```
Lake Scene
├─ ShallowWaterZone (near shore)
│  ├─ Water Type: Pond
│  └─ Fish: Bluegill, Sunfish (common, easy)
│
├─ DeepWaterZone (center of lake)
│  ├─ Water Type: Lake
│  └─ Fish: Bass, Trout (uncommon, harder)
│
└─ RareSpotZone (hidden area)
   ├─ Water Type: Lake
   └─ Fish: Salmon, Legendary fish (rare)
```

**Overlap Strategy:**
- Use smaller, overlapping colliders
- Player catches from the deepest overlapping zone
- FishingController uses first detected WaterZone

### Custom Success Rate Modifiers

Modify catch difficulty per zone:

```csharp
// In WaterZone
[SerializeField] private float baseSuccessRate = 0.7f;

// Factors affecting success:
// - Fish difficulty: Higher = harder (-5% per difficulty point)
// - Fish rarity: Higher = harder (-20% per rarity point)
// - Weather bonus: Favored weather = +15%
// - Time bonus: Preferred time = +10%
// - Water tile modifier: From WaterTile.fishingSuccessModifier
```

**Example Calculations:**

```
Catching Bass (Difficulty: 4, Rarity: 0.3) in Cloudy weather at Dawn:
Base:     0.7  (70%)
Difficulty: -0.15 (4-1) * 0.05
Rarity:   -0.06 (0.3 * 0.2)
Weather:  +0.15 (Bass favors Cloudy)
Time:     +0.10 (Bass prefers Dawn)
Total:    0.74  (74% success chance)
```

### Zone Information Display

Get zone info for UI:

```csharp
string info = waterZone.GetZoneInfo();
// Returns: "Quiet Pond (Pond)\nAvailable Species: 3"
```

### Linking Zones to Level Configuration

Reference zones in LevelConfiguration:

```csharp
// In LevelConfiguration.cs (extend if needed)
[Header("Fishing Zones")]
public WaterZone[] fishingZones;

// Assign in Inspector for each level
// Access via GameManager or level loader
```

---

## Troubleshooting

### Water Detection Issues

**Symptom:** Player can't start fishing near water

**Solutions:**

1. **Check Fishing Range:**
   ```csharp
   // In FishingController
   [SerializeField] private float fishingRange = 3f;
   ```
   Increase if player needs to be closer

2. **Verify Layer Mask:**
   ```
   FishingController > Water Layer Mask
   - Ensure "Water" layer is selected
   - Or layer 4 (default water layer)
   ```

3. **Check Collider Setup:**
   - WaterZone collider must have "Is Trigger" = true
   - Collider must be on correct layer
   - Collider should encompass water area

4. **Tilemap Integration:**
   - If using tilemaps, ensure TerrainLayerManager is configured
   - WaterLayer tilemap must be assigned
   - Water tiles must be WaterTile instances

### No Fish Spawning

**Symptom:** Player casts successfully but never catches fish

**Solutions:**

1. **Verify Fish Data:**
   ```
   WaterZone > Available Fish > Size > 0
   At least one fish entry required
   ```

2. **Check Fish Conditions:**
   ```
   FishData > Available Times: Must include current time
   FishData > Favored Weather: Must include current weather
   FishData > Water Types: Must include zone's water type
   ```
   **Fix:** Leave arrays empty to allow any condition

3. **Population Check:**
   ```
   Available Fish > Current Population > 0.1
   ```
   Call `RestockFish()` if depleted

4. **Success Rate Tuning:**
   ```
   WaterZone > Base Success Rate: Increase to 0.9 for testing
   FishData > Difficulty: Lower to 1-2 for testing
   FishData > Rarity: Lower to 0.1-0.3 for testing
   ```

### Performance Issues

**Symptom:** Frame drops with many water zones

**Solutions:**

1. **Limit Auto-Generation:**
   ```csharp
   // In TerrainLayerManager.GroupWaterTiles()
   // Add minimum size filter
   if (group.Count < 5)  // Skip tiny water zones
       continue;
   ```

2. **Use Fewer Zones:**
   - Merge small connected water areas
   - Disable auto-generation for decorative water
   - Use manual zones for key fishing spots only

3. **Optimize Colliders:**
   - Use BoxCollider2D instead of PolygonCollider2D where possible
   - Reduce collider complexity for large zones

### Animation/Visual Issues

**Symptom:** Water tiles not animating or displaying incorrectly

**Solutions:**

1. **Check WaterTile Configuration:**
   ```
   WaterTile > Animation Sprites: Assign sprite array
   WaterTile > Animation Speed: Try 0.5 - 2.0
   WaterTile > Random Start Frame: Enable for variety
   ```

2. **Refresh Tiles:**
   ```csharp
   // Call after changes
   waterLayer.RefreshAllTiles();
   ```

3. **Verify Sprite Import:**
   - Sprites must be set to "Sprite (2D and UI)"
   - Multiple mode for animation frames
   - Pixels Per Unit should match game settings

### Zone Generation Not Working

**Symptom:** No WaterZone GameObjects created in Hierarchy

**Solutions:**

1. **Enable Auto-Generation:**
   ```
   TerrainLayerManager > Auto Generate Water Zones: ✓ true
   ```

2. **Verify Water Tiles:**
   ```
   WaterTile asset > Generate Water Zone: ✓ true
   Tiles must be WaterTile type, not NormalTile
   ```

3. **Manual Refresh:**
   ```
   Select Grid GameObject
   TerrainLayerManager component > Right-click > Refresh All
   ```

4. **Check Console:**
   Look for debug messages:
   - "Searching for water tiles in bounds..."
   - "Found X water tiles"
   - "Generated X water zones total"

5. **Tilemap Issues:**
   - Ensure WaterLayer is assigned
   - Ensure tiles are actually painted (not just palette)
   - Check bounds: `WaterLayer.cellBounds` should cover painted area

### FishDatabase Not Found

**Symptom:** "FishDatabase is null" errors

**Solutions:**

1. **Resources Folder:**
   ```
   Move FishDatabase.asset to: Assets/Resources/
   GameManager loads from: Resources.Load<FishDatabase>("FishDatabase")
   ```

2. **Manual Assignment:**
   ```
   GameManager > Fish Database: [Drag FishDatabase asset]
   ```

3. **Create Database:**
   ```
   Assets > Create > Fishing Game > Fish Database
   Right-click asset > Create Sample Fish
   ```

---

## Quick Reference Checklist

### Creating Automatic Tilemap Zones

- [ ] Create WaterTile assets for each water type
- [ ] Set up Grid with tilemap layers (Base, Water, Decoration)
- [ ] Add TerrainLayerManager to Grid
- [ ] Assign all tilemap references
- [ ] Enable "Auto Generate Water Zones"
- [ ] Paint water tiles on WaterLayer
- [ ] Enter Play mode or manually refresh
- [ ] Configure generated zones with fish

### Creating Manual Zones

- [ ] Create empty GameObject
- [ ] Add WaterZone component
- [ ] Add Collider2D component (Is Trigger = true)
- [ ] Set GameObject layer to "Water"
- [ ] Configure water type and zone name
- [ ] Add fish to Available Fish list
- [ ] Verify FishingController water layer mask

### Fish Configuration

- [ ] Create/load FishDatabase asset
- [ ] Add fish data entries
- [ ] Set rarity, difficulty, weight ranges
- [ ] Configure time/weather/water type conditions
- [ ] Add fish to WaterZone Available Fish
- [ ] Set spawn weights (0.01-1.0 range)
- [ ] Set initial populations (usually 1.0)

### Testing

- [ ] Player has FishingController component
- [ ] Input System actions configured
- [ ] Move player within 3 units of water
- [ ] Cast line (Left Click/Enter)
- [ ] Wait for bite (2-8 seconds)
- [ ] Catch attempt (Left Click/Enter)
- [ ] Check Console for results
- [ ] Verify OnFishCaught event fires

---

## Example Scenarios

### Scenario 1: Simple Pond

**Goal:** Create a small pond with basic fish

```
1. Create WaterTile "PondWater"
   - Water Type: Pond
   - Fishing Success Modifier: 1.0

2. Paint 20-30 connected water tiles

3. Auto-generated zone appears: "WaterZone_Pond"

4. Configure zone:
   - Zone Name: "Quiet Pond"
   - Base Success Rate: 0.75
   - Available Fish:
     [0] Bluegill (weight: 1.0, population: 1.0)
     [1] Bass (weight: 0.3, population: 1.0)

5. Test: Should catch mostly Bluegill, occasional Bass
```

### Scenario 2: River with Time-Based Fish

**Goal:** Salmon spawn only at dawn

```
1. Create manual WaterZone GameObject

2. Add long BoxCollider2D following river path

3. Configure WaterZone:
   - Water Type: River
   - Zone Name: "Salmon Run"
   - Available Fish:
     [0] Common Carp (weight: 0.8)
        - Available Times: [Any]
     [1] Rainbow Trout (weight: 0.4)
        - Available Times: [Dawn, Morning]
     [2] Atlantic Salmon (weight: 0.1)
        - Available Times: [Dawn]
        - Favored Weather: [Rainy]

4. Result:
   - Morning: Carp + Trout available
   - Dawn: Carp + Trout + Salmon available
   - Dawn + Rainy: Salmon has +15% success bonus
```

### Scenario 3: Multi-Zone Lake

**Goal:** Different fish in shallow vs deep water

```
1. Paint large lake with WaterTiles

2. Disable auto-generation: TerrainLayerManager > Auto Generate = false

3. Create two manual WaterZones:

   ShallowZone:
   - Collider: Edges of lake
   - Water Type: Pond
   - Fish: Bluegill, Sunfish (easy, common)

   DeepZone:
   - Collider: Center of lake
   - Water Type: Lake
   - Fish: Bass, Trout, Salmon (harder, rare)

4. Player fishing from shore catches shallow fish
   Player in boat/center catches deep fish
```

---

## File Reference

### Key Script Paths
```
Assets/Scripts/Fishing/FishingController.cs
Assets/Scripts/Fishing/WaterZone.cs
Assets/Scripts/Data/FishData.cs
Assets/Scripts/Data/FishDatabase.cs
Assets/Scripts/Data/GameManager.cs
Assets/Scripts/Terrain/TerrainLayerManager.cs
Assets/Scripts/Tiles/WaterTile.cs
Assets/Scripts/Tiles/NormalTile.cs
```

### Asset Paths (Examples)
```
Assets/Data/ScriptableObjects/FishDatabase.asset
Assets/World/Tiles/PondWater.asset
Assets/World/Tiles/RiverWater.asset
Assets/World/Tiles/LakeWater.asset
Assets/Settings/Input/InputSystem_Actions.inputactions
Assets/Scenes/GameScene.unity
```

### Resources
```
Assets/Resources/FishDatabase.asset  (for auto-loading)
```

---

## Additional Resources

### Related Documentation
- `CLAUDE.md` - Project overview and architecture
- `Assets/UI/SETUP_GUIDE.md` - UI Toolkit integration
- Unity Tilemap Documentation: https://docs.unity3d.com/Manual/Tilemap.html
- Unity Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/

### Script API Reference

**WaterZone Methods:**
```csharp
FishData TryGetFish()              // Attempt to get a fish
void RestockFish()                 // Restore populations (+10%)
void SetWeather(WeatherCondition)  // Change weather condition
string GetZoneInfo()               // Get zone description
```

**FishingController Events:**
```csharp
System.Action<FishData, float> OnFishCaught  // (fishData, weight)
```

**TerrainLayerManager Methods:**
```csharp
bool IsWater(Vector3 worldPosition)
WaterType GetWaterType(Vector3 worldPosition)
void RefreshWaterZones()
void RefreshAll()  // Editor context menu
```

---

## Support and Debugging

### Enable Debug Logging

Add to your scripts for detailed output:

```csharp
// In WaterZone.TryGetFish()
Debug.Log($"[WaterZone] {zoneName}: Trying to catch fish");
Debug.Log($"[WaterZone] Eligible fish: {eligibleFish.Count}");
Debug.Log($"[WaterZone] Total spawn weight: {totalWeight}");

// In FishingController.AttemptCatch()
Debug.Log($"[Fishing] Attempt catch in zone: {currentWaterZone?.name}");
Debug.Log($"[Fishing] Result: {caughtFish?.fishName ?? "None"}");
```

### Visual Debugging

Enable Gizmos in Scene view:
- WaterZone: Blue cube/sphere showing collider
- FishingController: Blue wire sphere (fishing range), red sphere (cast target)

### Performance Profiling

If experiencing lag:
1. Window > Analysis > Profiler
2. Check "Scripts" section during fishing
3. Look for spikes in WaterZone.TryGetFish() or TerrainLayerManager

---

**Last Updated:** 2025-10-03
**Unity Version:** 6000.2.5f1
**Project:** Scratcher - 2D Isometric Fishing Game
