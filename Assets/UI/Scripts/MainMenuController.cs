using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class MainMenuController : UIToolkitPanel
{
    [Header("Scene Management")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string saveFileDirectory = "saves";

    // Main navigation buttons
    private Button newGameButton, continueButton, loadGameButton;
    private Button settingsButton, creditsButton, quitButton;

    // Panel containers
    private VisualElement settingsPanel, loadPanel, creditsPanel;

    // Settings panel elements
    private Button closeSettingsButton, resetSettingsButton, applySettingsButton;
    private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider;
    private Label masterVolumeText, musicVolumeText, sfxVolumeText;
    private DropdownField qualityDropdown, resolutionDropdown;
    private Toggle fullscreenToggle, vsyncToggle;
    private Toggle autosaveToggle, tooltipsToggle, inputHintsToggle;
    private Slider cameraSmoothingSlider;

    // Load panel elements
    private Button closeLoadButton, deleteSelectedButton, loadSelectedButton;
    private ScrollView saveSlots;
    private VisualElement selectedSaveSlot;

    // Credits panel elements
    private Button closeCreditsButton;

    // Version info
    private Label versionLabel, debugHintLabel;

    // Game settings data
    private GameSettings gameSettings;

    // Save data
    private List<SaveData> availableSaves = new List<SaveData>();

    [System.Serializable]
    public class GameSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 0.9f;
        public int qualityLevel = 2;
        public Resolution resolution;
        public bool fullscreen = true;
        public bool vsync = true;
        public bool autoSave = true;
        public bool showTooltips = true;
        public bool showInputHints = true;
        public float cameraSmoothing = 0.5f;
    }

    [System.Serializable]
    public class SaveData
    {
        public string fileName;
        public string displayName;
        public string lastPlayed;
        public int dayNumber;
        public int fishCaught;
        public int totalValue;
        public float playtime;
    }

    protected override void BindUIElements()
    {
        // Main navigation
        newGameButton = GetElement<Button>("new-game-button");
        continueButton = GetElement<Button>("continue-button");
        loadGameButton = GetElement<Button>("load-game-button");
        settingsButton = GetElement<Button>("settings-button");
        creditsButton = GetElement<Button>("credits-button");
        quitButton = GetElement<Button>("quit-button");

        // Panel containers
        settingsPanel = GetElement<VisualElement>("settings-panel");
        loadPanel = GetElement<VisualElement>("load-panel");
        creditsPanel = GetElement<VisualElement>("credits-panel");

        // Settings elements
        closeSettingsButton = GetElement<Button>("close-settings");
        resetSettingsButton = GetElement<Button>("reset-settings");
        applySettingsButton = GetElement<Button>("apply-settings");

        // Audio settings
        masterVolumeSlider = GetElement<Slider>("master-volume");
        musicVolumeSlider = GetElement<Slider>("music-volume");
        sfxVolumeSlider = GetElement<Slider>("sfx-volume");
        masterVolumeText = GetElement<Label>("master-volume-text");
        musicVolumeText = GetElement<Label>("music-volume-text");
        sfxVolumeText = GetElement<Label>("sfx-volume-text");

        // Graphics settings
        qualityDropdown = GetElement<DropdownField>("quality-dropdown");
        resolutionDropdown = GetElement<DropdownField>("resolution-dropdown");
        fullscreenToggle = GetElement<Toggle>("fullscreen-toggle");
        vsyncToggle = GetElement<Toggle>("vsync-toggle");

        // Gameplay settings
        autosaveToggle = GetElement<Toggle>("autosave-toggle");
        tooltipsToggle = GetElement<Toggle>("tooltips-toggle");
        inputHintsToggle = GetElement<Toggle>("input-hints-toggle");
        cameraSmoothingSlider = GetElement<Slider>("camera-smoothing");

        // Load panel elements
        closeLoadButton = GetElement<Button>("close-load");
        deleteSelectedButton = GetElement<Button>("delete-save");
        loadSelectedButton = GetElement<Button>("load-selected");
        saveSlots = GetElement<ScrollView>("save-slots");

        // Credits panel
        closeCreditsButton = GetElement<Button>("close-credits");

        // Version info
        versionLabel = GetElement<Label>("version-label");
        debugHintLabel = GetElement<Label>("debug-hint");
    }

    protected override void SetupEventHandlers()
    {
        // Main navigation
        if (newGameButton != null) newGameButton.clicked += OnNewGame;
        if (continueButton != null) continueButton.clicked += OnContinueGame;
        if (loadGameButton != null) loadGameButton.clicked += OnShowLoadPanel;
        if (settingsButton != null) settingsButton.clicked += OnShowSettingsPanel;
        if (creditsButton != null) creditsButton.clicked += OnShowCreditsPanel;
        if (quitButton != null) quitButton.clicked += OnQuitGame;

        // Settings panel
        if (closeSettingsButton != null) closeSettingsButton.clicked += OnCloseSettingsPanel;
        if (resetSettingsButton != null) resetSettingsButton.clicked += OnResetSettings;
        if (applySettingsButton != null) applySettingsButton.clicked += OnApplySettings;

        // Volume sliders
        if (masterVolumeSlider != null) masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.RegisterValueChangedCallback(OnSfxVolumeChanged);

        // Graphics settings
        if (qualityDropdown != null) qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
        if (resolutionDropdown != null) resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        if (fullscreenToggle != null) fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
        if (vsyncToggle != null) vsyncToggle.RegisterValueChangedCallback(OnVsyncChanged);

        // Gameplay settings
        if (autosaveToggle != null) autosaveToggle.RegisterValueChangedCallback(OnAutosaveChanged);
        if (tooltipsToggle != null) tooltipsToggle.RegisterValueChangedCallback(OnTooltipsChanged);
        if (inputHintsToggle != null) inputHintsToggle.RegisterValueChangedCallback(OnInputHintsChanged);
        if (cameraSmoothingSlider != null) cameraSmoothingSlider.RegisterValueChangedCallback(OnCameraSmoothingChanged);

        // Load panel
        if (closeLoadButton != null) closeLoadButton.clicked += OnCloseLoadPanel;
        if (deleteSelectedButton != null) deleteSelectedButton.clicked += OnDeleteSelectedSave;
        if (loadSelectedButton != null) loadSelectedButton.clicked += OnLoadSelectedSave;

        // Credits panel
        if (closeCreditsButton != null) closeCreditsButton.clicked += OnCloseCreditsPanel;
    }

    protected override void InitializeData()
    {
        // Load settings
        LoadSettings();

        // Initialize dropdowns
        InitializeQualityDropdown();
        InitializeResolutionDropdown();

        // Apply current settings to UI
        ApplySettingsToUI();

        // Hide all panels initially
        HideAllPanels();

        // Load available saves
        LoadAvailableSaves();

        // Set version info
        if (versionLabel != null)
            versionLabel.text = $"v{Application.version}";

        // Check if continue button should be enabled
        UpdateContinueButton();
    }

    private void LoadSettings()
    {
        gameSettings = new GameSettings();

        // Load from PlayerPrefs (could be replaced with JSON file)
        gameSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        gameSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        gameSettings.sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 0.9f);
        gameSettings.qualityLevel = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        gameSettings.fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        gameSettings.vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
        gameSettings.autoSave = PlayerPrefs.GetInt("AutoSave", 1) == 1;
        gameSettings.showTooltips = PlayerPrefs.GetInt("ShowTooltips", 1) == 1;
        gameSettings.showInputHints = PlayerPrefs.GetInt("ShowInputHints", 1) == 1;
        gameSettings.cameraSmoothing = PlayerPrefs.GetFloat("CameraSmoothing", 0.5f);

        gameSettings.resolution = Screen.currentResolution;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", gameSettings.masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", gameSettings.musicVolume);
        PlayerPrefs.SetFloat("SfxVolume", gameSettings.sfxVolume);
        PlayerPrefs.SetInt("QualityLevel", gameSettings.qualityLevel);
        PlayerPrefs.SetInt("Fullscreen", gameSettings.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", gameSettings.vsync ? 1 : 0);
        PlayerPrefs.SetInt("AutoSave", gameSettings.autoSave ? 1 : 0);
        PlayerPrefs.SetInt("ShowTooltips", gameSettings.showTooltips ? 1 : 0);
        PlayerPrefs.SetInt("ShowInputHints", gameSettings.showInputHints ? 1 : 0);
        PlayerPrefs.SetFloat("CameraSmoothing", gameSettings.cameraSmoothing);
        PlayerPrefs.Save();
    }

    private void InitializeQualityDropdown()
    {
        if (qualityDropdown == null) return;

        var qualityNames = QualitySettings.names;
        var choices = new List<string>(qualityNames);
        qualityDropdown.choices = choices;
        qualityDropdown.value = qualityNames[gameSettings.qualityLevel];
    }

    private void InitializeResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        var resolutions = Screen.resolutions;
        var choices = new List<string>();

        foreach (var res in resolutions)
        {
            choices.Add($"{res.width}x{res.height} @ {res.refreshRate}Hz");
        }

        resolutionDropdown.choices = choices;

        // Set current resolution
        var currentRes = Screen.currentResolution;
        var currentChoice = $"{currentRes.width}x{currentRes.height} @ {currentRes.refreshRate}Hz";
        resolutionDropdown.value = currentChoice;
    }

    private void ApplySettingsToUI()
    {
        // Audio settings
        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(gameSettings.masterVolume * 100);
        if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(gameSettings.musicVolume * 100);
        if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(gameSettings.sfxVolume * 100);

        UpdateVolumeTexts();

        // Graphics settings
        if (fullscreenToggle != null) fullscreenToggle.SetValueWithoutNotify(gameSettings.fullscreen);
        if (vsyncToggle != null) vsyncToggle.SetValueWithoutNotify(gameSettings.vsync);

        // Gameplay settings
        if (autosaveToggle != null) autosaveToggle.SetValueWithoutNotify(gameSettings.autoSave);
        if (tooltipsToggle != null) tooltipsToggle.SetValueWithoutNotify(gameSettings.showTooltips);
        if (inputHintsToggle != null) inputHintsToggle.SetValueWithoutNotify(gameSettings.showInputHints);
        if (cameraSmoothingSlider != null) cameraSmoothingSlider.SetValueWithoutNotify(gameSettings.cameraSmoothing);
    }

    private void UpdateVolumeTexts()
    {
        if (masterVolumeText != null) masterVolumeText.text = $"{gameSettings.masterVolume * 100:F0}%";
        if (musicVolumeText != null) musicVolumeText.text = $"{gameSettings.musicVolume * 100:F0}%";
        if (sfxVolumeText != null) sfxVolumeText.text = $"{gameSettings.sfxVolume * 100:F0}%";
    }

    private void LoadAvailableSaves()
    {
        availableSaves.Clear();

        string savePath = Path.Combine(Application.persistentDataPath, saveFileDirectory);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            return;
        }

        string[] saveFiles = Directory.GetFiles(savePath, "*.save");
        foreach (string file in saveFiles)
        {
            try
            {
                // This is a placeholder - you'd implement actual save file reading
                var saveData = new SaveData
                {
                    fileName = Path.GetFileName(file),
                    displayName = Path.GetFileNameWithoutExtension(file),
                    lastPlayed = File.GetLastWriteTime(file).ToString("yyyy-MM-dd HH:mm"),
                    dayNumber = Random.Range(1, 100), // Placeholder
                    fishCaught = Random.Range(0, 50), // Placeholder
                    totalValue = Random.Range(0, 1000), // Placeholder
                    playtime = Random.Range(1f, 100f) // Placeholder
                };

                availableSaves.Add(saveData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load save file {file}: {e.Message}");
            }
        }

        // Sort by last played
        availableSaves.Sort((a, b) => b.lastPlayed.CompareTo(a.lastPlayed));
    }

    private void UpdateContinueButton()
    {
        if (continueButton == null) return;

        bool hasSaves = availableSaves.Count > 0;
        continueButton.SetEnabled(hasSaves);

        if (!hasSaves)
        {
            continueButton.style.opacity = 0.5f;
        }
    }

    private void PopulateSaveSlots()
    {
        if (saveSlots == null) return;

        saveSlots.Clear();

        foreach (var save in availableSaves)
        {
            var slot = CreateSaveSlotElement(save);
            saveSlots.Add(slot);
        }
    }

    private VisualElement CreateSaveSlotElement(SaveData save)
    {
        var slot = new VisualElement();
        slot.AddToClassList("save-slot");
        slot.userData = save;

        // Save info container
        var info = new VisualElement();
        info.AddToClassList("save-info");

        var nameLabel = new Label(save.displayName);
        nameLabel.AddToClassList("save-name");

        var detailsLabel = new Label($"Day {save.dayNumber} | {save.fishCaught} fish | {save.totalValue} coins | {save.lastPlayed}");
        detailsLabel.AddToClassList("save-details");

        info.Add(nameLabel);
        info.Add(detailsLabel);

        // Save preview (placeholder)
        var preview = new VisualElement();
        preview.AddToClassList("save-preview");

        slot.Add(info);
        slot.Add(preview);

        // Click handler
        slot.RegisterCallback<ClickEvent>(evt => OnSaveSlotClicked(slot));

        return slot;
    }

    private void OnSaveSlotClicked(VisualElement slot)
    {
        // Remove selection from other slots
        if (saveSlots != null)
        {
            foreach (var child in saveSlots.Children())
            {
                child.RemoveFromClassList("selected");
            }
        }

        // Select this slot
        slot.AddToClassList("selected");
        selectedSaveSlot = slot;

        // Enable load button
        if (loadSelectedButton != null)
            loadSelectedButton.SetEnabled(true);

        if (deleteSelectedButton != null)
            deleteSelectedButton.SetEnabled(true);
    }

    private void HideAllPanels()
    {
        if (settingsPanel != null) settingsPanel.style.display = DisplayStyle.None;
        if (loadPanel != null) loadPanel.style.display = DisplayStyle.None;
        if (creditsPanel != null) creditsPanel.style.display = DisplayStyle.None;
    }

    // Event handlers
    private void OnNewGame()
    {
        Debug.Log("Starting new game...");
        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnContinueGame()
    {
        if (availableSaves.Count > 0)
        {
            Debug.Log($"Continuing game: {availableSaves[0].displayName}");
            // Load the most recent save
            SceneManager.LoadScene(gameSceneName);
        }
    }

    private void OnShowLoadPanel()
    {
        HideAllPanels();
        if (loadPanel != null)
        {
            loadPanel.style.display = DisplayStyle.Flex;
            PopulateSaveSlots();
        }
    }

    private void OnShowSettingsPanel()
    {
        HideAllPanels();
        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnShowCreditsPanel()
    {
        HideAllPanels();
        if (creditsPanel != null)
            creditsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnQuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Settings event handlers
    private void OnCloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.style.display = DisplayStyle.None;
    }

    private void OnResetSettings()
    {
        gameSettings = new GameSettings();
        ApplySettingsToUI();
    }

    private void OnApplySettings()
    {
        // Apply graphics settings
        QualitySettings.SetQualityLevel(gameSettings.qualityLevel);
        Screen.SetResolution(gameSettings.resolution.width, gameSettings.resolution.height, gameSettings.fullscreen, gameSettings.resolution.refreshRate);
        QualitySettings.vSyncCount = gameSettings.vsync ? 1 : 0;

        // Save settings
        SaveSettings();

        // Close panel
        OnCloseSettingsPanel();
    }

    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        gameSettings.masterVolume = evt.newValue / 100f;
        UpdateVolumeTexts();
        // Apply to audio system
        AudioListener.volume = gameSettings.masterVolume;
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        gameSettings.musicVolume = evt.newValue / 100f;
        UpdateVolumeTexts();
        // Apply to music audio source
    }

    private void OnSfxVolumeChanged(ChangeEvent<float> evt)
    {
        gameSettings.sfxVolume = evt.newValue / 100f;
        UpdateVolumeTexts();
        // Apply to SFX audio sources
    }

    private void OnQualityChanged(ChangeEvent<string> evt)
    {
        var qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            if (qualityNames[i] == evt.newValue)
            {
                gameSettings.qualityLevel = i;
                break;
            }
        }
    }

    private void OnResolutionChanged(ChangeEvent<string> evt)
    {
        var resolutions = Screen.resolutions;
        for (int i = 0; i < resolutions.Length; i++)
        {
            var resString = $"{resolutions[i].width}x{resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
            if (resString == evt.newValue)
            {
                gameSettings.resolution = resolutions[i];
                break;
            }
        }
    }

    private void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        gameSettings.fullscreen = evt.newValue;
    }

    private void OnVsyncChanged(ChangeEvent<bool> evt)
    {
        gameSettings.vsync = evt.newValue;
    }

    private void OnAutosaveChanged(ChangeEvent<bool> evt)
    {
        gameSettings.autoSave = evt.newValue;
    }

    private void OnTooltipsChanged(ChangeEvent<bool> evt)
    {
        gameSettings.showTooltips = evt.newValue;
    }

    private void OnInputHintsChanged(ChangeEvent<bool> evt)
    {
        gameSettings.showInputHints = evt.newValue;
    }

    private void OnCameraSmoothingChanged(ChangeEvent<float> evt)
    {
        gameSettings.cameraSmoothing = evt.newValue;
    }

    // Load panel event handlers
    private void OnCloseLoadPanel()
    {
        if (loadPanel != null)
            loadPanel.style.display = DisplayStyle.None;
    }

    private void OnDeleteSelectedSave()
    {
        if (selectedSaveSlot != null)
        {
            var saveData = selectedSaveSlot.userData as SaveData;
            if (saveData != null)
            {
                // Delete save file
                string savePath = Path.Combine(Application.persistentDataPath, saveFileDirectory, saveData.fileName);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log($"Deleted save file: {saveData.fileName}");
                }

                // Refresh save list
                LoadAvailableSaves();
                PopulateSaveSlots();
                selectedSaveSlot = null;

                if (loadSelectedButton != null) loadSelectedButton.SetEnabled(false);
                if (deleteSelectedButton != null) deleteSelectedButton.SetEnabled(false);
            }
        }
    }

    private void OnLoadSelectedSave()
    {
        if (selectedSaveSlot != null)
        {
            var saveData = selectedSaveSlot.userData as SaveData;
            if (saveData != null)
            {
                Debug.Log($"Loading save: {saveData.displayName}");
                // Load the selected save and start game
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }

    // Credits panel event handlers
    private void OnCloseCreditsPanel()
    {
        if (creditsPanel != null)
            creditsPanel.style.display = DisplayStyle.None;
    }
}