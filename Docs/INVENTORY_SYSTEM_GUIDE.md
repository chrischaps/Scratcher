# Inventory System Guide

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Prerequisites](#prerequisites)
4. [Setting Up the Inventory System](#setting-up-the-inventory-system)
5. [Working with Caught Fish](#working-with-caught-fish)
6. [Event-Driven Integration](#event-driven-integration)
7. [UI Integration](#ui-integration)
8. [Advanced Features](#advanced-features)
9. [Testing and Debugging](#testing-and-debugging)
10. [Troubleshooting](#troubleshooting)
11. [API Reference](#api-reference)

---

## Overview

The Inventory System in Scratcher manages caught fish, calculates their value, tracks statistics, and integrates with the fishing mechanics and UI systems through an event-driven architecture.

### Key Features
- **Fish Storage**: Manages caught fish with weight, value, and timestamp tracking
- **Dynamic Valuation**: Calculates fish value based on weight and base value
- **Event System**: C# Actions for reactive updates to UI and other systems
- **Statistics Tracking**: Monitors totals, records, and unique species
- **Capacity Management**: Configurable max inventory size with full/empty checks
- **Bulk Operations**: Sell individual fish or entire inventory at once
- **Automatic Integration**: Connects to FishingController via events

### Core Components
- **InventorySystem**: Main component managing all inventory operations
- **CaughtFish**: Serializable data class representing a caught fish instance
- **InventoryStats**: Statistics container for analytics and UI display

---

## System Architecture

### File Locations

**Core Script:**
```
Assets/Scripts/Data/InventorySystem.cs
```

**Related Systems:**
```
Assets/Scripts/Fishing/FishingController.cs (fish source)
Assets/Scripts/Data/FishData.cs (fish properties)
Assets/Scripts/Data/FishDatabase.cs (fish definitions)
Assets/UI/Scripts/GameHUDController.cs (UI display)
Assets/UI/Scripts/NotificationManager.cs (notifications)
```

### Data Flow

```
Player Catches Fish
    └─> FishingController.OnFishCaught event fires
        └─> InventorySystem.OnFishCaughtHandler()
            └─> InventorySystem.AddFish()
                ├─> Create CaughtFish instance
                ├─> Add to inventory list
                ├─> Calculate value
                ├─> Fire OnFishAdded event
                │   └─> NotificationManager displays toast
                ├─> Fire OnValueChanged event
                │   └─> UI updates total value display
                └─> Return success/failure

Player Sells Fish
    └─> InventorySystem.SellFish() or SellAllFish()
        ├─> Remove from inventory
        ├─> Update total value
        ├─> Fire OnFishSold event
        └─> Fire OnValueChanged event
```

### Event-Driven Architecture

```
InventorySystem Events:
├─ OnFishAdded(CaughtFish)
│  └─ Subscribers: NotificationManager, GameHUD, Achievement System
│
├─ OnFishSold(CaughtFish)
│  └─ Subscribers: Economy System, UI, Statistics
│
└─ OnValueChanged(int)
   └─ Subscribers: GameHUD, Save System, Economy UI
```

---

## Prerequisites

### 1. Fish Data System

Ensure you have a FishDatabase configured:

**Required Files:**
- `Assets/Scripts/Data/FishData.cs` - Fish data structure
- `Assets/Scripts/Data/FishDatabase.cs` - Fish database ScriptableObject
- `Assets/Data/ScriptableObjects/FishDatabase.asset` - Actual database asset

**Verify database exists:**
```
Check: Assets/Resources/FishDatabase.asset
or
Check: GameManager has FishDatabase reference assigned
```

### 2. Fishing Controller

The inventory connects to the fishing system:

**Required:**
- FishingController component on player or in scene
- FishingController must have `OnFishCaught` event implemented

**Check file:**
```csharp
// In FishingController.cs
public System.Action<FishData, float> OnFishCaught;
```

### 3. Game Manager

GameManager coordinates system initialization:

**Required:**
- GameManager in scene
- GameManager finds/references InventorySystem

### 4. UI System (Optional but Recommended)

For visual feedback:

**Recommended:**
- GameHUDController for inventory display
- NotificationManager for catch notifications
- UIIntegrationManager for automatic wiring

---

## Setting Up the Inventory System

### Step 1: Create Inventory GameObject

**Option A - Standalone GameObject:**

1. **Create GameObject:**
   ```
   Hierarchy > Right-click > Create Empty
   Name: "InventorySystem"
   ```

2. **Add Component:**
   ```
   Add Component > Inventory System
   ```

3. **Configure settings:**
   ```
   InventorySystem
   └─ Max Inventory Size: 50
   ```

**Option B - Attach to GameManager:**

1. **Select GameManager** in Hierarchy

2. **Add Component:**
   ```
   Add Component > Inventory System
   ```

3. **GameManager will auto-find it:**
   ```csharp
   // In GameManager.InitializeGame()
   if (inventorySystem == null)
       inventorySystem = FindObjectOfType<InventorySystem>();
   ```

### Step 2: Configure Inventory Settings

Select the InventorySystem GameObject:

```
InventorySystem Component
├─ Inventory Settings
│  └─ Max Inventory Size: 50 (default)
│
└─ Current Inventory (read-only in Inspector)
   ├─ Caught Fish: [empty initially]
   └─ Total Value: 0
```

**Max Inventory Size Guidelines:**
- **20-30**: Limited inventory, frequent selling required (hardcore)
- **50**: Default, balanced gameplay
- **100+**: Relaxed inventory management
- **Unlimited**: Set to int.MaxValue (2,147,483,647)

### Step 3: Connect to Fishing Controller

The inventory automatically subscribes to fishing events:

```csharp
// In InventorySystem.Start()
FishingController fishingController = FindObjectOfType<FishingController>();
if (fishingController != null)
{
    fishingController.OnFishCaught += OnFishCaughtHandler;
}
```

**Verify connection:**
1. Enter Play mode
2. Catch a fish
3. Check Console: "Caught [FishName] (X.Xkg) worth X coins!"

### Step 4: (Optional) Connect to UI

For automatic UI updates:

**If using UI Toolkit:**
```
UIIntegrationManager automatically connects:
- InventorySystem.OnFishAdded → NotificationManager
- InventorySystem.OnValueChanged → GameHUD
```

**Manual connection (if needed):**
```csharp
public class InventoryUIBridge : MonoBehaviour
{
    [SerializeField] private InventorySystem inventory;
    [SerializeField] private Text inventoryCountText;
    [SerializeField] private Text totalValueText;

    private void Start()
    {
        inventory.OnFishAdded += OnFishAdded;
        inventory.OnValueChanged += OnValueChanged;
    }

    private void OnFishAdded(CaughtFish fish)
    {
        inventoryCountText.text = $"Fish: {inventory.GetInventoryCount()}";
    }

    private void OnValueChanged(int newValue)
    {
        totalValueText.text = $"Value: {newValue} coins";
    }
}
```

---

## Working with Caught Fish

### CaughtFish Data Class

Each caught fish is stored as a CaughtFish instance:

```csharp
[System.Serializable]
public class CaughtFish
{
    public FishData fishData;        // The fish species
    public float weight;             // Actual caught weight
    public int value;                // Calculated coin value
    public System.DateTime dateCaught; // Timestamp
}
```

### Value Calculation

Fish value is calculated on catch:

```csharp
value = baseValue * (actualWeight / averageWeight)
```

**Formula breakdown:**
```
Base Value: From FishData.baseValue
Average Weight: (minWeight + maxWeight) / 2
Weight Multiplier: actualWeight / averageWeight

Example - Bluegill:
  Base Value: 10 coins
  Min Weight: 0.5 kg
  Max Weight: 1.5 kg
  Average: 1.0 kg

Caught at 1.5 kg:
  value = 10 * (1.5 / 1.0) = 15 coins

Caught at 0.5 kg:
  value = 10 * (0.5 / 1.0) = 5 coins
```

**Key insights:**
- Heavier fish = more valuable
- Trophy fish can be worth 2x+ base value
- Small fish may be worth less than base value

### Adding Fish Manually

Add fish programmatically:

```csharp
InventorySystem inventory = FindObjectOfType<InventorySystem>();
FishData bluegill = fishDatabase.GetFishByName("Bluegill");

bool success = inventory.AddFish(bluegill, 1.2f); // 1.2 kg

if (success)
{
    Debug.Log("Fish added successfully!");
}
else
{
    Debug.Log("Inventory full!");
}
```

### Retrieving Inventory Data

**Get all caught fish:**
```csharp
List<CaughtFish> allFish = inventory.GetCaughtFish();

foreach (CaughtFish fish in allFish)
{
    Debug.Log($"{fish.fishData.fishName}: {fish.weight}kg, {fish.value} coins");
}
```

**Get inventory count:**
```csharp
int count = inventory.GetInventoryCount();
Debug.Log($"Fish in inventory: {count}");
```

**Get total value:**
```csharp
int totalValue = inventory.GetTotalValue();
Debug.Log($"Total value: {totalValue} coins");
```

**Check if full:**
```csharp
bool isFull = inventory.IsInventoryFull();
if (isFull)
{
    Debug.Log("Can't catch more fish! Sell some first.");
}
```

### Selling Fish

**Sell individual fish:**
```csharp
CaughtFish fishToSell = allFish[0]; // First fish in inventory
bool sold = inventory.SellFish(fishToSell);

if (sold)
{
    Debug.Log($"Sold {fishToSell.fishData.fishName} for {fishToSell.value} coins");
}
```

**Sell all fish:**
```csharp
inventory.SellAllFish();
// Console: "Sold 15 fish for 450 coins total!"
```

### Querying Fish Records

**Get heaviest catch:**
```csharp
CaughtFish heaviest = inventory.GetHeaviestFish();
if (heaviest != null)
{
    Debug.Log($"Heaviest: {heaviest.fishData.fishName} at {heaviest.weight}kg");
}
```

**Get most valuable catch:**
```csharp
CaughtFish valuable = inventory.GetMostValuableFish();
if (valuable != null)
{
    Debug.Log($"Most valuable: {valuable.fishData.fishName} worth {valuable.value} coins");
}
```

**Get all fish of a type:**
```csharp
List<CaughtFish> allBass = inventory.GetFishByType("Largemouth Bass");
Debug.Log($"Caught {allBass.Count} bass total");
```

**Get fish counts by species:**
```csharp
Dictionary<string, int> counts = inventory.GetFishCounts();

foreach (var kvp in counts)
{
    Debug.Log($"{kvp.Key}: {kvp.Value} caught");
}

// Output:
// Bluegill: 12 caught
// Bass: 5 caught
// Trout: 3 caught
```

### Statistics System

Get comprehensive inventory statistics:

```csharp
InventoryStats stats = inventory.GetStats();

Debug.Log($"Total Fish: {stats.totalFishCaught}");
Debug.Log($"Total Value: {stats.totalValue}");
Debug.Log($"Unique Species: {stats.uniqueSpecies}");
Debug.Log($"Heaviest: {stats.heaviestFish?.fishData.fishName}");
Debug.Log($"Most Valuable: {stats.mostValuableFish?.fishData.fishName}");
```

---

## Event-Driven Integration

### Understanding the Event System

InventorySystem uses C# Actions for loose coupling:

```csharp
public System.Action<CaughtFish> OnFishAdded;
public System.Action<CaughtFish> OnFishSold;
public System.Action<int> OnValueChanged;
```

**Benefits:**
- Multiple systems can react to inventory changes
- No tight coupling between systems
- Easy to add new subscribers
- Event-driven UI updates

### Subscribing to Events

**Subscribe in Start():**
```csharp
public class MySystem : MonoBehaviour
{
    private InventorySystem inventory;

    private void Start()
    {
        inventory = FindObjectOfType<InventorySystem>();

        // Subscribe to events
        inventory.OnFishAdded += HandleFishAdded;
        inventory.OnFishSold += HandleFishSold;
        inventory.OnValueChanged += HandleValueChanged;
    }

    private void HandleFishAdded(CaughtFish fish)
    {
        Debug.Log($"Caught: {fish.fishData.fishName}");
    }

    private void HandleFishSold(CaughtFish fish)
    {
        Debug.Log($"Sold: {fish.fishData.fishName}");
    }

    private void HandleValueChanged(int newValue)
    {
        Debug.Log($"New total value: {newValue}");
    }

    private void OnDestroy()
    {
        // Always unsubscribe!
        if (inventory != null)
        {
            inventory.OnFishAdded -= HandleFishAdded;
            inventory.OnFishSold -= HandleFishSold;
            inventory.OnValueChanged -= HandleValueChanged;
        }
    }
}
```

### Integration Examples

**Achievement System:**
```csharp
public class AchievementSystem : MonoBehaviour
{
    private void Start()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        inventory.OnFishAdded += CheckAchievements;
    }

    private void CheckAchievements(CaughtFish fish)
    {
        // Check for first catch
        if (inventory.GetInventoryCount() == 1)
        {
            UnlockAchievement("First Catch");
        }

        // Check for legendary catch
        if (fish.fishData.rarity >= 0.9f)
        {
            UnlockAchievement("Legendary Fisher");
        }

        // Check for 100 fish milestone
        if (inventory.GetInventoryCount() >= 100)
        {
            UnlockAchievement("Century Catch");
        }
    }
}
```

**Economy System:**
```csharp
public class EconomySystem : MonoBehaviour
{
    private int playerCoins = 0;

    private void Start()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        inventory.OnFishSold += AddCoins;
    }

    private void AddCoins(CaughtFish fish)
    {
        playerCoins += fish.value;
        Debug.Log($"Earned {fish.value} coins! Total: {playerCoins}");
    }
}
```

**Analytics System:**
```csharp
public class AnalyticsTracker : MonoBehaviour
{
    private void Start()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        inventory.OnFishAdded += TrackFishCatch;
    }

    private void TrackFishCatch(CaughtFish fish)
    {
        // Send to analytics service
        Analytics.LogEvent("fish_caught", new Dictionary<string, object>
        {
            {"species", fish.fishData.fishName},
            {"weight", fish.weight},
            {"value", fish.value},
            {"timestamp", fish.dateCaught}
        });
    }
}
```

---

## UI Integration

### GameHUD Integration

The UI Toolkit GameHUD automatically displays inventory info:

**Automatic Updates:**
```
InventorySystem → OnValueChanged event
    └─> UIIntegrationManager.OnInventoryValueChanged()
        └─> GameHUD updates total value display
```

**Manual UI update:**
```csharp
public class GameHUDController : UIToolkitPanel
{
    private Label inventoryCountLabel;
    private Label totalValueLabel;

    protected override void SetupUI()
    {
        inventoryCountLabel = root.Q<Label>("inventory-count");
        totalValueLabel = root.Q<Label>("total-value");

        // Subscribe to inventory events
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        if (inventory != null)
        {
            inventory.OnFishAdded += UpdateInventoryDisplay;
            inventory.OnFishSold += UpdateInventoryDisplay;
        }
    }

    private void UpdateInventoryDisplay(CaughtFish fish)
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        inventoryCountLabel.text = $"Fish: {inventory.GetInventoryCount()}";
        totalValueLabel.text = $"Value: {inventory.GetTotalValue()}";
    }
}
```

### Notification System Integration

Show toast notifications for fish catches:

**Automatic (via UIIntegrationManager):**
```csharp
// In UIIntegrationManager.cs
private void OnFishAdded(CaughtFish fish)
{
    if (NotificationManager.Instance != null)
    {
        NotificationManager.Instance.ShowFishCaughtNotification(
            fish.fishData.fishName,
            fish.weight,
            fish.value
        );
    }
}
```

**Manual notification:**
```csharp
NotificationManager.Instance.ShowNotification(
    "Fish Caught!",
    $"Caught {fish.fishData.fishName} ({fish.weight:F1}kg) worth {fish.value} coins",
    NotificationManager.NotificationType.Success,
    3f
);
```

### Inventory Panel UI

Create a detailed inventory display:

```csharp
public class InventoryPanelController : MonoBehaviour
{
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject fishItemPrefab;

    private InventorySystem inventory;

    private void Start()
    {
        inventory = FindObjectOfType<InventorySystem>();
        inventory.OnFishAdded += RefreshDisplay;
        inventory.OnFishSold += RefreshDisplay;

        RefreshDisplay(null);
    }

    private void RefreshDisplay(CaughtFish fish)
    {
        // Clear existing items
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        // Create UI for each fish
        List<CaughtFish> allFish = inventory.GetCaughtFish();
        foreach (CaughtFish caughtFish in allFish)
        {
            GameObject item = Instantiate(fishItemPrefab, inventoryGrid);
            FishItemUI itemUI = item.GetComponent<FishItemUI>();
            itemUI.Setup(caughtFish, inventory);
        }
    }
}

public class FishItemUI : MonoBehaviour
{
    [SerializeField] private Image fishIcon;
    [SerializeField] private Text fishNameText;
    [SerializeField] private Text weightText;
    [SerializeField] private Text valueText;
    [SerializeField] private Button sellButton;

    private CaughtFish fish;
    private InventorySystem inventory;

    public void Setup(CaughtFish caughtFish, InventorySystem inv)
    {
        fish = caughtFish;
        inventory = inv;

        fishIcon.sprite = fish.fishData.fishSprite;
        fishNameText.text = fish.fishData.fishName;
        weightText.text = $"{fish.weight:F2} kg";
        valueText.text = $"{fish.value} coins";

        sellButton.onClick.AddListener(SellFish);
    }

    private void SellFish()
    {
        inventory.SellFish(fish);
    }
}
```

---

## Advanced Features

### Inventory Capacity Management

**Check before fishing:**
```csharp
public class FishingController : MonoBehaviour
{
    private InventorySystem inventory;

    private void Start()
    {
        inventory = FindObjectOfType<InventorySystem>();
    }

    public bool CanCatchFish()
    {
        if (inventory.IsInventoryFull())
        {
            NotificationManager.Instance.ShowNotification(
                "Inventory Full",
                "Sell some fish to make room!",
                NotificationManager.NotificationType.Warning,
                3f
            );
            return false;
        }
        return true;
    }

    private void AttemptCatch()
    {
        if (!CanCatchFish())
            return;

        // Continue with fishing logic...
    }
}
```

### Inventory Sorting

Sort caught fish by various criteria:

```csharp
public List<CaughtFish> GetSortedInventory(SortCriteria criteria)
{
    List<CaughtFish> sorted = inventory.GetCaughtFish();

    switch (criteria)
    {
        case SortCriteria.Weight:
            sorted = sorted.OrderByDescending(f => f.weight).ToList();
            break;

        case SortCriteria.Value:
            sorted = sorted.OrderByDescending(f => f.value).ToList();
            break;

        case SortCriteria.DateCaught:
            sorted = sorted.OrderByDescending(f => f.dateCaught).ToList();
            break;

        case SortCriteria.Species:
            sorted = sorted.OrderBy(f => f.fishData.fishName).ToList();
            break;
    }

    return sorted;
}

public enum SortCriteria
{
    Weight,
    Value,
    DateCaught,
    Species
}
```

### Selective Selling

Sell fish based on criteria:

```csharp
public void SellFishBelowValue(int minValue)
{
    List<CaughtFish> allFish = inventory.GetCaughtFish();
    List<CaughtFish> toSell = allFish.Where(f => f.value < minValue).ToList();

    foreach (CaughtFish fish in toSell)
    {
        inventory.SellFish(fish);
    }

    Debug.Log($"Sold {toSell.Count} low-value fish");
}

public void SellDuplicates(int keepCount)
{
    Dictionary<string, int> counts = inventory.GetFishCounts();

    foreach (var kvp in counts)
    {
        if (kvp.Value > keepCount)
        {
            List<CaughtFish> species = inventory.GetFishByType(kvp.Key);

            // Keep the best ones, sell the rest
            var sorted = species.OrderByDescending(f => f.value).ToList();
            for (int i = keepCount; i < sorted.Count; i++)
            {
                inventory.SellFish(sorted[i]);
            }
        }
    }
}
```

### Save/Load Integration

Serialize inventory for save system:

```csharp
[System.Serializable]
public class InventorySaveData
{
    public List<CaughtFishSaveData> caughtFish;
    public int totalValue;
}

[System.Serializable]
public class CaughtFishSaveData
{
    public string fishName;
    public float weight;
    public int value;
    public string dateCaught;
}

public class SaveSystem : MonoBehaviour
{
    public InventorySaveData SaveInventory(InventorySystem inventory)
    {
        InventorySaveData saveData = new InventorySaveData();
        saveData.caughtFish = new List<CaughtFishSaveData>();

        foreach (CaughtFish fish in inventory.GetCaughtFish())
        {
            CaughtFishSaveData fishData = new CaughtFishSaveData
            {
                fishName = fish.fishData.fishName,
                weight = fish.weight,
                value = fish.value,
                dateCaught = fish.dateCaught.ToString()
            };
            saveData.caughtFish.Add(fishData);
        }

        saveData.totalValue = inventory.GetTotalValue();
        return saveData;
    }

    public void LoadInventory(InventorySystem inventory, InventorySaveData saveData)
    {
        FishDatabase database = Resources.Load<FishDatabase>("FishDatabase");

        foreach (CaughtFishSaveData fishData in saveData.caughtFish)
        {
            FishData fish = database.GetFishByName(fishData.fishName);
            if (fish != null)
            {
                inventory.AddFish(fish, fishData.weight);
            }
        }
    }
}
```

### Daily Catch Limit

Implement catch limits:

```csharp
public class CatchLimitManager : MonoBehaviour
{
    [SerializeField] private int dailyCatchLimit = 20;
    private int todaysCatches = 0;
    private int lastDay = 0;

    private void Start()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        inventory.OnFishAdded += OnFishCaught;

        GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnDayChanged += OnNewDay;
    }

    private void OnFishCaught(CaughtFish fish)
    {
        todaysCatches++;

        if (todaysCatches >= dailyCatchLimit)
        {
            NotificationManager.Instance.ShowNotification(
                "Daily Limit Reached",
                "You've caught your limit for today!",
                NotificationManager.NotificationType.Warning,
                4f
            );
        }
    }

    private void OnNewDay(int newDay)
    {
        todaysCatches = 0;
        Debug.Log("Daily catch limit reset!");
    }

    public bool CanCatchMore()
    {
        return todaysCatches < dailyCatchLimit;
    }
}
```

---

## Testing and Debugging

### Debug Tools

Add debug methods to InventorySystem:

```csharp
#if UNITY_EDITOR
[ContextMenu("Debug: Add Random Fish")]
private void DebugAddRandomFish()
{
    FishDatabase db = Resources.Load<FishDatabase>("FishDatabase");
    if (db != null && db.allFish.Count > 0)
    {
        FishData randomFish = db.allFish[Random.Range(0, db.allFish.Count)];
        float randomWeight = Random.Range(randomFish.minWeight, randomFish.maxWeight);
        AddFish(randomFish, randomWeight);
    }
}

[ContextMenu("Debug: Fill Inventory")]
private void DebugFillInventory()
{
    for (int i = 0; i < maxInventorySize; i++)
    {
        DebugAddRandomFish();
    }
}

[ContextMenu("Debug: Clear Inventory")]
private void DebugClearInventory()
{
    caughtFish.Clear();
    totalValue = 0;
    OnValueChanged?.Invoke(totalValue);
}

[ContextMenu("Debug: Print Statistics")]
private void DebugPrintStatistics()
{
    InventoryStats stats = GetStats();
    Debug.Log($"=== Inventory Statistics ===");
    Debug.Log($"Total Fish: {stats.totalFishCaught}");
    Debug.Log($"Total Value: {stats.totalValue} coins");
    Debug.Log($"Unique Species: {stats.uniqueSpecies}");
    Debug.Log($"Heaviest: {stats.heaviestFish?.fishData.fishName} ({stats.heaviestFish?.weight}kg)");
    Debug.Log($"Most Valuable: {stats.mostValuableFish?.fishData.fishName} ({stats.mostValuableFish?.value} coins)");
}
#endif
```

### Unit Testing

Create test cases for inventory:

```csharp
using NUnit.Framework;
using UnityEngine;

public class InventorySystemTests
{
    private InventorySystem inventory;
    private FishData testFish;

    [SetUp]
    public void Setup()
    {
        GameObject obj = new GameObject();
        inventory = obj.AddComponent<InventorySystem>();

        testFish = ScriptableObject.CreateInstance<FishData>();
        testFish.fishName = "Test Fish";
        testFish.baseValue = 10;
        testFish.minWeight = 1f;
        testFish.maxWeight = 3f;
    }

    [Test]
    public void AddFish_IncreasesCount()
    {
        inventory.AddFish(testFish, 2f);
        Assert.AreEqual(1, inventory.GetInventoryCount());
    }

    [Test]
    public void AddFish_CalculatesValueCorrectly()
    {
        inventory.AddFish(testFish, 2f); // 2kg, average is 2kg
        Assert.AreEqual(10, inventory.GetTotalValue());
    }

    [Test]
    public void AddFish_FullInventory_ReturnsFalse()
    {
        // Fill inventory
        for (int i = 0; i < 50; i++)
        {
            inventory.AddFish(testFish, 2f);
        }

        // Try to add one more
        bool result = inventory.AddFish(testFish, 2f);
        Assert.IsFalse(result);
    }

    [Test]
    public void SellFish_RemovesFromInventory()
    {
        inventory.AddFish(testFish, 2f);
        CaughtFish fish = inventory.GetCaughtFish()[0];

        inventory.SellFish(fish);
        Assert.AreEqual(0, inventory.GetInventoryCount());
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(inventory.gameObject);
        Object.DestroyImmediate(testFish);
    }
}
```

### Event Testing

Verify events fire correctly:

```csharp
[Test]
public void OnFishAdded_FiresWhenFishAdded()
{
    bool eventFired = false;
    inventory.OnFishAdded += (fish) => eventFired = true;

    inventory.AddFish(testFish, 2f);

    Assert.IsTrue(eventFired);
}

[Test]
public void OnValueChanged_FiresWithCorrectValue()
{
    int receivedValue = 0;
    inventory.OnValueChanged += (value) => receivedValue = value;

    inventory.AddFish(testFish, 2f);

    Assert.AreEqual(10, receivedValue);
}
```

---

## Troubleshooting

### Fish Not Being Added

**Symptom:** Catch fish but inventory stays empty

**Solutions:**

1. **Check event subscription:**
   ```csharp
   // In InventorySystem.Start()
   FishingController fishingController = FindObjectOfType<FishingController>();
   if (fishingController != null)
   {
       fishingController.OnFishCaught += OnFishCaughtHandler;
   }
   ```

2. **Verify FishingController event fires:**
   ```csharp
   // In FishingController.AttemptCatch()
   OnFishCaught?.Invoke(caughtFish, weight);
   ```

3. **Check Console for errors:**
   Look for null reference exceptions

4. **Verify inventory isn't full:**
   ```csharp
   Debug.Log($"Inventory: {inventory.GetInventoryCount()}/{maxInventorySize}");
   ```

### Events Not Firing

**Symptom:** Subscribe to events but handlers never called

**Solutions:**

1. **Ensure proper subscription syntax:**
   ```csharp
   inventory.OnFishAdded += HandleFishAdded; // Correct
   // NOT:
   // inventory.OnFishAdded = HandleFishAdded; // Wrong - replaces all subscribers
   ```

2. **Subscribe before events can fire:**
   Subscribe in Start() or Awake(), not Update()

3. **Check event is invoked:**
   ```csharp
   // In InventorySystem.AddFish()
   OnFishAdded?.Invoke(newCatch); // ? ensures null check
   ```

4. **Verify subscriber still exists:**
   If subscriber is destroyed, event won't fire

### Incorrect Fish Values

**Symptom:** Fish values seem wrong

**Solutions:**

1. **Check FishData.baseValue:**
   ```
   Select fish asset > Base Value: Should be set
   ```

2. **Verify weight range:**
   ```
   Min Weight: 0.5
   Max Weight: 3.0
   Caught Weight: Should be between these
   ```

3. **Debug value calculation:**
   ```csharp
   float avgWeight = (fish.minWeight + fish.maxWeight) * 0.5f;
   float multiplier = weight / avgWeight;
   int calculatedValue = Mathf.RoundToInt(fish.baseValue * multiplier);
   Debug.Log($"Base:{fish.baseValue} * Mult:{multiplier} = {calculatedValue}");
   ```

### Memory Leaks from Events

**Symptom:** Performance degrades over time

**Solutions:**

1. **Always unsubscribe in OnDestroy:**
   ```csharp
   private void OnDestroy()
   {
       if (inventory != null)
       {
           inventory.OnFishAdded -= HandleFishAdded;
           inventory.OnFishSold -= HandleFishSold;
           inventory.OnValueChanged -= HandleValueChanged;
       }
   }
   ```

2. **Use weak references for long-lived subscribers:**
   ```csharp
   // Advanced: Use weak event pattern if needed
   ```

3. **Profile memory usage:**
   ```
   Window > Analysis > Profiler > Memory
   Look for growing object counts
   ```

### Inventory Data Doesn't Persist

**Symptom:** Inventory resets on scene reload

**Solutions:**

1. **Implement save/load system:**
   See [Save/Load Integration](#saveload-integration) above

2. **Use DontDestroyOnLoad if appropriate:**
   ```csharp
   private void Awake()
   {
       DontDestroyOnLoad(gameObject);
   }
   ```
   (Note: Usually GameManager handles this)

3. **Verify save triggers:**
   Save on scene change, not just on quit

---

## API Reference

### InventorySystem Methods

#### AddFish(FishData fishData, float weight)
```csharp
public bool AddFish(FishData fishData, float weight)
```

Adds a caught fish to inventory.

**Parameters:**
- `fishData`: The fish species data
- `weight`: Actual weight caught (kg)

**Returns:** `true` if added successfully, `false` if inventory full

**Events Fired:**
- `OnFishAdded(CaughtFish)`
- `OnValueChanged(int)`

**Example:**
```csharp
FishData bass = fishDatabase.GetFishByName("Bass");
if (inventory.AddFish(bass, 2.5f))
{
    Debug.Log("Fish added!");
}
```

#### SellFish(CaughtFish fish)
```csharp
public bool SellFish(CaughtFish fish)
```

Sells a specific fish from inventory.

**Parameters:**
- `fish`: The CaughtFish instance to sell

**Returns:** `true` if sold, `false` if fish not in inventory

**Events Fired:**
- `OnFishSold(CaughtFish)`
- `OnValueChanged(int)`

#### SellAllFish()
```csharp
public void SellAllFish()
```

Sells entire inventory at once.

**Events Fired:**
- `OnFishSold(CaughtFish)` for each fish
- `OnValueChanged(int)` once at end

#### GetCaughtFish()
```csharp
public List<CaughtFish> GetCaughtFish()
```

Returns a copy of the caught fish list.

**Returns:** New list containing all caught fish

**Note:** Returns a copy, not the original list. Safe to iterate and modify.

#### GetInventoryCount()
```csharp
public int GetInventoryCount()
```

Returns number of fish in inventory.

#### GetTotalValue()
```csharp
public int GetTotalValue()
```

Returns combined value of all fish in inventory.

#### IsInventoryFull()
```csharp
public bool IsInventoryFull()
```

Checks if inventory is at max capacity.

#### GetHeaviestFish()
```csharp
public CaughtFish GetHeaviestFish()
```

Returns the heaviest fish caught, or null if inventory empty.

#### GetMostValuableFish()
```csharp
public CaughtFish GetMostValuableFish()
```

Returns the most valuable fish caught, or null if empty.

#### GetFishByType(string fishName)
```csharp
public List<CaughtFish> GetFishByType(string fishName)
```

Returns all caught fish of a specific species.

#### GetFishCounts()
```csharp
public Dictionary<string, int> GetFishCounts()
```

Returns count of each species in inventory.

**Example:**
```csharp
var counts = inventory.GetFishCounts();
foreach (var kvp in counts)
{
    Debug.Log($"{kvp.Key}: {kvp.Value}");
}
```

#### GetStats()
```csharp
public InventoryStats GetStats()
```

Returns comprehensive inventory statistics.

**Returns:** InventoryStats containing:
- `totalFishCaught`: Total count
- `totalValue`: Combined value
- `uniqueSpecies`: Number of different species
- `heaviestFish`: Heaviest catch
- `mostValuableFish`: Most valuable catch

### CaughtFish Class

```csharp
public class CaughtFish
{
    public FishData fishData;        // Fish species
    public float weight;             // Weight in kg
    public int value;                // Calculated value
    public System.DateTime dateCaught; // When caught
}
```

**Constructor:**
```csharp
public CaughtFish(FishData fish, float fishWeight)
```

Automatically calculates value based on weight.

### InventoryStats Class

```csharp
public class InventoryStats
{
    public int totalFishCaught;
    public int totalValue;
    public int uniqueSpecies;
    public CaughtFish heaviestFish;
    public CaughtFish mostValuableFish;
}
```

### Events

#### OnFishAdded
```csharp
public System.Action<CaughtFish> OnFishAdded;
```

Fires when fish is added to inventory.

**Subscribe:**
```csharp
inventory.OnFishAdded += (fish) => {
    Debug.Log($"Caught {fish.fishData.fishName}!");
};
```

#### OnFishSold
```csharp
public System.Action<CaughtFish> OnFishSold;
```

Fires when fish is sold.

#### OnValueChanged
```csharp
public System.Action<int> OnValueChanged;
```

Fires when total inventory value changes.

**Parameter:** New total value

---

## Quick Reference Checklist

### Initial Setup
- [ ] Create InventorySystem GameObject or attach to GameManager
- [ ] Configure max inventory size
- [ ] Verify FishingController exists with OnFishCaught event
- [ ] Test fish can be added

### Integration
- [ ] Subscribe to OnFishAdded for notifications
- [ ] Subscribe to OnValueChanged for UI updates
- [ ] Subscribe to OnFishSold for economy system
- [ ] Unsubscribe in OnDestroy()

### UI Setup
- [ ] Connect to GameHUD for inventory display
- [ ] Connect to NotificationManager for catch feedback
- [ ] Create inventory panel for detailed view
- [ ] Add sell buttons/functionality

### Testing
- [ ] Catch fish and verify inventory increases
- [ ] Sell fish and verify inventory decreases
- [ ] Check Console for catch messages
- [ ] Verify events fire correctly
- [ ] Test inventory full state

---

**Last Updated:** 2025-10-03
**Unity Version:** 6000.2.5f1
**Project:** Scratcher - 2D Isometric Fishing Game
