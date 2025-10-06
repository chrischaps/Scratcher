using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class DebugUIManager : UIToolkitPanel
{
    [Header("Debug Settings")] [SerializeField]
    private bool startVisible;

    [SerializeField] private float updateInterval = 0.1f;
    private Button addRandomFishButton, addRareFishButton, fillInventoryButton;
    private Button closeButton;
    private VisualElement currentActiveTab;
    private Label drawCallsValue, trianglesValue, verticesValue;
    private Label fishCount, totalValue;
    private FishDatabase fishDatabase;
    private FishingController fishingController;
    private ScrollView fishList;

    // System stats elements
    private Label fpsValue, frametimeValue, memoryValue, gameObjectsValue;
    private Button generateTerrainButton, clearTerrainButton;
    private InventorySystem inventorySystem;

    private float lastUpdateTime;
    private Label mapSizeDisplay, noiseScaleDisplay, waterThresholdDisplay;
    private SliderInt mapSizeSlider;

    // Fishing debug elements
    private Label nearWater, waterType, availableFishCount, totalFishTypes;

    // Control buttons
    private Button nextDayButton, clearInventoryButton;

    // Terrain elements
    private Slider noiseSizeSlider, waterThresholdSlider;
    private IsometricPlayerController playerController;
    private Label playerPosition, playerMoving, playerFishing;

    // Tab content containers
    private VisualElement systemTab, gameStateTab, fishingTab, terrainTab, cheatsTab;
    private Dictionary<string, Button> tabButtons;
    private Dictionary<string, VisualElement> tabs;

    // Tab buttons
    private Button tabSystem, tabGameState, tabFishing, tabTerrain, tabCheats;
    private Button teleportButton, godModeButton, noclipButton, resetGameButton;

    // Input fields
    private FloatField teleportX, teleportY;
    private TerrainGenerator terrainGenerator;
    private Label timeDisplay, timescaleDisplay, dayValue;

    // References to game systems
    private GameTimeManager timeManager;

    // Game state elements
    private Slider timeSlider, timescaleSlider;

    private void Update()
    {
        if (!IsVisible || !isInitialized) return;

        // Update at specified interval
        if (Time.unscaledTime - lastUpdateTime >= updateInterval)
        {
            UpdateDebugInfo();
            lastUpdateTime = Time.unscaledTime;
        }
    }

    protected override void BindUIElements()
    {
        // Initialize collections
        tabs = new Dictionary<string, VisualElement>();
        tabButtons = new Dictionary<string, Button>();

        // Bind header elements
        closeButton = GetElement<Button>("close-button");

        // Bind tab buttons
        tabSystem = GetElement<Button>("tab-system");
        tabGameState = GetElement<Button>("tab-gamestate");
        tabFishing = GetElement<Button>("tab-fishing");
        tabTerrain = GetElement<Button>("tab-terrain");
        tabCheats = GetElement<Button>("tab-cheats");

        // Store tab references
        tabButtons["system"] = tabSystem;
        tabButtons["gamestate"] = tabGameState;
        tabButtons["fishing"] = tabFishing;
        tabButtons["terrain"] = tabTerrain;
        tabButtons["cheats"] = tabCheats;

        // Bind tab content
        systemTab = GetElement<VisualElement>("system-tab");
        gameStateTab = GetElement<VisualElement>("gamestate-tab");
        fishingTab = GetElement<VisualElement>("fishing-tab");
        terrainTab = GetElement<VisualElement>("terrain-tab");
        cheatsTab = GetElement<VisualElement>("cheats-tab");

        // Store tab content references
        tabs["system"] = systemTab;
        tabs["gamestate"] = gameStateTab;
        tabs["fishing"] = fishingTab;
        tabs["terrain"] = terrainTab;
        tabs["cheats"] = cheatsTab;

        // Bind system stats elements
        fpsValue = GetElement<Label>("fps-value");
        frametimeValue = GetElement<Label>("frametime-value");
        memoryValue = GetElement<Label>("memory-value");
        gameObjectsValue = GetElement<Label>("gameobjects-value");
        drawCallsValue = GetElement<Label>("drawcalls-value");
        trianglesValue = GetElement<Label>("triangles-value");
        verticesValue = GetElement<Label>("vertices-value");

        // Bind game state elements
        timeSlider = GetElement<Slider>("time-slider");
        timescaleSlider = GetElement<Slider>("timescale-slider");
        timeDisplay = GetElement<Label>("time-display");
        timescaleDisplay = GetElement<Label>("timescale-display");
        dayValue = GetElement<Label>("day-value");
        playerPosition = GetElement<Label>("player-position");
        playerMoving = GetElement<Label>("player-moving");
        playerFishing = GetElement<Label>("player-fishing");
        fishCount = GetElement<Label>("fish-count");
        totalValue = GetElement<Label>("total-value");

        // Bind control buttons
        nextDayButton = GetElement<Button>("next-day-button");
        clearInventoryButton = GetElement<Button>("clear-inventory-button");

        // Bind fishing elements
        nearWater = GetElement<Label>("near-water");
        waterType = GetElement<Label>("water-type");
        availableFishCount = GetElement<Label>("available-fish-count");
        totalFishTypes = GetElement<Label>("total-fish-types");
        fishList = GetElement<ScrollView>("fish-list");

        // Bind terrain elements
        mapSizeSlider = GetElement<SliderInt>("map-size-slider");
        noiseSizeSlider = GetElement<Slider>("noise-scale-slider");
        waterThresholdSlider = GetElement<Slider>("water-threshold-slider");
        mapSizeDisplay = GetElement<Label>("map-size-display");
        noiseScaleDisplay = GetElement<Label>("noise-scale-display");
        waterThresholdDisplay = GetElement<Label>("water-threshold-display");
        generateTerrainButton = GetElement<Button>("generate-terrain-button");
        clearTerrainButton = GetElement<Button>("clear-terrain-button");

        // Bind cheat elements
        addRandomFishButton = GetElement<Button>("add-random-fish-button");
        addRareFishButton = GetElement<Button>("add-rare-fish-button");
        fillInventoryButton = GetElement<Button>("fill-inventory-button");
        teleportX = GetElement<FloatField>("teleport-x");
        teleportY = GetElement<FloatField>("teleport-y");
        teleportButton = GetElement<Button>("teleport-button");
        godModeButton = GetElement<Button>("god-mode-button");
        noclipButton = GetElement<Button>("noclip-button");
        resetGameButton = GetElement<Button>("reset-game-button");
    }

    protected override void SetupEventHandlers()
    {
        // Header events
        if (closeButton != null)
            closeButton.clicked += () => HidePanel();

        // Tab events
        if (tabSystem != null) tabSystem.clicked += () => SwitchTab("system");
        if (tabGameState != null) tabGameState.clicked += () => SwitchTab("gamestate");
        if (tabFishing != null) tabFishing.clicked += () => SwitchTab("fishing");
        if (tabTerrain != null) tabTerrain.clicked += () => SwitchTab("terrain");
        if (tabCheats != null) tabCheats.clicked += () => SwitchTab("cheats");

        // Game state controls
        if (timeSlider != null)
            timeSlider.RegisterValueChangedCallback(OnTimeSliderChanged);
        if (timescaleSlider != null)
            timescaleSlider.RegisterValueChangedCallback(OnTimescaleChanged);

        // Control buttons
        if (nextDayButton != null)
            nextDayButton.clicked += OnNextDay;
        if (clearInventoryButton != null)
            clearInventoryButton.clicked += OnClearInventory;

        // Terrain controls
        if (mapSizeSlider != null)
            mapSizeSlider.RegisterValueChangedCallback(OnMapSizeChanged);
        if (noiseSizeSlider != null)
            noiseSizeSlider.RegisterValueChangedCallback(OnNoiseScaleChanged);
        if (waterThresholdSlider != null)
            waterThresholdSlider.RegisterValueChangedCallback(OnWaterThresholdChanged);
        if (generateTerrainButton != null)
            generateTerrainButton.clicked += OnGenerateTerrain;
        if (clearTerrainButton != null)
            clearTerrainButton.clicked += OnClearTerrain;

        // Cheat buttons
        if (addRandomFishButton != null)
            addRandomFishButton.clicked += OnAddRandomFish;
        if (addRareFishButton != null)
            addRareFishButton.clicked += OnAddRareFish;
        if (fillInventoryButton != null)
            fillInventoryButton.clicked += OnFillInventory;
        if (teleportButton != null)
            teleportButton.clicked += OnTeleport;
        if (godModeButton != null)
            godModeButton.clicked += OnToggleGodMode;
        if (noclipButton != null)
            noclipButton.clicked += OnToggleNoclip;
        if (resetGameButton != null)
            resetGameButton.clicked += OnResetGame;
    }

    protected override void InitializeData()
    {
        // Find game system references
        timeManager = FindObjectOfType<GameTimeManager>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        fishingController = FindObjectOfType<FishingController>();
        playerController = FindObjectOfType<IsometricPlayerController>();
        terrainGenerator = FindObjectOfType<TerrainGenerator>();

        // Find fish database
        if (GameManager.Instance != null)
            fishDatabase = GameManager.Instance.GetFishDatabase();

        // Set initial tab
        SwitchTab("system");
        currentActiveTab = systemTab;

        // Set initial visibility
        if (startVisible)
            ShowPanel();
        else
            HidePanel();

        // Populate fish list
        PopulateFishList();
    }

    private void UpdateDebugInfo()
    {
        UpdateSystemStats();
        UpdateGameState();
        UpdateFishingInfo();
    }

    private void UpdateSystemStats()
    {
        // Performance stats
        if (fpsValue != null)
            fpsValue.text = Mathf.RoundToInt(1f / Time.unscaledDeltaTime).ToString();

        if (frametimeValue != null)
            frametimeValue.text = $"{Time.unscaledDeltaTime * 1000f:F1}ms";

        if (memoryValue != null)
            memoryValue.text = $"{GC.GetTotalMemory(false) / 1024 / 1024} MB";

        if (gameObjectsValue != null)
            gameObjectsValue.text = FindObjectsOfType<GameObject>().Length.ToString();

        // Unity stats (these would need to be gathered from Unity's internal stats)
        if (drawCallsValue != null)
            drawCallsValue.text = "N/A"; // Would need UnityStats.drawCalls in development builds

        if (trianglesValue != null)
            trianglesValue.text = "N/A";

        if (verticesValue != null)
            verticesValue.text = "N/A";
    }

    private void UpdateGameState()
    {
        // Time info
        if (timeManager != null)
        {
            var currentTime = timeManager.GetCurrentTime();
            if (timeSlider != null && !timeSlider.focusController.focusedElement.Equals(timeSlider))
                timeSlider.SetValueWithoutNotify(currentTime);

            if (timeDisplay != null)
                timeDisplay.text = timeManager.GetTimeString();

            if (dayValue != null)
                dayValue.text = timeManager.GetCurrentDay().ToString();
        }

        // Timescale
        if (timescaleDisplay != null)
            timescaleDisplay.text = $"{Time.timeScale:F1}x";

        // Player info
        if (playerController != null)
        {
            if (playerPosition != null)
                playerPosition.text =
                    $"({playerController.transform.position.x:F1}, {playerController.transform.position.y:F1}, {playerController.transform.position.z:F1})";

            if (playerMoving != null)
                playerMoving.text = playerController.IsMoving.ToString();
        }

        // Fishing info
        if (fishingController != null && playerFishing != null)
            // This would need a public property in FishingController
            playerFishing.text = "Unknown"; // fishingController.IsFishing.ToString();

        // Inventory info
        if (inventorySystem != null)
        {
            if (fishCount != null)
                fishCount.text = inventorySystem.GetInventoryCount().ToString();

            if (totalValue != null)
                totalValue.text = inventorySystem.GetTotalValue().ToString();
        }
    }

    private void UpdateFishingInfo()
    {
        if (fishingController != null)
        {
            // These would need public properties in FishingController
            if (nearWater != null)
                nearWater.text = "Unknown"; // fishingController.IsNearWater.ToString();

            if (waterType != null)
                waterType.text = "Unknown"; // fishingController.CurrentWaterType?.ToString() ?? "None";
        }

        if (fishDatabase != null && availableFishCount != null)
            // This would need implementation based on current conditions
            availableFishCount.text = "Unknown";
    }

    private void PopulateFishList()
    {
        if (fishDatabase == null || fishList == null) return;

        var allFish = fishDatabase.GetAllFish();
        if (totalFishTypes != null)
            totalFishTypes.text = allFish.Count.ToString();

        fishList.Clear();

        foreach (var fish in allFish)
        {
            var fishItem = new VisualElement();
            fishItem.AddToClassList("fish-item");

            var nameLabel = new Label(fish.fishName);
            nameLabel.AddToClassList("fish-name");

            var rarityLabel = new Label($"{fish.rarity:P0}");
            rarityLabel.AddToClassList("fish-rarity");

            fishItem.Add(nameLabel);
            fishItem.Add(rarityLabel);
            fishList.Add(fishItem);
        }
    }

    private void SwitchTab(string tabName)
    {
        // Hide all tabs
        foreach (var tab in tabs.Values) tab.RemoveFromClassList("tab-active");

        // Remove active class from all tab buttons
        foreach (var button in tabButtons.Values) button.RemoveFromClassList("tab-active");

        // Show selected tab
        if (tabs.TryGetValue(tabName, out var selectedTab))
        {
            selectedTab.AddToClassList("tab-active");
            currentActiveTab = selectedTab;
        }

        // Activate selected tab button
        if (tabButtons.TryGetValue(tabName, out var selectedButton)) selectedButton.AddToClassList("tab-active");
    }

    // Event handlers
    private void OnTimeSliderChanged(ChangeEvent<float> evt)
    {
        if (timeManager != null)
            timeManager.SetTime(evt.newValue);
    }

    private void OnTimescaleChanged(ChangeEvent<float> evt)
    {
        Time.timeScale = evt.newValue;
    }

    private void OnNextDay()
    {
        if (timeManager != null)
            timeManager.SkipToTime(8f); // Skip to next day at 8 AM
    }

    private void OnClearInventory()
    {
        if (inventorySystem != null)
            inventorySystem.SellAllFish();
    }

    private void OnMapSizeChanged(ChangeEvent<int> evt)
    {
        if (mapSizeDisplay != null)
            mapSizeDisplay.text = $"{evt.newValue}x{evt.newValue}";
    }

    private void OnNoiseScaleChanged(ChangeEvent<float> evt)
    {
        if (noiseScaleDisplay != null)
            noiseScaleDisplay.text = evt.newValue.ToString("F2");
    }

    private void OnWaterThresholdChanged(ChangeEvent<float> evt)
    {
        if (waterThresholdDisplay != null)
            waterThresholdDisplay.text = evt.newValue.ToString("F2");
    }

    private void OnGenerateTerrain()
    {
        if (terrainGenerator != null)
            // This would need a public method in TerrainGenerator
            Debug.Log("Generate Terrain requested - would need public method in TerrainGenerator");
    }

    private void OnClearTerrain()
    {
        if (terrainGenerator != null)
            // This would need a public method in TerrainGenerator
            Debug.Log("Clear Terrain requested - would need public method in TerrainGenerator");
    }

    private void OnAddRandomFish()
    {
        if (fishDatabase != null && inventorySystem != null)
        {
            var randomFish = fishDatabase.GetRandomFish();
            if (randomFish != null)
            {
                var weight = Random.Range(randomFish.minWeight, randomFish.maxWeight);
                inventorySystem.AddFish(randomFish, weight);
            }
        }
    }

    private void OnAddRareFish()
    {
        if (fishDatabase != null && inventorySystem != null)
        {
            var rareFish = fishDatabase.GetFishByRarity(0.7f, 1.0f);
            if (rareFish.Count > 0)
            {
                var fish = rareFish[Random.Range(0, rareFish.Count)];
                var weight = Random.Range(fish.minWeight, fish.maxWeight);
                inventorySystem.AddFish(fish, weight);
            }
        }
    }

    private void OnFillInventory()
    {
        if (fishDatabase != null && inventorySystem != null)
        {
            var allFish = fishDatabase.GetAllFish();
            foreach (var fish in allFish)
            {
                if (inventorySystem.IsInventoryFull()) break;
                var weight = Random.Range(fish.minWeight, fish.maxWeight);
                inventorySystem.AddFish(fish, weight);
            }
        }
    }

    private void OnTeleport()
    {
        if (playerController != null && teleportX != null && teleportY != null)
        {
            var newPosition = new Vector3(teleportX.value, teleportY.value, playerController.transform.position.z);
            playerController.transform.position = newPosition;
        }
    }

    private void OnToggleGodMode()
    {
        Debug.Log("God Mode toggle - would need implementation");
    }

    private void OnToggleNoclip()
    {
        Debug.Log("Noclip toggle - would need implementation");
    }

    private void OnResetGame()
    {
        Debug.Log("Reset Game - would need implementation");
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        if (root != null) root.style.display = DisplayStyle.Flex;
    }

    public override void HidePanel()
    {
        base.HidePanel();
        if (root != null) root.style.display = DisplayStyle.None;
    }
}