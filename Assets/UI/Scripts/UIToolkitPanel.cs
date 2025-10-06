using UnityEngine;
using UnityEngine.UIElements;

public abstract class UIToolkitPanel : MonoBehaviour
{
    [Header("UI Document")] [SerializeField]
    protected UIDocument uiDocument;

    [SerializeField] protected string panelName;
    protected bool isInitialized;

    protected VisualElement root;

    public bool IsVisible => root != null && root.style.display == DisplayStyle.Flex;
    public bool IsInitialized => isInitialized;
    public string PanelName => string.IsNullOrEmpty(panelName) ? name : panelName;

    protected virtual void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    protected virtual void Start()
    {
        InitializePanel();
    }

    protected virtual void InitializePanel()
    {
        if (uiDocument == null)
        {
            Debug.LogError($"UIDocument not found on {gameObject.name}");
            return;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError($"Root visual element not found for {gameObject.name}");
            return;
        }

        BindUIElements();
        SetupEventHandlers();
        InitializeData();

        isInitialized = true;
        OnPanelInitialized();
    }

    protected abstract void BindUIElements();
    protected abstract void SetupEventHandlers();

    protected virtual void InitializeData()
    {
    }

    protected virtual void OnPanelInitialized()
    {
    }

    public virtual void ShowPanel()
    {
        if (root != null)
            root.style.display = DisplayStyle.Flex;
    }

    public virtual void HidePanel()
    {
        if (root != null)
            root.style.display = DisplayStyle.None;
    }

    public virtual void TogglePanel()
    {
        if (root != null)
        {
            var isVisible = root.style.display == DisplayStyle.Flex;
            if (isVisible)
                HidePanel();
            else
                ShowPanel();
        }
    }

    protected T GetElement<T>(string elementName) where T : VisualElement
    {
        return root?.Q<T>(elementName);
    }

    protected void SetElementText(string elementName, string text)
    {
        var element = GetElement<Label>(elementName);
        if (element != null)
            element.text = text;
    }

    protected void SetElementValue<T>(string elementName, T value) where T : struct
    {
        var element = root?.Q(elementName);
        if (element is INotifyValueChanged<T> valueElement)
            valueElement.value = value;
    }
}