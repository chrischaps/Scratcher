using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Integrates the new UI Toolkit system with existing game systems
/// Handles the transition and provides compatibility layer
/// </summary>
public class UIIntegrationManager : MonoBehaviour
{
    [Header("UI Documents")]
    [SerializeField] private UIDocument gameHUDDocument;
    [SerializeField] private UIDocument debugUIDocument;
    [SerializeField] private UIDocument mainMenuDocument;

    [Header("Integration Settings")]
    [SerializeField] private bool replaceOldUI = true;
    [SerializeField] private bool enableDebugMode = true;
    [SerializeField] private KeyCode debugToggleKey = KeyCode.F1;

    // UI Controllers
    private GameHUDController gameHUD;
    private DebugUIManager debugUI;
    private MainMenuController mainMenu;

    // Legacy UI components to replace
    private FishingGameUI legacyFishingUI;

    // Game system references
    private GameManager gameManager;
    private FishingController fishingController;
    private InventorySystem inventorySystem;
    private GameTimeManager timeManager;

    public static UIIntegrationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeUIIntegration();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeUIIntegration()
    {
        // Find or create UI documents
        SetupUIDocuments();

        // Get UI controllers
        SetupUIControllers();

        // Find game system references
        FindGameSystems();

        // Setup integration
        IntegrateWithGameSystems();

        // Replace legacy UI if requested
        if (replaceOldUI)
        {
            ReplaceLegacyUI();
        }

        Debug.Log("UI Integration Manager initialized successfully");
    }

    private void SetupUIDocuments()
    {
        // Game HUD Document
        if (gameHUDDocument == null)
        {
            gameHUDDocument = FindUIDocument("GameHUD");
            if (gameHUDDocument == null)
            {
                gameHUDDocument = CreateUIDocument("GameHUD", "GameHUD");
            }
        }

        // Debug UI Document
        if (debugUIDocument == null && enableDebugMode)
        {
            debugUIDocument = FindUIDocument("DebugPanel");
            if (debugUIDocument == null)
            {
                debugUIDocument = CreateUIDocument("DebugPanel", "DebugPanel");
            }
        }

        // Main Menu Document (if in main menu scene)
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
        {
            if (mainMenuDocument == null)
            {
                mainMenuDocument = FindUIDocument("MainMenu");
                if (mainMenuDocument == null)
                {
                    mainMenuDocument = CreateUIDocument("MainMenu", "MainMenu");
                }
            }
        }
    }

    private UIDocument FindUIDocument(string documentName)
    {
        var documents = FindObjectsOfType<UIDocument>();
        foreach (var doc in documents)
        {
            if (doc.name == documentName || doc.gameObject.name == documentName)
            {
                return doc;
            }
        }
        return null;
    }

    private UIDocument CreateUIDocument(string name, string assetName)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(transform);

        var document = gameObject.AddComponent<UIDocument>();

        // Try to load the visual tree asset
        var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/UXML/{assetName}");
        if (visualTreeAsset != null)
        {
            document.visualTreeAsset = visualTreeAsset;
        }

        // Try to load the style sheet
        var styleSheet = Resources.Load<StyleSheet>($"UI/USS/{assetName}");
        if (styleSheet != null && document.rootVisualElement != null)
        {
            document.rootVisualElement.styleSheets.Add(styleSheet);
        }

        return document;
    }

    private void SetupUIControllers()
    {
        // Game HUD Controller
        if (gameHUDDocument != null)
        {
            gameHUD = gameHUDDocument.GetComponent<GameHUDController>();
            if (gameHUD == null)
            {
                gameHUD = gameHUDDocument.gameObject.AddComponent<GameHUDController>();
            }
        }

        // Debug UI Controller
        if (debugUIDocument != null && enableDebugMode)
        {
            debugUI = debugUIDocument.GetComponent<DebugUIManager>();
            if (debugUI == null)
            {
                debugUI = debugUIDocument.gameObject.AddComponent<DebugUIManager>();
            }
        }

        // Main Menu Controller
        if (mainMenuDocument != null)
        {
            mainMenu = mainMenuDocument.GetComponent<MainMenuController>();
            if (mainMenu == null)
            {
                mainMenu = mainMenuDocument.gameObject.AddComponent<MainMenuController>();
            }
        }
    }

    private void FindGameSystems()
    {
        gameManager = FindObjectOfType<GameManager>();
        fishingController = FindObjectOfType<FishingController>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        timeManager = FindObjectOfType<GameTimeManager>();

        // Find legacy UI
        legacyFishingUI = FindObjectOfType<FishingGameUI>();
    }

    private void IntegrateWithGameSystems()
    {
        // Integrate with GameManager
        if (gameManager != null)
        {
            // Connect UI Manager to game manager
            var uiManager = gameManager.GetComponent<UIManager>();
            if (uiManager == null)
            {
                uiManager = gameManager.gameObject.AddComponent<UIManager>();
            }

            // Register UI panels with UIManager
            if (gameHUD != null) uiManager.RegisterPanel(gameHUD);
            if (debugUI != null) uiManager.RegisterPanel(debugUI);
            if (mainMenu != null) uiManager.RegisterPanel(mainMenu);
        }

        // Integrate with Fishing Controller
        if (fishingController != null && gameHUD != null)
        {
            // Add integration layer to fishing controller
            var fishingIntegration = fishingController.GetComponent<FishingUIIntegration>();
            if (fishingIntegration == null)
            {
                fishingIntegration = fishingController.gameObject.AddComponent<FishingUIIntegration>();
                fishingIntegration.Initialize(gameHUD, fishingController);
            }
        }

        // Integrate with Inventory System
        if (inventorySystem != null && gameHUD != null)
        {
            // Subscribe to inventory events for HUD updates
            inventorySystem.OnFishAdded += OnFishAdded;
            inventorySystem.OnValueChanged += OnInventoryValueChanged;
        }

        // Integrate with Time Manager
        if (timeManager != null && gameHUD != null)
        {
            // Subscribe to time events for HUD updates
            timeManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
            timeManager.OnDayChanged += OnDayChanged;
        }

        // Setup Notification Manager
        SetupNotificationManager();
    }

    private void SetupNotificationManager()
    {
        var notificationManager = FindObjectOfType<NotificationManager>();
        if (notificationManager == null)
        {
            var notificationGO = new GameObject("NotificationManager");
            notificationGO.transform.SetParent(transform);
            notificationManager = notificationGO.AddComponent<NotificationManager>();
        }
    }

    private void ReplaceLegacyUI()
    {
        if (legacyFishingUI != null)
        {
            // Disable legacy UI components
            legacyFishingUI.gameObject.SetActive(false);
            Debug.Log("Disabled legacy FishingGameUI");
        }

        // Find and disable other legacy UI components
        var legacyCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in legacyCanvases)
        {
            if (canvas.name.Contains("Legacy") || canvas.name.Contains("Old"))
            {
                canvas.gameObject.SetActive(false);
                Debug.Log($"Disabled legacy canvas: {canvas.name}");
            }
        }
    }

    private void Update()
    {
        HandleDebugInput();
    }

    private void HandleDebugInput()
    {
        if (enableDebugMode)
        {
            // Use new Input System
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                Key targetKey = ConvertKeyCodeToKey(debugToggleKey);
                if (keyboard[targetKey].wasPressedThisFrame)
                {
                    if (debugUI != null)
                    {
                        debugUI.TogglePanel();
                    }
                    else if (UIManager.Instance != null)
                    {
                        UIManager.Instance.TogglePanel("DebugPanel");
                    }
                }
            }
        }
    }

    private Key ConvertKeyCodeToKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.F1: return Key.F1;
            case KeyCode.F2: return Key.F2;
            case KeyCode.F3: return Key.F3;
            case KeyCode.F4: return Key.F4;
            case KeyCode.F5: return Key.F5;
            case KeyCode.F6: return Key.F6;
            case KeyCode.F7: return Key.F7;
            case KeyCode.F8: return Key.F8;
            case KeyCode.F9: return Key.F9;
            case KeyCode.F10: return Key.F10;
            case KeyCode.F11: return Key.F11;
            case KeyCode.F12: return Key.F12;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Tab: return Key.Tab;
            case KeyCode.Alpha1: return Key.Digit1;
            case KeyCode.Alpha2: return Key.Digit2;
            case KeyCode.Alpha3: return Key.Digit3;
            case KeyCode.Alpha4: return Key.Digit4;
            case KeyCode.Alpha5: return Key.Digit5;
            case KeyCode.Alpha6: return Key.Digit6;
            case KeyCode.Alpha7: return Key.Digit7;
            case KeyCode.Alpha8: return Key.Digit8;
            case KeyCode.Alpha9: return Key.Digit9;
            case KeyCode.Alpha0: return Key.Digit0;
            default: return Key.F1; // Default fallback
        }
    }

    // Event handlers for game system integration
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

    private void OnInventoryValueChanged(int newValue)
    {
        // HUD will update automatically through its own Update loop
        // This could trigger special notifications for milestones
        if (newValue > 0 && newValue % 1000 == 0)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification(
                    "Milestone Reached!",
                    $"Your total catch value has reached {newValue} coins!",
                    NotificationManager.NotificationType.Success,
                    4f
                );
            }
        }
    }

    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        // Show time of day notifications
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowTimeOfDayNotification(newTimeOfDay.ToString());
        }
    }

    private void OnDayChanged(int newDay)
    {
        // Show new day notification
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                "New Day",
                $"Day {newDay} has begun!",
                NotificationManager.NotificationType.Info,
                3f
            );
        }
    }

    // Public interface methods
    public void ShowGameHUD()
    {
        if (gameHUD != null)
            gameHUD.ShowPanel();
    }

    public void HideGameHUD()
    {
        if (gameHUD != null)
            gameHUD.HidePanel();
    }

    public void ShowFishingUI()
    {
        if (gameHUD != null)
            gameHUD.ShowFishingUI();
    }

    public void HideFishingUI()
    {
        if (gameHUD != null)
            gameHUD.HideFishingUI();
    }

    public void UpdateFishingProgress(float progress)
    {
        if (gameHUD != null)
            gameHUD.UpdateFishingProgress(progress);
    }

    public void UpdateFishingStatus(string status)
    {
        if (gameHUD != null)
            gameHUD.UpdateFishingStatus(status);
    }

    public void SetLocation(string location)
    {
        if (gameHUD != null)
            gameHUD.SetLocation(location);
    }

    public bool IsDebugUIVisible => debugUI != null && debugUI.IsVisible;

    public bool IsFishingUIVisible => gameHUD != null && gameHUD.IsFishingUIVisible;

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventorySystem != null)
        {
            inventorySystem.OnFishAdded -= OnFishAdded;
            inventorySystem.OnValueChanged -= OnInventoryValueChanged;
        }

        if (timeManager != null)
        {
            timeManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
            timeManager.OnDayChanged -= OnDayChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}

/// <summary>
/// Integration component for connecting fishing controller with new UI system
/// </summary>
public class FishingUIIntegration : MonoBehaviour
{
    private GameHUDController gameHUD;
    private FishingController fishingController;

    public void Initialize(GameHUDController hud, FishingController fishing)
    {
        gameHUD = hud;
        fishingController = fishing;

        // Connect fishing events to UI
        // Note: These events would need to be added to FishingController
        // fishingController.OnFishingStarted += OnFishingStarted;
        // fishingController.OnFishingEnded += OnFishingEnded;
        // fishingController.OnFishingProgressChanged += OnFishingProgressChanged;
    }

    private void OnFishingStarted()
    {
        if (gameHUD != null)
        {
            gameHUD.ShowFishingUI();
            gameHUD.UpdateFishingStatus("Fishing...");
            gameHUD.UpdateFishingInstruction("Wait for a bite...");
        }
    }

    private void OnFishingEnded()
    {
        if (gameHUD != null)
        {
            gameHUD.HideFishingUI();
        }
    }

    private void OnFishingProgressChanged(float progress)
    {
        if (gameHUD != null)
        {
            gameHUD.UpdateFishingProgress(progress);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        // fishingController.OnFishingStarted -= OnFishingStarted;
        // fishingController.OnFishingEnded -= OnFishingEnded;
        // fishingController.OnFishingProgressChanged -= OnFishingProgressChanged;
    }
}