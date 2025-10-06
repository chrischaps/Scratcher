using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FishingGameUI : MonoBehaviour
{
    [Header("Main UI Elements")] [SerializeField]
    private GameObject mainUI;

    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private TextMeshProUGUI inventoryCountText;
    [SerializeField] private TextMeshProUGUI totalValueText;

    [Header("Fishing UI")] [SerializeField]
    private GameObject fishingPanel;

    [SerializeField] private TextMeshProUGUI fishingInstructions;
    [SerializeField] private Slider fishingProgress;

    [Header("Water Zone UI")] [SerializeField]
    private GameObject waterZoneInfo;

    [SerializeField] private TextMeshProUGUI waterZoneText;

    [Header("Catch Notification")] [SerializeField]
    private GameObject catchNotification;

    [SerializeField] private Image fishImage;
    [SerializeField] private TextMeshProUGUI fishNameText;
    [SerializeField] private TextMeshProUGUI fishWeightText;
    [SerializeField] private TextMeshProUGUI fishValueText;

    [Header("Inventory Panel")] [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField] private Transform inventoryContent;
    [SerializeField] private GameObject fishItemPrefab;
    [SerializeField] private Button inventoryToggleButton;
    private InventorySystem inventorySystem;
    private bool isInventoryOpen;

    private GameTimeManager timeManager;

    private void Start()
    {
        timeManager = FindObjectOfType<GameTimeManager>();
        inventorySystem = FindObjectOfType<InventorySystem>();

        SetupUI();
        SubscribeToEvents();
    }

    private void Update()
    {
        UpdateTimeDisplay();
        UpdateInventoryDisplay();
    }

    private void OnDestroy()
    {
        if (timeManager != null)
            timeManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;

        if (inventorySystem != null)
        {
            inventorySystem.OnFishAdded -= ShowCatchNotification;
            inventorySystem.OnValueChanged -= OnValueChanged;
        }
    }

    private void SetupUI()
    {
        if (fishingPanel != null)
            fishingPanel.SetActive(false);

        if (waterZoneInfo != null)
            waterZoneInfo.SetActive(false);

        if (catchNotification != null)
            catchNotification.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (inventoryToggleButton != null)
            inventoryToggleButton.onClick.AddListener(ToggleInventory);
    }

    private void SubscribeToEvents()
    {
        if (timeManager != null) timeManager.OnTimeOfDayChanged += OnTimeOfDayChanged;

        if (inventorySystem != null)
        {
            inventorySystem.OnFishAdded += ShowCatchNotification;
            inventorySystem.OnValueChanged += OnValueChanged;
        }
    }

    private void OnTimeOfDayChanged(TimeOfDay timeOfDay)
    {
        UpdateTimeDisplay();
    }

    private void OnValueChanged(int newValue)
    {
        UpdateInventoryDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeManager != null && timeDisplay != null)
            timeDisplay.text = $"Day {timeManager.GetCurrentDay()} - {timeManager.GetTimeString()}";
    }

    private void UpdateInventoryDisplay()
    {
        if (inventorySystem != null)
        {
            if (inventoryCountText != null)
                inventoryCountText.text = $"Fish: {inventorySystem.GetInventoryCount()}";

            if (totalValueText != null)
                totalValueText.text = $"Value: {inventorySystem.GetTotalValue()} coins";
        }
    }

    public void ShowFishingUI(bool show)
    {
        if (fishingPanel != null)
            fishingPanel.SetActive(show);
    }

    public void UpdateFishingInstructions(string text)
    {
        if (fishingInstructions != null)
            fishingInstructions.text = text;
    }

    public void UpdateFishingProgress(float progress)
    {
        if (fishingProgress != null)
        {
            fishingProgress.gameObject.SetActive(progress >= 0);
            fishingProgress.value = progress;
        }
    }

    public void ShowWaterZoneInfo(string zoneInfo)
    {
        if (waterZoneInfo != null && waterZoneText != null)
        {
            waterZoneInfo.SetActive(true);
            waterZoneText.text = zoneInfo;
        }
    }

    public void HideWaterZoneInfo()
    {
        if (waterZoneInfo != null)
            waterZoneInfo.SetActive(false);
    }

    private void ShowCatchNotification(CaughtFish fish)
    {
        if (catchNotification == null) return;

        StartCoroutine(DisplayCatchNotification(fish));
    }

    private IEnumerator DisplayCatchNotification(CaughtFish fish)
    {
        if (fishImage != null && fish.fishData.fishSprite != null)
            fishImage.sprite = fish.fishData.fishSprite;

        if (fishNameText != null)
            fishNameText.text = fish.fishData.fishName;

        if (fishWeightText != null)
            fishWeightText.text = $"{fish.weight:F1} kg";

        if (fishValueText != null)
            fishValueText.text = $"{fish.value} coins";

        catchNotification.SetActive(true);

        yield return new WaitForSeconds(3f);

        catchNotification.SetActive(false);
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            RefreshInventoryDisplay();
    }

    private void RefreshInventoryDisplay()
    {
        if (inventoryContent == null || fishItemPrefab == null || inventorySystem == null)
            return;

        // Clear existing items
        foreach (Transform child in inventoryContent) Destroy(child.gameObject);

        // Add fish items
        var caughtFish = inventorySystem.GetCaughtFish();
        foreach (var fish in caughtFish) CreateFishInventoryItem(fish);
    }

    private void CreateFishInventoryItem(CaughtFish fish)
    {
        var item = Instantiate(fishItemPrefab, inventoryContent);

        // Set up the item (assuming the prefab has the right components)
        var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = $"{fish.fishData.fishName} ({fish.weight:F1}kg) - {fish.value} coins";

        var image = item.GetComponentInChildren<Image>();
        if (image != null && fish.fishData.fishSprite != null)
            image.sprite = fish.fishData.fishSprite;

        // Add sell button functionality
        var sellButton = item.GetComponentInChildren<Button>();
        if (sellButton != null)
            sellButton.onClick.AddListener(() =>
            {
                inventorySystem.SellFish(fish);
                RefreshInventoryDisplay();
            });
    }
}