# UI Toolkit System Guide

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Prerequisites](#prerequisites)
4. [Quick Start Setup](#quick-start-setup)
5. [Core UI Components](#core-ui-components)
6. [UI Integration System](#ui-integration-system)
7. [Data Binding and Reactive Updates](#data-binding-and-reactive-updates)
8. [Notification System](#notification-system)
9. [Debug Panel](#debug-panel)
10. [Customization and Extension](#customization-and-extension)
11. [Styling with USS](#styling-with-uss)
12. [Testing and Debugging](#testing-and-debugging)
13. [Performance Optimization](#performance-optimization)
14. [Troubleshooting](#troubleshooting)
15. [API Reference](#api-reference)

---

## Overview

The UI Toolkit System provides a modern, performant UI implementation for Scratcher using Unity's UI Toolkit (formerly UIElements). This system replaces legacy Canvas-based UI with a hardware-accelerated, CSS-like styling approach.

### Key Features
- **Hardware-Accelerated Rendering**: Better performance than traditional Canvas UI
- **CSS-Like Styling**: Familiar USS (Unity Style Sheets) workflow
- **Reactive Data Binding**: Automatic UI updates when game state changes
- **Modular Panel System**: Easy to add, remove, and manage UI panels
- **Event-Driven Architecture**: Loose coupling between UI and game systems
- **Debug Tools**: Comprehensive F1 debug panel with system monitoring
- **Responsive Design**: Flexbox layouts that adapt to screen sizes
- **Toast Notifications**: Animated notification system with sound support

### Core Components
- **UIManager**: Centralized UI state management and panel registry
- **UIIntegrationManager**: Connects UI Toolkit with existing game systems
- **UIToolkitPanel**: Base class for all UI panels
- **GameHUDController**: Main game interface (time, inventory, stats)
- **DebugUIManager**: 5-tabbed debug interface
- **MainMenuController**: Main menu system
- **NotificationManager**: Toast notification system
- **DataBindingHelper**: Utility for reactive UI updates

---

## System Architecture

### File Locations

**Core Scripts:**
```
Assets/UI/Scripts/UIManager.cs
Assets/UI/Scripts/UIIntegrationManager.cs
Assets/UI/Scripts/UIToolkitPanel.cs
Assets/UI/Scripts/DataBindingHelper.cs
Assets/UI/Scripts/GameHUDController.cs
Assets/UI/Scripts/DebugUIManager.cs
Assets/UI/Scripts/MainMenuController.cs
Assets/UI/Scripts/NotificationManager.cs
```

**UI Layout Files (UXML):**
```
Assets/UI/UXML/GameHUD.uxml
Assets/UI/UXML/DebugPanel.uxml
Assets/UI/UXML/MainMenu.uxml
```

**Style Sheets (USS):**
```
Assets/UI/USS/MainHUD.uss
Assets/UI/USS/DebugUI.uss
Assets/UI/USS/MainMenu.uss
```

**Supporting Documentation:**
```
Assets/UI/SETUP_GUIDE.md
Docs/UI_TOOLKIT_SYSTEM_GUIDE.md (this file)
```

### System Hierarchy

```
UI System Architecture
├─ UIManager (Singleton)
│  ├─ Panel Registry
│  ├─ State Management
│  └─ Input Handling (ESC to close panels)
│
├─ UIIntegrationManager
│  ├─ Connects Game Systems to UI
│  ├─ Event Subscriptions
│  └─ Automatic Panel Setup
│
├─ UI Panels
│  ├─ GameHUDController
│  │  ├─ Time Display
│  │  ├─ Inventory Count
│  │  ├─ Player Stats
│  │  └─ Fishing Overlay
│  │
│  ├─ DebugUIManager
│  │  ├─ System Tab (FPS, Memory)
│  │  ├─ Game State Tab (Time, Player)
│  │  ├─ Fishing Tab (Zones, Database)
│  │  ├─ Terrain Tab (Generation)
│  │  └─ Cheats Tab (Dev Tools)
│  │
│  └─ MainMenuController
│     ├─ Start Game
│     ├─ Settings
│     ├─ Save/Load
│     └─ Credits
│
└─ NotificationManager (Singleton)
   ├─ Toast Notifications
   ├─ Type-Based Styling
   └─ Animation Support
```

### Data Flow

```
Game Event Occurs
    └─> Event Fired (e.g., OnFishAdded)
        └─> UIIntegrationManager Receives Event
            └─> Updates Relevant UI Panel
                └─> DataBindingHelper Updates Visual Elements
                    └─> UI Updates in Real-Time
                        └─> NotificationManager Shows Toast (if applicable)
```

---

## Prerequisites

Before setting up the UI Toolkit system, ensure you have:

### 1. Unity Version
- **Unity 6000.2.5f1** or later (UI Toolkit is built-in)
- Universal Render Pipeline (URP) 17.2.0 or later

### 2. Input System
The UI system uses Unity's new Input System with these bindings:
- **F1 Key**: Toggle debug panel (configured in `InputSystem_Actions.inputactions`)
- **ESC Key**: Close top UI panel

### 3. Required Packages
```json
{
  "com.unity.ugui": "2.0.0",           // For UI components
  "com.unity.inputsystem": "1.14.2"   // For input handling
}
```

### 4. Existing Game Systems
The UI integrates with these existing systems:
- **GameManager**: Central game coordination
- **InventorySystem**: Fish inventory and storage
- **GameTimeManager**: Time progression and day/night cycle
- **FishingController**: Fishing mechanics
- **TerrainLayerManager**: Terrain and water zones

---

## Quick Start Setup

### Method 1: Automatic Setup (Recommended)

1. **Add UIIntegrationManager to Scene:**
   ```
   1. Create empty GameObject named "UI System"
   2. Add Component → UIIntegrationManager
   3. Configure settings in Inspector:
      - Enable Debug Mode: true (for debug panel)
      - Replace Old UI: true (disables legacy UI)
   ```

2. **Play the Scene:**
   - UI system automatically creates GameHUD
   - Press **F1** to open debug panel
   - Notifications appear when events occur (fish caught, day changed, etc.)

### Method 2: Manual Setup

1. **Create GameHUD:**
   ```
   1. Create GameObject named "GameHUD"
   2. Add Component → UIDocument
   3. Assign Visual Tree Asset: GameHUD.uxml
   4. Add Component → GameHUDController
   ```

2. **Create Debug Panel:**
   ```
   1. Create GameObject named "DebugPanel"
   2. Add Component → UIDocument
   3. Assign Visual Tree Asset: DebugPanel.uxml
   4. Add Component → DebugUIManager
   5. Initially disable the GameObject
   ```

3. **Create Notification Manager:**
   ```
   1. Create GameObject named "NotificationManager"
   2. Add Component → NotificationManager
   3. Leave enabled (creates notification area automatically)
   ```

4. **Register Panels with UIManager:**
   ```csharp
   // In your initialization code:
   UIManager.Instance.RegisterPanel("GameHUD", gameHudController);
   UIManager.Instance.RegisterPanel("DebugPanel", debugUIManager);
   ```

---

## Core UI Components

### UIManager

**Purpose:** Centralized management of all UI panels and state.

**Key Features:**
- Singleton pattern for global access
- Panel registry with show/hide methods
- Input handling (ESC key closes panels)
- Stack-based panel management

**Usage:**
```csharp
// Show a panel
UIManager.Instance.ShowPanel("GameHUD");

// Hide a panel
UIManager.Instance.HidePanel("DebugPanel");

// Toggle a panel
UIManager.Instance.TogglePanel("DebugPanel");

// Register custom panel
UIManager.Instance.RegisterPanel("MyPanel", myPanelController);
```

**Location:** `Assets/UI/Scripts/UIManager.cs`

### UIToolkitPanel (Base Class)

**Purpose:** Base class for all UI panel controllers.

**Key Features:**
- Standardized Show/Hide methods with virtual overrides
- Root VisualElement access
- Automatic UIDocument reference
- OnEnable/OnDisable lifecycle

**Creating a Custom Panel:**
```csharp
using UnityEngine;
using UnityEngine.UIElements;

public class MyCustomPanel : UIToolkitPanel
{
    private Label titleLabel;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Query UI elements
        titleLabel = root.Q<Label>("title");

        // Bind events
        var closeButton = root.Q<Button>("close-button");
        closeButton.clicked += Hide;
    }

    public override void Show()
    {
        base.Show();
        titleLabel.text = "Custom Panel Active";
    }

    public override void Hide()
    {
        base.Hide();
        // Cleanup logic
    }
}
```

**Location:** `Assets/UI/Scripts/UIToolkitPanel.cs`

### GameHUDController

**Purpose:** Main in-game HUD displaying time, inventory, and player stats.

**Displayed Information:**
- **Time & Date**: Current time of day and day number
- **Inventory**: Fish count and capacity
- **Player Stats**: Health, energy, etc.
- **Fishing Overlay**: Active when fishing (shows progress)

**UI Elements:**
```xml
<!-- GameHUD.uxml Structure -->
<UXML>
  <VisualElement class="game-hud">
    <VisualElement class="time-panel">
      <Label name="time-label" />
      <Label name="day-label" />
    </VisualElement>

    <VisualElement class="inventory-panel">
      <Label name="fish-count" />
    </VisualElement>

    <VisualElement name="fishing-overlay" class="hidden">
      <ProgressBar name="fishing-progress" />
    </VisualElement>
  </VisualElement>
</UXML>
```

**Location:** `Assets/UI/Scripts/GameHUDController.cs`

### DebugUIManager

**Purpose:** Comprehensive debug panel with 5 tabs for development and testing.

**Tabs:**

1. **System Tab**
   - FPS counter
   - Memory usage
   - Draw calls
   - Vertex count
   - Batch count

2. **Game State Tab**
   - Time controls (set time, adjust speed)
   - Player position and stats
   - Inventory management

3. **Fishing Tab**
   - Water zone list and details
   - Fish database viewer
   - Catch rate debugging

4. **Terrain Tab**
   - Live terrain generation
   - Parameter controls (noise scale, thresholds)
   - Regenerate terrain button

5. **Cheats Tab**
   - Add fish to inventory
   - Teleport player
   - Toggle systems (fishing, movement)

**Opening Debug Panel:**
- Press **F1** key (default binding)
- Or call: `UIManager.Instance.TogglePanel("DebugPanel")`

**Location:** `Assets/UI/Scripts/DebugUIManager.cs`

### NotificationManager

**Purpose:** Toast notification system with animations and type-based styling.

**Notification Types:**
```csharp
public enum NotificationType
{
    Info,     // Blue background
    Success,  // Green background
    Warning,  // Orange background
    Error     // Red background
}
```

**Usage:**
```csharp
// Show basic notification
NotificationManager.Instance.ShowNotification(
    "Fish Caught!",
    "You caught a Rainbow Trout",
    NotificationManager.NotificationType.Success
);

// With custom duration
NotificationManager.Instance.ShowNotification(
    "Warning",
    "Low energy!",
    NotificationManager.NotificationType.Warning,
    duration: 5f
);
```

**Features:**
- Automatic queuing (max 5 visible)
- Fade-in/fade-out animations
- Sound support (optional)
- Type-based color coding

**Location:** `Assets/UI/Scripts/NotificationManager.cs`

---

## UI Integration System

### UIIntegrationManager

**Purpose:** Bridges the gap between UI Toolkit and existing game systems through event subscriptions.

**Configuration:**
```csharp
[Header("UI Settings")]
public bool enableDebugMode = true;    // F1 debug panel
public bool replaceOldUI = true;       // Disable legacy UI

[Header("Input")]
public Key debugToggleKey = Key.F1;
```

**Event Subscriptions:**

The integration manager automatically connects to these events:

1. **InventorySystem Events:**
   ```csharp
   InventorySystem.OnFishAdded += (fish) => {
       NotificationManager.ShowNotification($"Caught {fish.fishName}!");
       UpdateInventoryDisplay();
   };
   ```

2. **GameTimeManager Events:**
   ```csharp
   GameTimeManager.OnTimeOfDayChanged += (timeOfDay) => {
       UpdateTimeDisplay(timeOfDay);
   };

   GameTimeManager.OnDayChanged += (day) => {
       NotificationManager.ShowNotification($"Day {day}");
   };
   ```

3. **FishingController Events:**
   ```csharp
   FishingController.OnFishingStarted += () => {
       ShowFishingOverlay();
   };

   FishingController.OnFishingEnded += () => {
       HideFishingOverlay();
   };
   ```

**Adding Custom Event Integration:**
```csharp
// In UIIntegrationManager.cs
private void SubscribeToEvents()
{
    // Existing subscriptions...

    // Add your custom event
    MyCustomSystem.OnCustomEvent += HandleCustomEvent;
}

private void HandleCustomEvent(CustomEventData data)
{
    // Update UI accordingly
    var label = gameHud.Root.Q<Label>("custom-label");
    label.text = data.ToString();
}
```

**Location:** `Assets/UI/Scripts/UIIntegrationManager.cs`

---

## Data Binding and Reactive Updates

### DataBindingHelper

**Purpose:** Utility class for creating reactive UI updates that automatically refresh when data changes.

**Key Methods:**

1. **BindText**: Bind a label to a dynamic value
   ```csharp
   DataBindingHelper.BindText(
       root,
       "player-health",
       () => player.currentHealth.ToString()
   );
   ```

2. **BindSlider**: Bind a slider to getter/setter
   ```csharp
   DataBindingHelper.BindSlider(
       root,
       "volume-slider",
       () => AudioListener.volume,
       (value) => AudioListener.volume = value
   );
   ```

3. **BindButton**: Bind button click to action
   ```csharp
   DataBindingHelper.BindButton(
       root,
       "start-button",
       () => StartGame()
   );
   ```

### Reactive Update Pattern

**Example: Health Bar**
```csharp
public class HealthDisplay : MonoBehaviour
{
    private VisualElement root;
    private ProgressBar healthBar;
    private PlayerController player;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        healthBar = root.Q<ProgressBar>("health-bar");
        player = FindObjectOfType<PlayerController>();

        // Subscribe to health changes
        player.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBar.value = (currentHealth / maxHealth) * 100f;
        healthBar.title = $"{currentHealth}/{maxHealth}";
    }

    private void OnDisable()
    {
        player.OnHealthChanged -= UpdateHealthBar;
    }
}
```

**Example: Inventory Display**
```csharp
public class InventoryDisplay : MonoBehaviour
{
    private Label fishCountLabel;
    private InventorySystem inventory;

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        fishCountLabel = root.Q<Label>("fish-count");
        inventory = InventorySystem.Instance;

        // Initial update
        UpdateFishCount();

        // Subscribe to changes
        inventory.OnFishAdded += (_) => UpdateFishCount();
        inventory.OnFishRemoved += (_) => UpdateFishCount();
    }

    private void UpdateFishCount()
    {
        fishCountLabel.text = $"Fish: {inventory.GetFishCount()}";
    }
}
```

**Location:** `Assets/UI/Scripts/DataBindingHelper.cs`

---

## Notification System

### Notification Types and Styling

Each notification type has distinct visual styling:

**Info (Default):**
- Color: Blue (`rgba(80, 150, 220, 0.9)`)
- Use for: General information, tips

**Success:**
- Color: Green (`rgba(100, 200, 100, 0.9)`)
- Use for: Achievements, successful actions, fish caught

**Warning:**
- Color: Orange (`rgba(200, 150, 50, 0.9)`)
- Use for: Low resources, approaching limits

**Error:**
- Color: Red (`rgba(200, 80, 80, 0.9)`)
- Use for: Failed actions, critical issues

### Advanced Usage

**Custom Duration:**
```csharp
NotificationManager.Instance.ShowNotification(
    "Long Message",
    "This will stay visible for 10 seconds",
    NotificationManager.NotificationType.Info,
    duration: 10f
);
```

**Integration with Game Events:**
```csharp
// In your game system
public class AchievementSystem : MonoBehaviour
{
    public void UnlockAchievement(string achievementName)
    {
        // Game logic...

        // Show notification
        NotificationManager.Instance.ShowNotification(
            "Achievement Unlocked!",
            achievementName,
            NotificationManager.NotificationType.Success
        );
    }
}
```

**Queue Management:**
The system automatically manages notification queuing:
- Maximum 5 notifications visible at once
- New notifications push old ones up
- Oldest notifications fade out when limit reached

---

## Debug Panel

### System Tab

**Displayed Metrics:**
- **FPS**: Frames per second with real-time graph
- **Memory**: Heap allocation and GC statistics
- **Rendering**: Draw calls, vertices, batches
- **System**: OS, Unity version, platform

**Usage:**
Automatically updates every frame. Use to identify performance bottlenecks.

### Game State Tab

**Time Controls:**
```csharp
// Set specific time
timeSlider.value = 14.5f; // 2:30 PM

// Adjust time scale
timeScaleSlider.value = 2.0f; // 2x speed
```

**Player Controls:**
- View player position
- Teleport to coordinates
- View/modify player stats

### Fishing Tab

**Water Zone Viewer:**
- Lists all active water zones
- Shows zone properties (type, fish count)
- Click to view zone details

**Fish Database:**
- View all fish in database
- See spawn conditions (time, weather, water type)
- Check rarity and difficulty

### Terrain Tab

**Live Terrain Generation:**
```csharp
// Adjust parameters
noiseScaleSlider.value = 0.15f;
waterThresholdSlider.value = 0.4f;

// Regenerate with new settings
GenerateTerrainButton.clicked += () => {
    terrainGenerator.GenerateTerrain();
};
```

### Cheats Tab

**Developer Tools:**
- **Add Fish**: Instantly add any fish to inventory
- **Set Time**: Jump to specific time of day
- **Teleport**: Move player to coordinates
- **Toggle Systems**: Enable/disable fishing, movement, etc.

**Usage:**
```csharp
// Example: Add fish cheat
var addFishButton = root.Q<Button>("add-fish-button");
addFishButton.clicked += () => {
    var selectedFish = GetSelectedFishFromDropdown();
    InventorySystem.Instance.AddFish(selectedFish);
};
```

---

## Customization and Extension

### Creating a New UI Panel

**Step 1: Create UXML Layout**
```xml
<!-- Assets/UI/UXML/MyPanel.uxml -->
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement class="my-panel">
        <ui:Label name="title" text="My Custom Panel" />
        <ui:Button name="action-button" text="Do Action" />
        <ui:Button name="close-button" text="Close" />
    </ui:VisualElement>
</ui:UXML>
```

**Step 2: Create USS Stylesheet**
```css
/* Assets/UI/USS/MyPanel.uss */
.my-panel {
    background-color: rgba(32, 32, 32, 0.9);
    padding: 20px;
    border-radius: 10px;
}

#title {
    font-size: 24px;
    color: white;
    margin-bottom: 10px;
}

.button {
    margin-top: 5px;
    padding: 10px;
}
```

**Step 3: Create Controller Script**
```csharp
// Assets/UI/Scripts/MyPanelController.cs
using UnityEngine;
using UnityEngine.UIElements;

public class MyPanelController : UIToolkitPanel
{
    private Label titleLabel;
    private Button actionButton;
    private Button closeButton;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Query elements
        titleLabel = root.Q<Label>("title");
        actionButton = root.Q<Button>("action-button");
        closeButton = root.Q<Button>("close-button");

        // Bind events
        actionButton.clicked += OnActionClicked;
        closeButton.clicked += Hide;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        // Cleanup
        actionButton.clicked -= OnActionClicked;
        closeButton.clicked -= Hide;
    }

    private void OnActionClicked()
    {
        Debug.Log("Action button clicked!");
        titleLabel.text = "Action Performed!";
    }

    public override void Show()
    {
        base.Show();
        titleLabel.text = "Panel Opened";
    }
}
```

**Step 4: Add to Scene**
```
1. Create GameObject named "MyPanel"
2. Add UIDocument component
3. Assign MyPanel.uxml to Visual Tree Asset
4. Add MyPanelController component
5. Register with UIManager:
   UIManager.Instance.RegisterPanel("MyPanel", myPanelController);
```

### Extending the GameHUD

**Adding a New Element:**

1. **Edit GameHUD.uxml:**
   ```xml
   <ui:VisualElement class="custom-section">
       <ui:Label name="custom-label" text="Custom Value: 0" />
   </ui:VisualElement>
   ```

2. **Update GameHUDController.cs:**
   ```csharp
   private Label customLabel;

   protected override void OnEnable()
   {
       base.OnEnable();
       customLabel = root.Q<Label>("custom-label");

       // Bind to data
       InvokeRepeating(nameof(UpdateCustomValue), 0f, 1f);
   }

   private void UpdateCustomValue()
   {
       customLabel.text = $"Custom Value: {GetCustomValue()}";
   }
   ```

3. **Style in MainHUD.uss:**
   ```css
   .custom-section {
       margin-top: 10px;
       padding: 5px;
   }

   #custom-label {
       color: yellow;
       font-size: 16px;
   }
   ```

---

## Styling with USS

### USS Basics

USS (Unity Style Sheets) uses CSS-like syntax:

```css
/* Class selector */
.my-class {
    background-color: blue;
}

/* ID selector */
#my-element {
    color: white;
}

/* Pseudo-class */
.button:hover {
    background-color: gray;
}

/* Child selector */
.parent > .child {
    margin: 5px;
}
```

### Color Scheme

**Primary Colors:**
```css
/* Project color palette */
:root {
    --primary-color: rgba(80, 150, 220, 0.9);
    --success-color: rgba(100, 200, 100, 0.9);
    --warning-color: rgba(200, 150, 50, 0.9);
    --error-color: rgba(200, 80, 80, 0.9);
    --background-color: rgba(32, 32, 32, 0.85);
    --text-color: rgba(255, 255, 255, 0.9);
}
```

### Responsive Design

**Media Queries:**
```css
/* Desktop (default) */
.game-hud {
    width: 100%;
    height: 100%;
}

/* Tablet */
@media (max-width: 1024px) {
    .game-hud {
        padding: 10px;
    }
}

/* Mobile */
@media (max-width: 768px) {
    .game-hud {
        padding: 5px;
        font-size: 12px;
    }
}
```

### Animations

**CSS Transitions:**
```css
.notification-toast {
    opacity: 0;
    transition-property: opacity, translate;
    transition-duration: 0.3s;
    transition-timing-function: ease-in-out;
}

.notification-toast.visible {
    opacity: 1;
    translate: 0 0;
}
```

**Using Animations in C#:**
```csharp
// Add class to trigger animation
element.AddToClassList("visible");

// Remove class after delay
this.Schedule(() => {
    element.RemoveFromClassList("visible");
}, 3f);
```

### Common USS Patterns

**Panel Container:**
```css
.panel {
    background-color: rgba(32, 32, 32, 0.9);
    border-radius: 10px;
    padding: 20px;
    border-width: 2px;
    border-color: rgba(100, 100, 100, 0.5);
}
```

**Button Styling:**
```css
.button {
    background-color: rgba(80, 150, 220, 0.8);
    color: white;
    padding: 10px 20px;
    border-radius: 5px;
    border-width: 0;
    font-size: 14px;
    cursor: pointer;
}

.button:hover {
    background-color: rgba(100, 170, 240, 0.9);
}

.button:active {
    background-color: rgba(60, 130, 200, 0.9);
}
```

---

## Testing and Debugging

### UI Toolkit Debugger

**Opening the Debugger:**
1. Window → UI Toolkit → Debugger
2. Select your UIDocument in scene
3. Inspect element hierarchy and styles

**Features:**
- Live element inspection
- Style override testing
- Event monitoring
- Layout debugging

### Common Debugging Tasks

**Check Element Exists:**
```csharp
var element = root.Q<Label>("my-label");
if (element == null)
{
    Debug.LogError("Element 'my-label' not found!");
}
```

**Verify Styling:**
```csharp
// Log computed style
Debug.Log($"Background: {element.resolvedStyle.backgroundColor}");
Debug.Log($"Display: {element.resolvedStyle.display}");
```

**Monitor Events:**
```csharp
button.clicked += () => {
    Debug.Log($"Button clicked at {Time.time}");
};
```

### Performance Profiling

**UI Toolkit Profiling:**
1. Window → Analysis → Profiler
2. Enable "UI" and "UI Details" modules
3. Look for:
   - Layout recalculations (should be minimal)
   - Style recalculations (batch when possible)
   - Event callback overhead

**Best Practices:**
- Cache frequently accessed elements
- Batch UI updates using `Schedule()`
- Minimize layout changes
- Use `display: none` instead of destroying elements

---

## Performance Optimization

### Best Practices

**1. Cache Element Queries:**
```csharp
// Bad - queries every frame
void Update()
{
    var label = root.Q<Label>("fps");
    label.text = fps.ToString();
}

// Good - cache the reference
private Label fpsLabel;

void OnEnable()
{
    fpsLabel = root.Q<Label>("fps");
}

void Update()
{
    fpsLabel.text = fps.ToString();
}
```

**2. Batch UI Updates:**
```csharp
// Bad - multiple individual updates
void UpdateStats()
{
    healthLabel.text = health.ToString();
    energyLabel.text = energy.ToString();
    moneyLabel.text = money.ToString();
}

// Good - batch using Schedule
void UpdateStats()
{
    this.Schedule(() => {
        healthLabel.text = health.ToString();
        energyLabel.text = energy.ToString();
        moneyLabel.text = money.ToString();
    });
}
```

**3. Use Display Toggle Instead of Destroy:**
```csharp
// Bad - creates/destroys elements
void ShowPanel()
{
    CreatePanelElements();
}

void HidePanel()
{
    DestroyPanelElements();
}

// Good - toggle display
void ShowPanel()
{
    panel.style.display = DisplayStyle.Flex;
}

void HidePanel()
{
    panel.style.display = DisplayStyle.None;
}
```

**4. Minimize Layout Recalculations:**
```csharp
// Bad - triggers layout every update
void Update()
{
    panel.style.width = CalculateWidth();
}

// Good - only update when needed
void OnScreenResize()
{
    panel.style.width = CalculateWidth();
}
```

### Notification System Optimization

**Limit Active Notifications:**
```csharp
// In NotificationManager
private const int MAX_VISIBLE_NOTIFICATIONS = 5;

public void ShowNotification(...)
{
    if (activeNotifications.Count >= MAX_VISIBLE_NOTIFICATIONS)
    {
        // Remove oldest
        RemoveOldestNotification();
    }
    // Add new notification
}
```

### Memory Management

**Unsubscribe from Events:**
```csharp
private void OnEnable()
{
    InventorySystem.OnFishAdded += HandleFishAdded;
}

private void OnDisable()
{
    // IMPORTANT: Prevent memory leaks
    InventorySystem.OnFishAdded -= HandleFishAdded;
}
```

---

## Troubleshooting

### UI Not Appearing

**Symptoms:** UI elements are invisible or not rendering.

**Solutions:**
1. **Check UIDocument Assignment:**
   - Verify Visual Tree Asset is assigned in Inspector
   - Ensure UXML file exists at specified path

2. **Verify Panel Sorting Order:**
   - UIDocument has Sort Order property
   - Higher values render on top

3. **Check Display Style:**
   ```csharp
   // Ensure element is visible
   element.style.display = DisplayStyle.Flex;
   element.style.opacity = 1;
   ```

4. **Inspect Console for Errors:**
   - UXML parsing errors
   - Missing USS files
   - Element query failures

### Elements Not Found (Null Reference)

**Symptoms:** `Q<>()` queries return null.

**Solutions:**
1. **Verify Element Names:**
   ```xml
   <!-- UXML must have matching name -->
   <ui:Label name="my-label" />
   ```
   ```csharp
   // C# query must match exactly
   var label = root.Q<Label>("my-label");
   ```

2. **Check Query Timing:**
   ```csharp
   // Bad - root might not be initialized
   private void Awake()
   {
       var label = root.Q<Label>("my-label");
   }

   // Good - root is ready in OnEnable
   private void OnEnable()
   {
       var label = root.Q<Label>("my-label");
   }
   ```

3. **Use Correct Element Type:**
   ```csharp
   // Must match UXML element type
   var button = root.Q<Button>("my-button"); // Correct
   var button = root.Q<Label>("my-button");  // Wrong - returns null
   ```

### Styling Not Applied

**Symptoms:** USS styles don't appear on elements.

**Solutions:**
1. **Verify USS is Loaded:**
   - Check UIDocument has StyleSheet assigned
   - Ensure USS file is in correct location

2. **Check Class/ID Selectors:**
   ```css
   /* USS */
   .my-class { color: red; }
   #my-id { color: blue; }
   ```
   ```xml
   <!-- UXML - must have matching class/name -->
   <ui:Label class="my-class" />
   <ui:Label name="my-id" />
   ```

3. **Use UI Debugger:**
   - Window → UI Toolkit → Debugger
   - Select element and inspect computed styles
   - Check if styles are overridden

4. **Verify Selector Specificity:**
   ```css
   /* More specific selector wins */
   .panel .label { color: red; }     /* Specificity: 2 */
   #my-label { color: blue; }        /* Specificity: 1 (ID) */
   ```

### Debug Panel Not Opening

**Symptoms:** F1 key doesn't toggle debug panel.

**Solutions:**
1. **Check Input System:**
   - Verify InputSystem_Actions.inputactions includes F1 binding
   - Ensure Input System package is installed

2. **Verify UIIntegrationManager:**
   ```csharp
   // Check these settings in Inspector
   enableDebugMode = true;
   debugToggleKey = Key.F1;
   ```

3. **Check Panel Registration:**
   ```csharp
   // Debug panel must be registered
   UIManager.Instance.RegisterPanel("DebugPanel", debugUIManager);
   ```

### Performance Issues

**Symptoms:** Low FPS, stuttering, high CPU usage.

**Solutions:**
1. **Limit Update Frequency:**
   ```csharp
   // Bad - updates every frame
   void Update()
   {
       UpdateAllStats();
   }

   // Good - updates periodically
   void Start()
   {
       InvokeRepeating(nameof(UpdateAllStats), 0f, 0.5f);
   }
   ```

2. **Reduce Active Notifications:**
   ```csharp
   // Limit concurrent notifications
   private const int MAX_NOTIFICATIONS = 3; // Down from 5
   ```

3. **Profile with Profiler:**
   - Window → Analysis → Profiler
   - Enable UI module
   - Look for expensive operations

4. **Optimize USS:**
   ```css
   /* Bad - complex selectors */
   .panel > .container > .item > .label { }

   /* Good - direct class */
   .item-label { }
   ```

### Events Not Firing

**Symptoms:** UI doesn't update when game state changes.

**Solutions:**
1. **Check Event Subscriptions:**
   ```csharp
   // Verify subscription is active
   InventorySystem.OnFishAdded += HandleFishAdded;

   // Check event is being invoked
   OnFishAdded?.Invoke(fishData);
   ```

2. **Verify Manager Initialization:**
   ```csharp
   // Ensure managers are initialized in correct order
   void Awake()
   {
       // UIIntegrationManager should initialize after game systems
   }
   ```

3. **Debug Event Flow:**
   ```csharp
   private void HandleFishAdded(FishData fish)
   {
       Debug.Log($"Event received: {fish.fishName}");
       // Update UI
   }
   ```

---

## API Reference

### UIManager

**Methods:**
```csharp
// Panel Management
void RegisterPanel(string panelName, UIToolkitPanel panel)
void ShowPanel(string panelName)
void HidePanel(string panelName)
void TogglePanel(string panelName)
bool IsPanelVisible(string panelName)

// Input Handling
void HandleEscapeKey() // Closes top panel
```

**Properties:**
```csharp
static UIManager Instance { get; }
Dictionary<string, UIToolkitPanel> panels { get; }
```

### UIToolkitPanel

**Virtual Methods:**
```csharp
virtual void Show()        // Override to add show logic
virtual void Hide()        // Override to add hide logic
virtual void OnEnable()    // Setup and queries
virtual void OnDisable()   // Cleanup
```

**Properties:**
```csharp
protected VisualElement root { get; }
protected UIDocument uiDocument { get; }
```

### NotificationManager

**Methods:**
```csharp
void ShowNotification(
    string title,
    string message,
    NotificationType type = NotificationType.Info,
    float duration = 3f
)

void ClearAllNotifications()
```

**Enums:**
```csharp
enum NotificationType { Info, Success, Warning, Error }
```

### DataBindingHelper

**Static Methods:**
```csharp
void BindText(
    VisualElement root,
    string elementName,
    Func<string> getValue
)

void BindSlider(
    VisualElement root,
    string elementName,
    Func<float> getValue,
    Action<float> setValue
)

void BindButton(
    VisualElement root,
    string elementName,
    Action onClick
)
```

### GameHUDController

**Public Methods:**
```csharp
void UpdateTimeDisplay(float timeOfDay)
void UpdateDayDisplay(int day)
void UpdateInventoryCount(int count)
void ShowFishingOverlay()
void HideFishingOverlay()
void UpdateFishingProgress(float progress)
```

### DebugUIManager

**Public Methods:**
```csharp
void SwitchTab(string tabName)
void RefreshAllTabs()
void UpdateSystemMetrics()
void UpdateGameState()
void UpdateFishingInfo()
void UpdateTerrainInfo()
```

**Tab Names:**
- `"system-tab"`
- `"gamestate-tab"`
- `"fishing-tab"`
- `"terrain-tab"`
- `"cheats-tab"`

---

## Further Reading

### Related Documentation
- **Setup Guide**: `Assets/UI/SETUP_GUIDE.md` - Quick setup instructions
- **Fishing System**: `Docs/FISHING_ZONES_GUIDE.md` - Fishing integration details
- **Inventory System**: `Docs/INVENTORY_SYSTEM_GUIDE.md` - Inventory events
- **Time System**: `Docs/GAME_TIME_SYSTEM_GUIDE.md` - Time events
- **Terrain System**: `Docs/TERRAIN_GENERATION_GUIDE.md` - Terrain integration

### Unity Documentation
- [UI Toolkit Overview](https://docs.unity3d.com/Manual/UIElements.html)
- [UXML Reference](https://docs.unity3d.com/Manual/UIE-UXML.html)
- [USS Reference](https://docs.unity3d.com/Manual/UIE-USS.html)
- [UI Builder](https://docs.unity3d.com/Manual/UIBuilder.html)

### Best Practices
- Always cache element queries in `OnEnable()`
- Unsubscribe from events in `OnDisable()`
- Use data binding for reactive updates
- Leverage USS for animations instead of C# when possible
- Test UI at different resolutions
- Profile UI performance regularly

---

**Document Version:** 1.0
**Last Updated:** 2025-01-05
**Unity Version:** 6000.2.5f1
**UI Toolkit Version:** Built-in
