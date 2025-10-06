using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUDController : UIToolkitPanel
{
    // Notification system
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    [Header("HUD Settings")] [SerializeField]
    private bool showInputHints = true;

    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private int maxNotifications = 5;
    private readonly List<VisualElement> activeNotifications = new();
    private string currentLocation = "Fishing Pond";
    private TimeOfDay currentTimeOfDay = TimeOfDay.Morning;

    // Top bar elements
    private Label dayLabel, timeLabel, timeOfDayLabel;
    private ProgressBar energyBar;
    private Label energyText;

    // Bottom bar elements
    private Label fishCount, moneyAmount;
    private FishingController fishingController;
    private Label fishingInstruction;

    // Fishing overlay
    private VisualElement fishingOverlay;
    private VisualElement fishingPanel;
    private ProgressBar fishingProgress;
    private Label fishingStatus;

    // Input hints
    private VisualElement inputHints;
    private InventorySystem inventorySystem;

    // State tracking
    private Label locationName, interactionHint;

    // Mini inventory
    private VisualElement miniInventory;
    private VisualElement miniInventoryGrid;

    // Notification system
    private VisualElement notificationArea;
    private Button openInventoryButton;
    private IsometricPlayerController playerController;

    // Game system references
    private GameTimeManager timeManager;
    private VisualElement weatherIcon;
    private Label weatherLabel;

    public bool IsFishingUIVisible { get; private set; }

    private void Update()
    {
        UpdateTimeDisplay();
        UpdatePlayerStats();
        UpdateInventoryDisplay();
        UpdateLocationInfo();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (timeManager != null)
        {
            timeManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;
            timeManager.OnDayChanged -= OnDayChanged;
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnFishAdded -= OnFishCaught;
            inventorySystem.OnValueChanged -= OnInventoryValueChanged;
        }
    }

    protected override void BindUIElements()
    {
        // Top bar elements
        dayLabel = GetElement<Label>("day-label");
        timeLabel = GetElement<Label>("time-label");
        timeOfDayLabel = GetElement<Label>("timeofday-label");
        weatherLabel = GetElement<Label>("weather-label");
        weatherIcon = GetElement<VisualElement>("weather-icon");
        energyBar = GetElement<ProgressBar>("energy-bar");
        energyText = GetElement<Label>("energy-text");

        // Bottom bar elements
        fishCount = GetElement<Label>("fish-count");
        moneyAmount = GetElement<Label>("money-amount");
        locationName = GetElement<Label>("location-name");
        interactionHint = GetElement<Label>("interaction-hint");

        // Notification system
        notificationArea = GetElement<VisualElement>("notification-area");

        // Fishing overlay
        fishingOverlay = GetElement<VisualElement>("fishing-overlay");
        fishingPanel = GetElement<VisualElement>("fishing-panel");
        fishingStatus = GetElement<Label>("fishing-status");
        fishingProgress = GetElement<ProgressBar>("fishing-progress");
        fishingInstruction = GetElement<Label>("fishing-instruction");

        // Mini inventory
        miniInventory = GetElement<VisualElement>("mini-inventory");
        miniInventoryGrid = GetElement<VisualElement>("mini-inventory-grid");
        openInventoryButton = GetElement<Button>("open-inventory-button");

        // Input hints
        inputHints = GetElement<VisualElement>("input-hints");
    }

    protected override void SetupEventHandlers()
    {
        // Inventory button
        if (openInventoryButton != null)
            openInventoryButton.clicked += OnOpenInventory;
    }

    protected override void InitializeData()
    {
        // Find game system references
        timeManager = FindObjectOfType<GameTimeManager>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        fishingController = FindObjectOfType<FishingController>();
        playerController = FindObjectOfType<IsometricPlayerController>();

        // Subscribe to events
        SubscribeToGameEvents();

        // Set initial state
        HideFishingUI();
        UpdateInputHints();

        // Set initial location
        if (locationName != null)
            locationName.text = currentLocation;

        // Hide interaction hint initially
        if (interactionHint != null)
            interactionHint.style.display = DisplayStyle.None;
    }

    private void SubscribeToGameEvents()
    {
        if (timeManager != null)
        {
            timeManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
            timeManager.OnDayChanged += OnDayChanged;
        }

        if (inventorySystem != null)
        {
            inventorySystem.OnFishAdded += OnFishCaught;
            inventorySystem.OnValueChanged += OnInventoryValueChanged;
        }

        // Note: FishingController would need these events added
        // fishingController.OnFishingStarted += OnFishingStarted;
        // fishingController.OnFishingEnded += OnFishingEnded;
        // fishingController.OnNearWaterChanged += OnNearWaterChanged;
    }

    private void UpdateTimeDisplay()
    {
        if (timeManager == null) return;

        if (dayLabel != null)
            dayLabel.text = $"Day {timeManager.GetCurrentDay()}";

        if (timeLabel != null)
            timeLabel.text = timeManager.GetTimeString();

        if (timeOfDayLabel != null)
            timeOfDayLabel.text = timeManager.GetCurrentTimeOfDay().ToString();
    }

    private void UpdatePlayerStats()
    {
        // Energy bar (placeholder - would need player energy system)
        if (energyBar != null)
        {
            var energy = 100f; // Placeholder
            energyBar.value = energy;

            if (energyText != null)
                energyText.text = $"{energy:F0}/100";
        }
    }

    private void UpdateInventoryDisplay()
    {
        if (inventorySystem == null) return;

        if (fishCount != null)
            fishCount.text = inventorySystem.GetInventoryCount().ToString();

        if (moneyAmount != null)
            moneyAmount.text = inventorySystem.GetTotalValue().ToString();

        UpdateMiniInventory();
    }

    private void UpdateMiniInventory()
    {
        if (miniInventoryGrid == null || inventorySystem == null) return;

        // Clear existing items
        miniInventoryGrid.Clear();

        // Add recent catches (limit to 6 items)
        var recentFish = inventorySystem.GetCaughtFish();
        var displayCount = Mathf.Min(recentFish.Count, 6);

        for (var i = recentFish.Count - displayCount; i < recentFish.Count; i++)
        {
            var fish = recentFish[i];
            var fishItem = CreateMiniInventoryItem(fish);
            miniInventoryGrid.Add(fishItem);
        }
    }

    private VisualElement CreateMiniInventoryItem(CaughtFish fish)
    {
        var item = new VisualElement();
        item.AddToClassList("mini-fish-item");

        // Add fish sprite if available
        if (fish.fishData.fishSprite != null)
            item.style.backgroundImage = new StyleBackground(fish.fishData.fishSprite);

        // Add tooltip functionality
        item.tooltip = $"{fish.fishData.fishName}\n{fish.weight:F1}kg - {fish.value} coins";

        return item;
    }

    private void UpdateLocationInfo()
    {
        // Update interaction hints based on context
        if (fishingController != null)
        {
            // This would need public properties in FishingController
            var nearWater = false; // fishingController.IsNearWater;
            var isFishing = false; // fishingController.IsFishing;

            if (interactionHint != null)
            {
                if (nearWater && !isFishing)
                {
                    interactionHint.text = "Left click to cast line";
                    interactionHint.style.display = DisplayStyle.Flex;
                }
                else if (isFishing)
                {
                    interactionHint.text = "E to cancel fishing";
                    interactionHint.style.display = DisplayStyle.Flex;
                }
                else
                {
                    interactionHint.style.display = DisplayStyle.None;
                }
            }
        }
    }

    private void UpdateInputHints()
    {
        if (inputHints != null)
            inputHints.style.display = showInputHints ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // Event handlers
    private void OnTimeOfDayChanged(TimeOfDay newTimeOfDay)
    {
        currentTimeOfDay = newTimeOfDay;
        UpdateWeatherDisplay();
    }

    private void OnDayChanged(int newDay)
    {
        ShowNotification("New Day", $"Day {newDay} has begun!");
    }

    private void OnFishCaught(CaughtFish fish)
    {
        ShowNotification("Fish Caught!",
            $"Caught {fish.fishData.fishName} ({fish.weight:F1}kg) worth {fish.value} coins!",
            NotificationType.Success);
    }

    private void OnInventoryValueChanged(int newValue)
    {
        // Could add animation or effect here
    }

    private void OnOpenInventory()
    {
        Debug.Log("OnOpenInventory");
        // This would open the full inventory UI
        if (UIPanelManager.Instance != null) UIPanelManager.Instance.TogglePanel("InventoryPanel");
    }

    private void UpdateWeatherDisplay()
    {
        // Placeholder weather system
        var weather = "Sunny";

        if (weatherLabel != null)
            weatherLabel.text = weather;

        // Update weather icon color based on weather
        if (weatherIcon != null)
            switch (weather)
            {
                case "Sunny":
                    weatherIcon.style.backgroundColor = new Color(1f, 0.8f, 0.4f, 0.8f);
                    break;
                case "Cloudy":
                    weatherIcon.style.backgroundColor = new Color(0.7f, 0.7f, 0.8f, 0.8f);
                    break;
                case "Rainy":
                    weatherIcon.style.backgroundColor = new Color(0.5f, 0.6f, 0.8f, 0.8f);
                    break;
                default:
                    weatherIcon.style.backgroundColor = new Color(1f, 0.8f, 0.4f, 0.8f);
                    break;
            }
    }

    // Fishing UI methods
    public void ShowFishingUI()
    {
        if (fishingOverlay != null)
        {
            fishingOverlay.style.display = DisplayStyle.Flex;
            IsFishingUIVisible = true;
        }
    }

    public void HideFishingUI()
    {
        if (fishingOverlay != null)
        {
            fishingOverlay.style.display = DisplayStyle.None;
            IsFishingUIVisible = false;
        }
    }

    public void UpdateFishingStatus(string status)
    {
        if (fishingStatus != null)
            fishingStatus.text = status;
    }

    public void UpdateFishingProgress(float progress)
    {
        if (fishingProgress != null)
            fishingProgress.value = progress;
    }

    public void UpdateFishingInstruction(string instruction)
    {
        if (fishingInstruction != null)
            fishingInstruction.text = instruction;
    }

    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        if (notificationArea == null) return;

        // Create notification element
        var notification = CreateNotificationElement(title, message, type);

        // Add to area and track
        notificationArea.Add(notification);
        activeNotifications.Add(notification);

        // Remove excess notifications
        while (activeNotifications.Count > maxNotifications)
        {
            var oldNotification = activeNotifications[0];
            activeNotifications.RemoveAt(0);
            RemoveNotification(oldNotification);
        }

        // Auto-remove after duration
        StartCoroutine(RemoveNotificationAfterDelay(notification, notificationDuration));
    }

    private VisualElement CreateNotificationElement(string title, string message, NotificationType type)
    {
        var notification = new VisualElement();
        notification.AddToClassList("notification-toast");

        // Add icon
        var icon = new VisualElement();
        icon.AddToClassList("notification-icon");

        // Set icon color based on type
        switch (type)
        {
            case NotificationType.Success:
                icon.style.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                notification.style.borderLeftColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                break;
            case NotificationType.Warning:
                icon.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                notification.style.borderLeftColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                break;
            case NotificationType.Error:
                icon.style.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);
                notification.style.borderLeftColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);
                break;
            default: // Info
                icon.style.backgroundColor = new Color(0.4f, 0.6f, 0.8f, 0.8f);
                notification.style.borderLeftColor = new Color(0.4f, 0.6f, 0.8f, 0.8f);
                break;
        }

        // Add title
        var titleLabel = new Label(title);
        titleLabel.AddToClassList("notification-title");

        // Add message
        var messageLabel = new Label(message);
        messageLabel.AddToClassList("notification-message");

        // Assemble notification
        var content = new VisualElement();
        content.style.flexDirection = FlexDirection.Row;

        var textContainer = new VisualElement();
        textContainer.style.flexDirection = FlexDirection.Column;
        textContainer.style.flexGrow = 1;
        textContainer.Add(titleLabel);
        textContainer.Add(messageLabel);

        content.Add(icon);
        content.Add(textContainer);
        notification.Add(content);

        return notification;
    }

    private IEnumerator RemoveNotificationAfterDelay(VisualElement notification, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveNotification(notification);
    }

    private void RemoveNotification(VisualElement notification)
    {
        if (notification == null) return;

        // Add fade out class
        notification.AddToClassList("notification-hidden");

        // Remove from tracking
        activeNotifications.Remove(notification);

        // Remove from DOM after animation
        StartCoroutine(RemoveNotificationFromDOM(notification, 0.3f));
    }

    private IEnumerator RemoveNotificationFromDOM(VisualElement notification, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notification.parent != null)
            notification.parent.Remove(notification);
    }

    // Public interface methods
    public void SetLocation(string location)
    {
        currentLocation = location;
        if (locationName != null)
            locationName.text = location;
    }

    public void SetInputHintsVisible(bool visible)
    {
        showInputHints = visible;
        UpdateInputHints();
    }
}