# Game Time System Guide

## Table of Contents
1. [Overview](#overview)
2. [System Architecture](#system-architecture)
3. [Prerequisites](#prerequisites)
4. [Setting Up the Time System](#setting-up-the-time-system)
5. [Time Configuration](#time-configuration)
6. [Time of Day System](#time-of-day-system)
7. [Integration with Game Systems](#integration-with-game-systems)
8. [Advanced Features](#advanced-features)
9. [Testing and Debugging](#testing-and-debugging)
10. [Troubleshooting](#troubleshooting)
11. [API Reference](#api-reference)

---

## Overview

The Game Time System in Scratcher manages the in-game day/night cycle, time progression, and provides time-based events for other game systems to react to (fishing availability, lighting, NPC schedules, etc.).

### Key Features
- **24-Hour Cycle**: Realistic time progression with configurable speed
- **Time of Day Periods**: Dawn, Morning, Afternoon, Evening, Night
- **Day Tracking**: Incremental day counter with new day events
- **Auto-Advancement**: Automatic time progression or manual control
- **Event System**: C# Actions for time-based game reactions
- **Time Display**: Formatted 12-hour or 24-hour time strings
- **Time Manipulation**: Skip to specific times, adjust speed, pause

### Core Components
- **GameTimeManager**: Main time management component
- **TimeOfDay Enum**: Five distinct time periods
- **Event System**: OnTimeOfDayChanged, OnDayChanged events
- **Integration Points**: Fishing system, UI, lighting, NPC behavior

---

## System Architecture

### File Locations

**Core Script:**
```
Assets/Scripts/Data/GameTimeManager.cs
```

**Related Systems:**
```
Assets/Scripts/Data/FishData.cs (time-based fish availability)
Assets/Scripts/Fishing/WaterZone.cs (time-based spawning)
Assets/UI/Scripts/GameHUDController.cs (time display)
Assets/UI/Scripts/UIIntegrationManager.cs (event integration)
```

### Time Flow

```
GameTimeManager.Update()
    └─> AdvanceTime()
        ├─> currentTime += delta
        ├─> Check for day rollover
        │   └─> Fire OnDayChanged event
        └─> UpdateTimeOfDay()
            └─> Check if time period changed
                └─> Fire OnTimeOfDayChanged event

Other Systems Subscribe:
├─> WaterZone updates fish availability
├─> GameHUD updates time display
├─> Lighting system adjusts ambient light
└─> NPC system updates schedules
```

### TimeOfDay Enum

```csharp
public enum TimeOfDay
{
    Dawn,      // 5:00 AM - 8:00 AM
    Morning,   // 8:00 AM - 12:00 PM
    Afternoon, // 12:00 PM - 6:00 PM
    Evening,   // 6:00 PM - 10:00 PM
    Night      // 10:00 PM - 5:00 AM
}
```

### Event Architecture

```
GameTimeManager Events:
├─ OnTimeOfDayChanged(TimeOfDay)
│  └─ Subscribers:
│     ├─ WaterZone (update fish spawns)
│     ├─ GameHUD (display period)
│     ├─ Lighting System (adjust ambience)
│     └─ NPC System (behavior changes)
│
└─ OnDayChanged(int)
   └─ Subscribers:
      ├─ WaterZone (restock fish)
      ├─ Shop System (refresh inventory)
      ├─ Quest System (daily quests)
      └─ Save System (auto-save)
```

---

## Prerequisites

### 1. Unity Project Setup

Ensure basic project structure exists:
- Unity 6000.2.5f1 or later
- Time.timeScale support (standard Unity)

### 2. No Required Dependencies

GameTimeManager is standalone and has no hard dependencies:
- Works independently
- Optional integrations with other systems
- Pure C# with Unity Time API

### 3. Scene Setup

No special scene requirements:
- Can exist on any GameObject
- Typically attached to GameManager
- Persists across scenes if needed

---

## Setting Up the Time System

### Step 1: Create GameTimeManager

**Option A - Standalone GameObject:**

1. **Create GameObject:**
   ```
   Hierarchy > Right-click > Create Empty
   Name: "GameTimeManager"
   ```

2. **Add Component:**
   ```
   Add Component > Game Time Manager
   ```

**Option B - Attach to GameManager:**

1. **Select GameManager** in Hierarchy

2. **Add Component:**
   ```
   Add Component > Game Time Manager
   ```

3. **GameManager auto-finds it:**
   ```csharp
   // In GameManager.InitializeGame()
   if (timeManager == null)
       timeManager = FindObjectOfType<GameTimeManager>();
   ```

### Step 2: Configure Basic Settings

Select the GameTimeManager GameObject:

```
GameTimeManager Component
├─ Time Settings
│  ├─ Day Length In Minutes: 24.0 (real-time minutes)
│  └─ Auto Advance Time: ✓ true
│
└─ Current Time
   └─ Current Time: 8.0 (8:00 AM start)
```

**Configuration explanations:**
- **Day Length In Minutes**: How long a full 24-hour cycle takes in real-time
  - 24 minutes = 1 real minute = 1 game hour
  - 12 minutes = 1 real minute = 2 game hours (faster)
  - 48 minutes = 2 real minutes = 1 game hour (slower)

- **Auto Advance Time**: Whether time automatically progresses
  - ✓ true = Time advances automatically (typical)
  - false = Time only advances when manually called (scripted events)

- **Current Time**: Starting time (0-24 hour format)
  - 8.0 = 8:00 AM (recommended start)
  - 6.0 = 6:00 AM (dawn start)
  - 12.0 = 12:00 PM (noon)

### Step 3: Test Time Progression

1. **Enter Play Mode**

2. **Watch Inspector:**
   - Current Time value should increase
   - Updates every frame

3. **Verify Console:**
   Add debug logging (optional):
   ```csharp
   // Add to GameTimeManager.UpdateTimeOfDay()
   Debug.Log($"Time of day changed to: {currentTimeOfDay}");
   ```

4. **Check time display:**
   If UI connected, time should update on screen

### Step 4: (Optional) Integrate with GameManager

If using GameManager pattern:

```csharp
// In GameManager.cs
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameTimeManager timeManager;

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (timeManager == null)
            timeManager = FindObjectOfType<GameTimeManager>();

        if (timeManager != null)
        {
            Debug.Log($"Game started at {timeManager.GetTimeString()}");
        }
    }
}
```

---

## Time Configuration

### Day Length Settings

The speed of time progression is controlled by `dayLengthInMinutes`:

```csharp
[SerializeField] private float dayLengthInMinutes = 24f;
```

**Common Configurations:**

**Ultra-Fast (Testing):**
```
Day Length: 1 minute
Effect: Full day in 1 real minute
Use: Rapid testing, time-lapse
```

**Fast Pace:**
```
Day Length: 6 minutes
Effect: 1 real minute = 4 game hours
Use: Active gameplay, short sessions
```

**Standard Pace:**
```
Day Length: 12 minutes
Effect: 1 real minute = 2 game hours
Use: Balanced gameplay (recommended)
```

**Realistic Pace:**
```
Day Length: 24 minutes (default)
Effect: 1 real minute = 1 game hour
Use: Relaxed gameplay, immersive experience
```

**Slow Pace:**
```
Day Length: 48 minutes
Effect: 2 real minutes = 1 game hour
Use: Very relaxed, strategic gameplay
```

**Calculation Formula:**
```
Time Progression Rate = 24 hours / dayLengthInMinutes
Real Seconds per Game Hour = dayLengthInMinutes * 60 / 24

Examples:
  24 minutes: 24 * 60 / 24 = 60 seconds per game hour
  12 minutes: 12 * 60 / 24 = 30 seconds per game hour
  6 minutes:  6 * 60 / 24  = 15 seconds per game hour
```

### Time of Day Boundaries

Time periods are defined by hour thresholds:

```csharp
private TimeOfDay GetTimeOfDayFromHour(float hour)
{
    if (hour >= 5f && hour < 8f)        // 5:00 AM - 8:00 AM
        return TimeOfDay.Dawn;
    else if (hour >= 8f && hour < 12f)  // 8:00 AM - 12:00 PM
        return TimeOfDay.Morning;
    else if (hour >= 12f && hour < 18f) // 12:00 PM - 6:00 PM
        return TimeOfDay.Afternoon;
    else if (hour >= 18f && hour < 22f) // 6:00 PM - 10:00 PM
        return TimeOfDay.Evening;
    else                                 // 10:00 PM - 5:00 AM
        return TimeOfDay.Night;
}
```

**Period Durations:**
- Dawn: 3 hours (5-8 AM)
- Morning: 4 hours (8 AM-12 PM)
- Afternoon: 6 hours (12-6 PM)
- Evening: 4 hours (6-10 PM)
- Night: 7 hours (10 PM-5 AM)

**Customizing Boundaries:**

To modify time periods, edit the thresholds:

```csharp
// Example: Make Dawn longer (4-9 AM)
if (hour >= 4f && hour < 9f)
    return TimeOfDay.Dawn;
else if (hour >= 9f && hour < 12f)
    return TimeOfDay.Morning;
// ...
```

### Starting Configuration

Set the initial time and day:

```csharp
[Header("Current Time")]
[SerializeField, Range(0f, 24f)] private float currentTime = 8f; // 8 AM
private int currentDay = 1;
```

**Common Start Times:**
- 6:00 AM (6f): Early morning, dawn experience
- 8:00 AM (8f): Standard start (default)
- 12:00 PM (12f): Midday start
- 18:00 PM (18f): Evening start (unique experience)

**Starting Day:**
Always initializes to Day 1. Can be changed for save/load:

```csharp
public void SetDay(int day)
{
    currentDay = day;
}
```

---

## Time of Day System

### Understanding Time Periods

Each time period has distinct characteristics:

**Dawn (5-8 AM):**
- Transition period from night
- Special fish species active
- Lighting changes from dark to light
- "Golden hour" for certain activities

**Morning (8 AM-12 PM):**
- Active gameplay period
- Most NPCs awake and active
- Good fishing conditions
- Bright lighting

**Afternoon (12 PM-6 PM):**
- Peak activity time
- Warmest period visually
- Different fish species available
- Longest time period

**Evening (6-10 PM):**
- Transition to night
- Evening-specific fish
- Lighting dims
- NPCs return home

**Night (10 PM-5 AM):**
- Dark period
- Nocturnal fish active
- Reduced visibility
- Shops closed, fewer NPCs

### Using Time Periods in Gameplay

**Fish Availability:**
```csharp
// In FishData.cs
public TimeOfDay[] availableTimes; // Fish only catchable during these times

// In WaterZone.cs
private TimeOfDay GetCurrentTimeOfDay()
{
    if (timeManager != null)
        return timeManager.GetCurrentTimeOfDay();
    return TimeOfDay.Morning;
}

// Fish filtering
if (fishData.availableTimes.Length > 0)
{
    if (!fishData.availableTimes.Contains(currentTimeOfDay))
        continue; // Skip this fish
}
```

**Lighting Integration:**
```csharp
public class DayNightLighting : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Gradient timeOfDayColors;

    private GameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnTimeOfDayChanged += UpdateLighting;
    }

    private void UpdateLighting(TimeOfDay timeOfDay)
    {
        float t = (float)timeOfDay / 5f; // 0-1 across time periods
        globalLight.color = timeOfDayColors.Evaluate(t);

        switch (timeOfDay)
        {
            case TimeOfDay.Dawn:
                globalLight.intensity = 0.6f;
                break;
            case TimeOfDay.Morning:
            case TimeOfDay.Afternoon:
                globalLight.intensity = 1.0f;
                break;
            case TimeOfDay.Evening:
                globalLight.intensity = 0.7f;
                break;
            case TimeOfDay.Night:
                globalLight.intensity = 0.3f;
                break;
        }
    }
}
```

### Time-Based Behaviors

**NPC Schedule System:**
```csharp
public class NPCSchedule : MonoBehaviour
{
    [SerializeField] private Transform homePosition;
    [SerializeField] private Transform workPosition;

    private GameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnTimeOfDayChanged += UpdateSchedule;
        UpdateSchedule(timeManager.GetCurrentTimeOfDay());
    }

    private void UpdateSchedule(TimeOfDay timeOfDay)
    {
        switch (timeOfDay)
        {
            case TimeOfDay.Dawn:
            case TimeOfDay.Night:
                // Go home
                transform.position = homePosition.position;
                break;

            case TimeOfDay.Morning:
            case TimeOfDay.Afternoon:
                // Go to work
                transform.position = workPosition.position;
                break;

            case TimeOfDay.Evening:
                // Return home
                StartCoroutine(MoveToPosition(homePosition.position));
                break;
        }
    }
}
```

**Shop Hours:**
```csharp
public class ShopManager : MonoBehaviour
{
    [SerializeField] private TimeOfDay[] openHours = {
        TimeOfDay.Morning,
        TimeOfDay.Afternoon,
        TimeOfDay.Evening
    };

    private bool isOpen = false;

    private void Start()
    {
        GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnTimeOfDayChanged += CheckShopHours;
        CheckShopHours(timeManager.GetCurrentTimeOfDay());
    }

    private void CheckShopHours(TimeOfDay currentTime)
    {
        isOpen = System.Array.Exists(openHours, time => time == currentTime);

        if (isOpen)
            Debug.Log("Shop is now open!");
        else
            Debug.Log("Shop is closed.");
    }

    public bool CanTrade()
    {
        return isOpen;
    }
}
```

---

## Integration with Game Systems

### Fishing System Integration

The fishing system uses time for spawn availability:

**WaterZone checks current time:**
```csharp
// In WaterZone.TryGetFish()
private TimeOfDay GetCurrentTimeOfDay()
{
    if (timeManager != null)
        return timeManager.GetCurrentTimeOfDay();
    return TimeOfDay.Morning;
}

// Filter fish by available times
foreach (var fishSpawn in availableFish)
{
    if (fishSpawn.fishData.availableTimes.Length > 0)
    {
        TimeOfDay currentTime = GetCurrentTimeOfDay();
        if (!fishSpawn.fishData.availableTimes.Contains(currentTime))
            continue; // Skip - wrong time of day
    }

    eligibleFish.Add(fishSpawn);
}
```

**Daily fish restocking:**
```csharp
// In WaterZone.cs
private void Start()
{
    GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
    if (timeManager != null)
    {
        timeManager.OnDayChanged += OnNewDay;
    }
}

private void OnNewDay(int newDay)
{
    RestockFish(); // Replenish fish populations
    Debug.Log($"Water zone restocked for day {newDay}");
}
```

### UI Integration

**Time Display in GameHUD:**
```csharp
// In GameHUDController.cs
private Label timeLabel;
private Label dayLabel;

protected override void SetupUI()
{
    timeLabel = root.Q<Label>("time-display");
    dayLabel = root.Q<Label>("day-display");

    GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
    if (timeManager != null)
    {
        timeManager.OnTimeOfDayChanged += UpdateTimeDisplay;
        timeManager.OnDayChanged += UpdateDayDisplay;
    }
}

private void Update()
{
    // Update time display every frame for smooth updates
    GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
    if (timeManager != null)
    {
        timeLabel.text = timeManager.GetTimeString();
    }
}

private void UpdateTimeDisplay(TimeOfDay newTime)
{
    // Update time period icon or color
    switch (newTime)
    {
        case TimeOfDay.Dawn:
            timeLabel.AddToClassList("time-dawn");
            break;
        case TimeOfDay.Morning:
            timeLabel.AddToClassList("time-morning");
            break;
        // etc.
    }
}

private void UpdateDayDisplay(int newDay)
{
    dayLabel.text = $"Day {newDay}";
}
```

**Time-based notifications:**
```csharp
// In UIIntegrationManager.cs
private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
{
    if (NotificationManager.Instance != null)
    {
        string message = GetTimeOfDayMessage(newTimeOfDay);
        NotificationManager.Instance.ShowNotification(
            "Time Changed",
            message,
            NotificationManager.NotificationType.Info,
            3f
        );
    }
}

private string GetTimeOfDayMessage(TimeOfDay time)
{
    return time switch
    {
        TimeOfDay.Dawn => "The sun is rising...",
        TimeOfDay.Morning => "Good morning!",
        TimeOfDay.Afternoon => "It's a beautiful afternoon.",
        TimeOfDay.Evening => "Evening approaches...",
        TimeOfDay.Night => "Night has fallen.",
        _ => ""
    };
}
```

### Save System Integration

**Saving time state:**
```csharp
[System.Serializable]
public class TimeSaveData
{
    public float currentTime;
    public int currentDay;
    public bool autoAdvanceTime;
}

public class SaveSystem : MonoBehaviour
{
    public TimeSaveData SaveTime(GameTimeManager timeManager)
    {
        return new TimeSaveData
        {
            currentTime = timeManager.GetCurrentTime(),
            currentDay = timeManager.GetCurrentDay(),
            autoAdvanceTime = true // or from setting
        };
    }

    public void LoadTime(GameTimeManager timeManager, TimeSaveData saveData)
    {
        timeManager.SetTime(saveData.currentTime);
        // Set day (need to add SetDay method to GameTimeManager)
    }
}
```

---

## Advanced Features

### Manual Time Control

Pause and resume time:

```csharp
public class TimeController : MonoBehaviour
{
    private GameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();
    }

    public void PauseTime()
    {
        // Set autoAdvanceTime to false via reflection or add public method
        Time.timeScale = 0f; // Pause entire game
    }

    public void ResumeTime()
    {
        Time.timeScale = 1f;
    }

    public void SetTimeSpeed(float multiplier)
    {
        Time.timeScale = multiplier;
        // 0.5 = half speed
        // 1.0 = normal
        // 2.0 = double speed
    }
}
```

### Skip to Specific Time

Jump to a target time:

```csharp
public void SkipToTime(float targetHour)
{
    timeManager.SkipToTime(targetHour);
}

// Examples:
SkipToTime(6f);   // Skip to 6:00 AM (dawn)
SkipToTime(12f);  // Skip to noon
SkipToTime(18f);  // Skip to 6:00 PM (evening)
SkipToTime(0f);   // Skip to midnight
```

**Skip to next time period:**
```csharp
public void SkipToNextPeriod()
{
    TimeOfDay current = timeManager.GetCurrentTimeOfDay();
    float targetTime = current switch
    {
        TimeOfDay.Night => 5f,      // Skip to dawn
        TimeOfDay.Dawn => 8f,       // Skip to morning
        TimeOfDay.Morning => 12f,   // Skip to afternoon
        TimeOfDay.Afternoon => 18f, // Skip to evening
        TimeOfDay.Evening => 22f,   // Skip to night
        _ => 8f
    };

    timeManager.SkipToTime(targetTime);
}
```

### Time-Based Spawning

Spawn events at specific times:

```csharp
public class EventSpawner : MonoBehaviour
{
    [SerializeField] private float spawnTime = 12f; // Noon
    private bool hasSpawnedToday = false;

    private void Start()
    {
        GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnDayChanged += OnNewDay;
    }

    private void Update()
    {
        GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
        float currentTime = timeManager.GetCurrentTime();

        if (!hasSpawnedToday && currentTime >= spawnTime)
        {
            SpawnEvent();
            hasSpawnedToday = true;
        }
    }

    private void OnNewDay(int day)
    {
        hasSpawnedToday = false; // Reset for new day
    }

    private void SpawnEvent()
    {
        Debug.Log("Daily event spawned at noon!");
        // Spawn special NPC, activate quest, etc.
    }
}
```

### Weather System Integration

Combine time with weather:

```csharp
public class WeatherSystem : MonoBehaviour
{
    private GameTimeManager timeManager;
    private WeatherCondition currentWeather;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnDayChanged += OnNewDay;
        timeManager.OnTimeOfDayChanged += OnTimeChanged;
    }

    private void OnNewDay(int day)
    {
        // Roll for new weather each day
        currentWeather = GetRandomWeather();
        Debug.Log($"Day {day} weather: {currentWeather}");
    }

    private void OnTimeChanged(TimeOfDay newTime)
    {
        // Weather might change during day
        if (newTime == TimeOfDay.Afternoon)
        {
            if (Random.value < 0.3f) // 30% chance
            {
                currentWeather = WeatherCondition.Rainy;
                Debug.Log("Afternoon rain storm!");
            }
        }
    }

    private WeatherCondition GetRandomWeather()
    {
        float roll = Random.value;
        if (roll < 0.5f) return WeatherCondition.Sunny;
        if (roll < 0.8f) return WeatherCondition.Cloudy;
        return WeatherCondition.Rainy;
    }
}
```

### Time-Based Achievements

Track time-related achievements:

```csharp
public class TimeAchievements : MonoBehaviour
{
    private int daysPlayed = 0;

    private void Start()
    {
        GameTimeManager timeManager = FindObjectOfType<GameTimeManager>();
        timeManager.OnDayChanged += OnDayChanged;
    }

    private void OnDayChanged(int newDay)
    {
        daysPlayed = newDay;

        CheckAchievements();
    }

    private void CheckAchievements()
    {
        if (daysPlayed == 7)
        {
            UnlockAchievement("First Week");
        }
        else if (daysPlayed == 30)
        {
            UnlockAchievement("One Month");
        }
        else if (daysPlayed == 100)
        {
            UnlockAchievement("Century Player");
        }
    }

    private void UnlockAchievement(string achievementName)
    {
        Debug.Log($"Achievement Unlocked: {achievementName}");
    }
}
```

---

## Testing and Debugging

### Debug Time Controls

Add debug methods for testing:

```csharp
#if UNITY_EDITOR
[ContextMenu("Debug: Speed Up Time x10")]
private void DebugSpeedUpTime()
{
    Time.timeScale = 10f;
    Debug.Log("Time speed increased to 10x");
}

[ContextMenu("Debug: Normal Time Speed")]
private void DebugNormalSpeed()
{
    Time.timeScale = 1f;
}

[ContextMenu("Debug: Skip to Dawn")]
private void DebugSkipToDawn()
{
    SkipToTime(5f);
}

[ContextMenu("Debug: Skip to Night")]
private void DebugSkipToNight()
{
    SkipToTime(22f);
}

[ContextMenu("Debug: Next Day")]
private void DebugNextDay()
{
    SkipToTime(currentTime + 24f);
}

[ContextMenu("Debug: Print Time Info")]
private void DebugPrintTimeInfo()
{
    Debug.Log($"Current Time: {GetTimeString()}");
    Debug.Log($"Current Day: {currentDay}");
    Debug.Log($"Time of Day: {GetCurrentTimeOfDay()}");
    Debug.Log($"Time Scale: {Time.timeScale}");
}
#endif
```

### Visual Time Debug Display

Create on-screen debug display:

```csharp
public class TimeDebugDisplay : MonoBehaviour
{
    private GameTimeManager timeManager;
    private GUIStyle style;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();

        style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        if (timeManager == null) return;

        string info = $"Time: {timeManager.GetTimeString()}\n" +
                     $"Day: {timeManager.GetCurrentDay()}\n" +
                     $"Period: {timeManager.GetCurrentTimeOfDay()}\n" +
                     $"Speed: {Time.timeScale}x";

        GUI.Label(new Rect(10, 10, 300, 100), info, style);
    }
}
```

### Unit Testing

Test time progression logic:

```csharp
using NUnit.Framework;
using UnityEngine;

public class GameTimeManagerTests
{
    private GameObject obj;
    private GameTimeManager timeManager;

    [SetUp]
    public void Setup()
    {
        obj = new GameObject();
        timeManager = obj.AddComponent<GameTimeManager>();
    }

    [Test]
    public void TimeAdvances_WhenAutoAdvanceEnabled()
    {
        float startTime = timeManager.GetCurrentTime();

        // Simulate one second
        timeManager.AdvanceTime(); // Need to make this public or use reflection

        float endTime = timeManager.GetCurrentTime();
        Assert.Greater(endTime, startTime);
    }

    [Test]
    public void DayIncreases_WhenTimeReaches24()
    {
        timeManager.SetTime(23.9f);
        int startDay = timeManager.GetCurrentDay();

        timeManager.SkipToTime(24.5f);

        int endDay = timeManager.GetCurrentDay();
        Assert.AreEqual(startDay + 1, endDay);
    }

    [Test]
    public void TimeOfDay_CorrectForMorning()
    {
        timeManager.SetTime(10f); // 10 AM
        Assert.AreEqual(TimeOfDay.Morning, timeManager.GetCurrentTimeOfDay());
    }

    [Test]
    public void TimeOfDay_CorrectForNight()
    {
        timeManager.SetTime(23f); // 11 PM
        Assert.AreEqual(TimeOfDay.Night, timeManager.GetCurrentTimeOfDay());
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(obj);
    }
}
```

---

## Troubleshooting

### Time Not Advancing

**Symptom:** Current Time stays at 8.0 and doesn't change

**Solutions:**

1. **Check Auto Advance Time:**
   ```
   GameTimeManager > Auto Advance Time: ✓ Must be checked
   ```

2. **Verify game isn't paused:**
   ```csharp
   Debug.Log($"Time.timeScale: {Time.timeScale}"); // Should be 1.0
   ```

3. **Check Update is running:**
   ```csharp
   // Add to GameTimeManager.Update()
   Debug.Log("Update running");
   ```

4. **Ensure in Play mode:**
   Time only advances during Play mode, not Edit mode

### Events Not Firing

**Symptom:** Subscribe to OnTimeOfDayChanged but handler never called

**Solutions:**

1. **Verify subscription:**
   ```csharp
   timeManager.OnTimeOfDayChanged += HandleTimeChanged; // Correct
   // NOT:
   // timeManager.OnTimeOfDayChanged = HandleTimeChanged; // Wrong!
   ```

2. **Subscribe early enough:**
   Subscribe in Start() or Awake(), not Update()

3. **Check time is advancing:**
   If time doesn't change, events won't fire

4. **Ensure crossing time boundaries:**
   Events only fire when time of day CHANGES
   ```csharp
   // Add debug logging
   if (newTimeOfDay != currentTimeOfDay)
   {
       Debug.Log($"Time period changed: {currentTimeOfDay} -> {newTimeOfDay}");
       currentTimeOfDay = newTimeOfDay;
       OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
   }
   ```

### Incorrect Time Display

**Symptom:** Time shows "25:00 PM" or other invalid format

**Solutions:**

1. **Use GetTimeString() method:**
   ```csharp
   string timeStr = timeManager.GetTimeString(); // Formatted correctly
   ```

2. **Check 12/24 hour conversion:**
   ```csharp
   // GetTimeString() handles conversion:
   int hours = Mathf.FloorToInt(currentTime);
   string period = hours >= 12 ? "PM" : "AM";
   int displayHours = hours % 12;
   if (displayHours == 0) displayHours = 12;
   ```

3. **Verify time bounds:**
   ```csharp
   // Time should wrap at 24
   if (currentTime >= 24f)
   {
       currentTime -= 24f;
   }
   ```

### Time Advancing Too Fast/Slow

**Symptom:** Full day completes in 10 seconds or 2 hours

**Solutions:**

1. **Check Day Length setting:**
   ```
   GameTimeManager > Day Length In Minutes: 24.0
   ```

2. **Verify Time.timeScale:**
   ```csharp
   Debug.Log($"Time scale: {Time.timeScale}"); // Should be 1.0
   ```

3. **Recalculate expected speed:**
   ```
   24 minute day = 1 real minute per game hour
   Current: X minutes = Check dayLengthInMinutes
   ```

4. **Check for multiple time managers:**
   ```csharp
   GameTimeManager[] managers = FindObjectsOfType<GameTimeManager>();
   Debug.Log($"Found {managers.Length} time managers"); // Should be 1
   ```

### Day Counter Not Incrementing

**Symptom:** Always shows "Day 1"

**Solutions:**

1. **Verify time reaching 24:**
   ```csharp
   // Add to AdvanceTime()
   if (currentTime >= 24f)
   {
       Debug.Log($"Day changing from {currentDay} to {currentDay + 1}");
       currentTime -= 24f;
       currentDay++;
       OnDayChanged?.Invoke(currentDay);
   }
   ```

2. **Check day length:**
   If day length is very long, might not see rollover in testing

3. **Manually test:**
   ```csharp
   // Use context menu
   [ContextMenu("Force Next Day")]
   private void ForceNextDay()
   {
       currentTime = 0f;
       currentDay++;
       OnDayChanged?.Invoke(currentDay);
   }
   ```

---

## API Reference

### GameTimeManager Properties

```csharp
[SerializeField] private float dayLengthInMinutes = 24f;
[SerializeField] private bool autoAdvanceTime = true;
[SerializeField, Range(0f, 24f)] private float currentTime = 8f;
private int currentDay = 1;
```

### GameTimeManager Methods

#### GetCurrentTimeOfDay()
```csharp
public TimeOfDay GetCurrentTimeOfDay()
```

Returns the current time period.

**Returns:** TimeOfDay enum value

**Example:**
```csharp
TimeOfDay period = timeManager.GetCurrentTimeOfDay();
if (period == TimeOfDay.Night)
{
    Debug.Log("It's nighttime!");
}
```

#### GetCurrentTime()
```csharp
public float GetCurrentTime()
```

Returns current time in 24-hour format (0-24).

**Returns:** float (0.0 = midnight, 12.0 = noon, 23.5 = 11:30 PM)

#### GetCurrentDay()
```csharp
public int GetCurrentDay()
```

Returns current day number (1-based).

#### GetTimeString()
```csharp
public string GetTimeString()
```

Returns formatted time string in 12-hour format.

**Returns:** String like "08:30 AM" or "11:45 PM"

**Example:**
```csharp
string timeStr = timeManager.GetTimeString();
timeLabel.text = timeStr; // "03:25 PM"
```

#### SetTime(float newTime)
```csharp
public void SetTime(float newTime)
```

Sets current time to specific value (0-24).

**Parameters:**
- `newTime`: Hour in 24-hour format (clamped to 0-24)

**Example:**
```csharp
timeManager.SetTime(12f); // Set to noon
timeManager.SetTime(0f);  // Set to midnight
```

#### SkipToTime(float targetTime)
```csharp
public void SkipToTime(float targetTime)
```

Skips to target time, handles day rollover if needed.

**Parameters:**
- `targetTime`: Target hour (can be > 24 for next day)

**Example:**
```csharp
timeManager.SkipToTime(18f); // Skip to 6 PM today
timeManager.SkipToTime(26f); // Skip to 2 AM tomorrow
```

### Events

#### OnTimeOfDayChanged
```csharp
public System.Action<TimeOfDay> OnTimeOfDayChanged;
```

Fires when time period changes (Dawn→Morning, etc).

**Subscribe:**
```csharp
timeManager.OnTimeOfDayChanged += (newTime) => {
    Debug.Log($"Time period: {newTime}");
};
```

#### OnDayChanged
```csharp
public System.Action<int> OnDayChanged;
```

Fires when day increments at midnight.

**Subscribe:**
```csharp
timeManager.OnDayChanged += (newDay) => {
    Debug.Log($"It's now day {newDay}!");
};
```

### TimeOfDay Enum

```csharp
public enum TimeOfDay
{
    Dawn,      // 5:00 AM - 8:00 AM
    Morning,   // 8:00 AM - 12:00 PM
    Afternoon, // 12:00 PM - 6:00 PM
    Evening,   // 6:00 PM - 10:00 PM
    Night      // 10:00 PM - 5:00 AM
}
```

---

## Quick Reference Checklist

### Initial Setup
- [ ] Create GameTimeManager GameObject
- [ ] Add GameTimeManager component
- [ ] Set Day Length In Minutes (24 recommended)
- [ ] Enable Auto Advance Time
- [ ] Set starting time (8.0 for 8 AM)
- [ ] Test time advances in Play mode

### Integration
- [ ] Subscribe to OnTimeOfDayChanged for period changes
- [ ] Subscribe to OnDayChanged for daily events
- [ ] Connect to UI for time display
- [ ] Integrate with fishing system for availability
- [ ] (Optional) Add lighting system integration
- [ ] Always unsubscribe in OnDestroy()

### Testing
- [ ] Enter Play mode and verify time advances
- [ ] Watch Inspector - Current Time should increase
- [ ] Verify time periods change correctly
- [ ] Test day rollover (or use debug skip)
- [ ] Check event subscribers receive notifications
- [ ] Test UI updates with time

---

**Last Updated:** 2025-10-03
**Unity Version:** 6000.2.5f1
**Project:** Scratcher - 2D Isometric Fishing Game
