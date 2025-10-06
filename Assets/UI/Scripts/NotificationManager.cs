using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [Header("Notification Settings")]
    [SerializeField] private float defaultDuration = 3f;
    [SerializeField] private int maxNotifications = 5;
    [SerializeField] private bool enableSounds = true;

    [Header("Audio")]
    [SerializeField] private AudioClip notificationSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip errorSound;

    private UIDocument uiDocument;
    private VisualElement notificationArea;
    private List<NotificationElement> activeNotifications = new List<NotificationElement>();
    private AudioSource audioSource;

    public static NotificationManager Instance { get; private set; }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    private class NotificationElement
    {
        public VisualElement element;
        public float timeRemaining;
        public NotificationType type;
        public bool isRemoving;

        public NotificationElement(VisualElement element, float duration, NotificationType type)
        {
            this.element = element;
            this.timeRemaining = duration;
            this.type = type;
            this.isRemoving = false;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeNotificationManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeNotificationManager()
    {
        // Get or create UIDocument
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        // Create notification area if it doesn't exist
        if (uiDocument.rootVisualElement != null)
        {
            notificationArea = uiDocument.rootVisualElement.Q<VisualElement>("notification-area");

            if (notificationArea == null)
            {
                CreateNotificationArea();
            }
        }

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.7f;
        }
    }

    private void CreateNotificationArea()
    {
        if (uiDocument.rootVisualElement == null) return;

        notificationArea = new VisualElement();
        notificationArea.name = "notification-area";
        notificationArea.AddToClassList("notification-area");

        // Style the notification area
        notificationArea.style.position = Position.Absolute;
        notificationArea.style.top = 100;
        notificationArea.style.right = 20;
        notificationArea.style.width = 300;
        notificationArea.style.flexDirection = FlexDirection.Column;
        notificationArea.style.alignItems = Align.FlexEnd;

        uiDocument.rootVisualElement.Add(notificationArea);
    }

    private void Update()
    {
        UpdateNotifications();
    }

    private void UpdateNotifications()
    {
        for (int i = activeNotifications.Count - 1; i >= 0; i--)
        {
            var notification = activeNotifications[i];

            if (!notification.isRemoving)
            {
                notification.timeRemaining -= Time.unscaledDeltaTime;

                if (notification.timeRemaining <= 0)
                {
                    StartCoroutine(RemoveNotification(notification));
                }
            }
        }
    }

    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, float duration = -1)
    {
        if (notificationArea == null)
        {
            Debug.LogWarning("Notification area not initialized");
            return;
        }

        if (duration < 0)
            duration = defaultDuration;

        // Remove excess notifications
        while (activeNotifications.Count >= maxNotifications)
        {
            var oldest = activeNotifications[0];
            if (!oldest.isRemoving)
            {
                StartCoroutine(RemoveNotification(oldest));
            }
            else
            {
                break; // Wait for removal to complete
            }
        }

        // Create notification element
        var notificationElement = CreateNotificationElement(title, message, type);
        var notification = new NotificationElement(notificationElement, duration, type);

        // Add to area and track
        notificationArea.Add(notificationElement);
        activeNotifications.Add(notification);

        // Play sound
        PlayNotificationSound(type);

        // Animate in
        StartCoroutine(AnimateNotificationIn(notificationElement));
    }

    public void ShowFishCaughtNotification(string fishName, float weight, int value)
    {
        string title = "Fish Caught!";
        string message = $"{fishName} ({weight:F1}kg) - {value} coins";
        ShowNotification(title, message, NotificationType.Success);
    }

    public void ShowLevelUpNotification(string skill, int level)
    {
        string title = "Level Up!";
        string message = $"{skill} is now level {level}";
        ShowNotification(title, message, NotificationType.Success, 4f);
    }

    public void ShowInventoryFullNotification()
    {
        string title = "Inventory Full";
        string message = "Your inventory is full. Sell some fish to make space.";
        ShowNotification(title, message, NotificationType.Warning);
    }

    public void ShowQuestCompletedNotification(string questName, int reward)
    {
        string title = "Quest Complete!";
        string message = $"{questName} - Earned {reward} coins";
        ShowNotification(title, message, NotificationType.Success, 4f);
    }

    public void ShowTimeOfDayNotification(string timeOfDay)
    {
        string title = "Time Change";
        string message = $"It is now {timeOfDay}";
        ShowNotification(title, message, NotificationType.Info, 2f);
    }

    public void ShowSaveGameNotification(bool success)
    {
        if (success)
        {
            ShowNotification("Game Saved", "Your progress has been saved.", NotificationType.Success, 2f);
        }
        else
        {
            ShowNotification("Save Failed", "Failed to save your progress.", NotificationType.Error);
        }
    }

    private VisualElement CreateNotificationElement(string title, string message, NotificationType type)
    {
        var notification = new VisualElement();
        notification.AddToClassList("notification-toast");

        // Create content container
        var content = new VisualElement();
        content.style.flexDirection = FlexDirection.Row;
        content.style.alignItems = Align.Center;

        // Create icon
        var icon = new VisualElement();
        icon.AddToClassList("notification-icon");
        SetIconStyle(icon, type);

        // Create text container
        var textContainer = new VisualElement();
        textContainer.style.flexDirection = FlexDirection.Column;
        textContainer.style.flexGrow = 1;
        textContainer.style.marginLeft = 8;

        // Create title
        var titleLabel = new Label(title);
        titleLabel.AddToClassList("notification-title");
        titleLabel.style.color = Color.white;
        titleLabel.style.fontSize = 12;
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.marginBottom = 2;

        // Create message
        var messageLabel = new Label(message);
        messageLabel.AddToClassList("notification-message");
        messageLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        messageLabel.style.fontSize = 10;
        messageLabel.style.whiteSpace = WhiteSpace.Normal;

        // Assemble notification
        textContainer.Add(titleLabel);
        textContainer.Add(messageLabel);
        content.Add(icon);
        content.Add(textContainer);
        notification.Add(content);

        // Set notification style
        SetNotificationStyle(notification, type);

        return notification;
    }

    private void SetIconStyle(VisualElement icon, NotificationType type)
    {
        icon.style.width = 24;
        icon.style.height = 24;
        icon.style.borderTopLeftRadius = 4;
        icon.style.borderTopRightRadius = 4;
        icon.style.borderBottomLeftRadius = 4;
        icon.style.borderBottomRightRadius = 4;

        switch (type)
        {
            case NotificationType.Success:
                icon.style.backgroundColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                break;
            case NotificationType.Warning:
                icon.style.backgroundColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                break;
            case NotificationType.Error:
                icon.style.backgroundColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);
                break;
            default: // Info
                icon.style.backgroundColor = new Color(0.4f, 0.6f, 0.8f, 0.8f);
                break;
        }
    }

    private void SetNotificationStyle(VisualElement notification, NotificationType type)
    {
        // Base style
        notification.style.backgroundColor = new Color(0.125f, 0.125f, 0.125f, 0.9f);
        notification.style.borderTopLeftRadius = 6;
        notification.style.borderTopRightRadius = 6;
        notification.style.borderBottomLeftRadius = 6;
        notification.style.borderBottomRightRadius = 6;
        notification.style.paddingTop = 12;
        notification.style.paddingBottom = 12;
        notification.style.paddingLeft = 16;
        notification.style.paddingRight = 16;
        notification.style.marginBottom = 8;
        notification.style.borderLeftWidth = 4;
        notification.style.maxWidth = 280;
        notification.style.minWidth = 200;

        // Set border color based on type
        switch (type)
        {
            case NotificationType.Success:
                notification.style.borderLeftColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                break;
            case NotificationType.Warning:
                notification.style.borderLeftColor = new Color(0.8f, 0.6f, 0.2f, 0.8f);
                break;
            case NotificationType.Error:
                notification.style.borderLeftColor = new Color(0.8f, 0.3f, 0.3f, 0.8f);
                break;
            default: // Info
                notification.style.borderLeftColor = new Color(0.4f, 0.6f, 0.8f, 0.8f);
                break;
        }

        // Initial state for animation
        notification.style.opacity = 0;
        notification.style.translate = new Translate(new Length(100, LengthUnit.Percent), 0);
    }

    private void PlayNotificationSound(NotificationType type)
    {
        if (!enableSounds || audioSource == null) return;

        AudioClip soundToPlay = notificationSound;

        switch (type)
        {
            case NotificationType.Success:
                if (successSound != null) soundToPlay = successSound;
                break;
            case NotificationType.Warning:
                if (warningSound != null) soundToPlay = warningSound;
                break;
            case NotificationType.Error:
                if (errorSound != null) soundToPlay = errorSound;
                break;
        }

        if (soundToPlay != null)
        {
            audioSource.PlayOneShot(soundToPlay);
        }
    }

    private IEnumerator AnimateNotificationIn(VisualElement notification)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Ease out animation
            float easedT = 1f - (1f - t) * (1f - t);

            notification.style.opacity = easedT;
            notification.style.translate = new Translate(new Length(100 * (1f - easedT), LengthUnit.Percent), 0);

            yield return null;
        }

        // Ensure final state
        notification.style.opacity = 1;
        notification.style.translate = new Translate(0, 0);
    }

    private IEnumerator AnimateNotificationOut(VisualElement notification)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Ease in animation
            float easedT = t * t;

            notification.style.opacity = 1f - easedT;
            notification.style.translate = new Translate(new Length(100 * easedT, LengthUnit.Percent), 0);

            yield return null;
        }

        // Remove from DOM
        if (notification.parent != null)
        {
            notification.parent.Remove(notification);
        }
    }

    private IEnumerator RemoveNotification(NotificationElement notification)
    {
        if (notification.isRemoving) yield break;

        notification.isRemoving = true;

        // Animate out
        yield return StartCoroutine(AnimateNotificationOut(notification.element));

        // Remove from tracking
        activeNotifications.Remove(notification);
    }

    public void ClearAllNotifications()
    {
        foreach (var notification in activeNotifications)
        {
            if (!notification.isRemoving)
            {
                StartCoroutine(RemoveNotification(notification));
            }
        }
    }

    public void SetSoundsEnabled(bool enabled)
    {
        enableSounds = enabled;
    }

    public void SetMaxNotifications(int max)
    {
        maxNotifications = Mathf.Max(1, max);
    }

    public void SetDefaultDuration(float duration)
    {
        defaultDuration = Mathf.Max(0.5f, duration);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}