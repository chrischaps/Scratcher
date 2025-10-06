using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
///     Bridges game systems with UI Toolkit components
///     Handles event wiring, notifications, and high-level UI coordination
/// </summary>
public class GameUIController : MonoBehaviour
{
    [Header("UI Documents")] [SerializeField]
    private UIDocument gameHUDDocument;

    [SerializeField] private UIDocument debugUIDocument;
    [SerializeField] private UIDocument mainMenuDocument;

    [Header("Integration Settings")] [SerializeField]
    private bool replaceOldUI = true;

    [SerializeField] private bool enableDebugMode = true;
    private DebugUIManager debugUI;
    private FishingController fishingController;

    // UI Controllers
    private GameHUDController gameHUD;

    // Game system references
    private GameManager gameManager;
    private InventorySystem inventorySystem;

    // Legacy UI components to replace
    private FishingGameUI legacyFishingUI;
    private MainMenuController mainMenu;
    private GameTimeManager timeManager;

    public static GameUIController Instance { get; private set; }

    public bool IsDebugUIVisible => debugUI != null && debugUI.IsVisible;

    public bool IsFishingUIVisible => gameHUD != null && gameHUD.IsFishingUIVisible;

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

        if (Instance == this) Instance = null;
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
        if (replaceOldUI) ReplaceLegacyUI();

        Debug.Log("GameUIController initialized successfully");
    }

    private void SetupUIDocuments()
    {
        // Game HUD Document
        if (gameHUDDocument == null)
        {
            gameHUDDocument = FindUIDocument("GameHUD");
            if (gameHUDDocument == null) gameHUDDocument = CreateUIDocument("GameHUD", "GameHUD");
        }

        // Debug UI Document
        if (debugUIDocument == null && enableDebugMode)
        {
            debugUIDocument = FindUIDocument("DebugPanel");
            if (debugUIDocument == null) debugUIDocument = CreateUIDocument("DebugPanel", "DebugPanel");
        }

        // Main Menu Document (if in main menu scene)
        if (SceneManager.GetActiveScene().name == "MainMenu")
            if (mainMenuDocument == null)
            {
                mainMenuDocument = FindUIDocument("MainMenu");
                if (mainMenuDocument == null) mainMenuDocument = CreateUIDocument("MainMenu", "MainMenu");
            }
    }

    private UIDocument FindUIDocument(string documentName)
    {
        var documents = FindObjectsOfType<UIDocument>();
        foreach (var doc in documents)
            if (doc.name == documentName || doc.gameObject.name == documentName)
                return doc;

        return null;
    }

    private UIDocument CreateUIDocument(string name, string assetName)
    {
        var gameObject = new GameObject(name);
        gameObject.transform.SetParent(transform);

        var document = gameObject.AddComponent<UIDocument>();

        // Try to load the visual tree asset
        var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UI/UXML/{assetName}");
        if (visualTreeAsset != null) document.visualTreeAsset = visualTreeAsset;

        // Try to load the style sheet
        var styleSheet = Resources.Load<StyleSheet>($"UI/USS/{assetName}");
        if (styleSheet != null && document.rootVisualElement != null)
            document.rootVisualElement.styleSheets.Add(styleSheet);

        return document;
    }

    private void SetupUIControllers()
    {
        // Game HUD Controller
        if (gameHUDDocument != null)
        {
            gameHUD = gameHUDDocument.GetComponent<GameHUDController>();
            if (gameHUD == null) gameHUD = gameHUDDocument.gameObject.AddComponent<GameHUDController>();
        }

        // Debug UI Controller
        if (debugUIDocument != null && enableDebugMode)
        {
            debugUI = debugUIDocument.GetComponent<DebugUIManager>();
            if (debugUI == null) debugUI = debugUIDocument.gameObject.AddComponent<DebugUIManager>();
        }

        // Main Menu Controller
        if (mainMenuDocument != null)
        {
            mainMenu = mainMenuDocument.GetComponent<MainMenuController>();
            if (mainMenu == null) mainMenu = mainMenuDocument.gameObject.AddComponent<MainMenuController>();
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
        // Connect UI Panel Manager to game manager
        var panelManager = GetComponent<UIPanelManager>();
        if (panelManager == null) panelManager = gameObject.AddComponent<UIPanelManager>();

        // Register UI panels with UIPanelManager
        if (gameHUD != null) panelManager.RegisterPanel(gameHUD);
        if (debugUI != null) panelManager.RegisterPanel(debugUI);
        if (mainMenu != null) panelManager.RegisterPanel(mainMenu);

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
            if (canvas.name.Contains("Legacy") || canvas.name.Contains("Old"))
            {
                canvas.gameObject.SetActive(false);
                Debug.Log($"Disabled legacy canvas: {canvas.name}");
            }
    }

    // Event handlers for game system integration
    private void OnFishAdded(CaughtFish fish)
    {
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowFishCaughtNotification(
                fish.fishData.fishName,
                fish.weight,
                fish.value
            );
    }

    private void OnInventoryValueChanged(int newValue)
    {
        // HUD will update automatically through its own Update loop
        // This could trigger special notifications for milestones
        if (newValue > 0 && newValue % 1000 == 0)
            if (NotificationManager.Instance != null)
                NotificationManager.Instance.ShowNotification(
                    "Milestone Reached!",
                    $"Your total catch value has reached {newValue} coins!",
                    NotificationManager.NotificationType.Success,
                    4f
                );
    }

    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        // Show time of day notifications
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowTimeOfDayNotification(newTimeOfDay.ToString());
    }

    private void OnDayChanged(int newDay)
    {
        // Show new day notification
        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowNotification(
                "New Day",
                $"Day {newDay} has begun!",
                NotificationManager.NotificationType.Info,
                3f
            );
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
}

/// <summary>
///     Integration component for connecting fishing controller with new UI system
/// </summary>
public class FishingUIIntegration : MonoBehaviour
{
    private FishingController fishingController;
    private GameHUDController gameHUD;

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        // fishingController.OnFishingStarted -= OnFishingStarted;
        // fishingController.OnFishingEnded -= OnFishingEnded;
        // fishingController.OnFishingProgressChanged -= OnFishingProgressChanged;
    }

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
        if (gameHUD != null) gameHUD.HideFishingUI();
    }

    private void OnFishingProgressChanged(float progress)
    {
        if (gameHUD != null) gameHUD.UpdateFishingProgress(progress);
    }
}