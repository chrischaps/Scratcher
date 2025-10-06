using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("UI Documents")] [SerializeField]
    private List<UIDocument> uiDocuments = new();

    [Header("Panel Management")] [SerializeField]
    private List<UIToolkitPanel> managedPanels = new();

    [Header("Debug Settings")] [SerializeField]
    private KeyCode debugToggleKey = KeyCode.F1;

    [SerializeField] private bool enableDebugUI = true;

    private readonly Dictionary<string, UIToolkitPanel> panelRegistry = new();
    private readonly Stack<UIToolkitPanel> panelStack = new();

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUIManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        HandleDebugInput();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void InitializeUIManager()
    {
        // Find all UI documents in the scene if not assigned
        if (uiDocuments.Count == 0) uiDocuments.AddRange(FindObjectsOfType<UIDocument>());

        // Find all UI panels if not assigned
        if (managedPanels.Count == 0) managedPanels.AddRange(FindObjectsOfType<UIToolkitPanel>());

        // Register panels
        foreach (var panel in managedPanels) RegisterPanel(panel);

        Debug.Log($"UIManager initialized with {uiDocuments.Count} documents and {managedPanels.Count} panels");
    }

    private void HandleDebugInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (enableDebugUI && keyboard[ConvertKeyCodeToKey(debugToggleKey)].wasPressedThisFrame) ToggleDebugUI();

            // ESC key to close top panel
            if (keyboard[Key.Escape].wasPressedThisFrame) CloseTopPanel();
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
            case KeyCode.Tilde: return Key.Backquote;
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

    public void RegisterPanel(UIToolkitPanel panel)
    {
        if (panel == null) return;

        var panelId = panel.name;
        if (panelRegistry.ContainsKey(panelId)) Debug.LogWarning($"Panel {panelId} already registered, overwriting");

        panelRegistry[panelId] = panel;
        Debug.Log($"Registered UI panel: {panelId}");
    }

    public void UnregisterPanel(UIToolkitPanel panel)
    {
        if (panel == null) return;

        var panelId = panel.name;
        if (panelRegistry.ContainsKey(panelId))
        {
            panelRegistry.Remove(panelId);
            Debug.Log($"Unregistered UI panel: {panelId}");
        }
    }

    public T GetPanel<T>(string panelId) where T : UIToolkitPanel
    {
        if (panelRegistry.TryGetValue(panelId, out var panel)) return panel as T;
        return null;
    }

    public void ShowPanel(string panelId, bool addToStack = true)
    {
        if (panelRegistry.TryGetValue(panelId, out var panel))
        {
            panel.ShowPanel();

            if (addToStack && !panelStack.Contains(panel)) panelStack.Push(panel);
        }
        else
        {
            Debug.LogWarning($"Panel {panelId} not found in registry");
        }
    }

    public void HidePanel(string panelId, bool removeFromStack = true)
    {
        if (panelRegistry.TryGetValue(panelId, out var panel))
        {
            panel.HidePanel();

            if (removeFromStack && panelStack.Contains(panel))
            {
                var tempStack = new Stack<UIToolkitPanel>();
                while (panelStack.Count > 0)
                {
                    var p = panelStack.Pop();
                    if (p != panel)
                        tempStack.Push(p);
                }

                while (tempStack.Count > 0) panelStack.Push(tempStack.Pop());
            }
        }
    }

    public void TogglePanel(string panelId)
    {
        if (panelRegistry.TryGetValue(panelId, out var panel))
        {
            if (panel.IsVisible)
                HidePanel(panelId);
            else
                ShowPanel(panelId);
        }
    }

    public void CloseTopPanel()
    {
        if (panelStack.Count > 0)
        {
            var topPanel = panelStack.Pop();
            topPanel.HidePanel();
        }
    }

    public void CloseAllPanels()
    {
        foreach (var panel in panelRegistry.Values) panel.HidePanel();
        panelStack.Clear();
    }

    private void ToggleDebugUI()
    {
        TogglePanel("DebugPanel");
    }

    public void EnableDebugMode(bool enable)
    {
        enableDebugUI = enable;
    }

    // Utility methods for common UI operations
    public void SetGlobalTheme(string themePath)
    {
        var themeAsset = Resources.Load<ThemeStyleSheet>(themePath);
        if (themeAsset != null)
            foreach (var doc in uiDocuments)
                if (doc.rootVisualElement != null)
                {
                    // Apply theme to all documents
                    // Note: Implementation depends on how themes are structured
                }
    }

    public List<string> GetRegisteredPanels()
    {
        return panelRegistry.Keys.ToList();
    }

    public int GetActivePanelCount()
    {
        return panelRegistry.Values.Count(p => p.IsVisible);
    }
}