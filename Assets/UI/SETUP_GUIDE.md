# UI Toolkit Setup Guide for Scratcher

This guide explains how to set up and use the new UI Toolkit system in your Scratcher project.

## Overview

The new UI system provides:
- **GameHUD**: Modern HUD with time, inventory, and player stats
- **DebugUI**: Comprehensive debug panel with system controls
- **MainMenu**: Professional main menu with settings
- **NotificationManager**: Toast notification system
- **DataBinding**: Reactive UI updates

## Quick Setup

### 1. Scene Setup

#### For Game Scene:
1. Create an empty GameObject named "UI System"
2. Add the `UIIntegrationManager` component
3. The system will automatically create UI documents and controllers

#### For Main Menu Scene:
1. Create an empty GameObject named "Main Menu"
2. Add `UIDocument` component
3. Assign `MainMenu.uxml` to Visual Tree Asset
4. Add `MainMenuController` component

### 2. UI Documents Setup

The system uses these UXML/USS files:
- `Assets/UI/UXML/GameHUD.uxml` + `Assets/UI/USS/MainHUD.uss`
- `Assets/UI/UXML/DebugPanel.uxml` + `Assets/UI/USS/DebugUI.uss`
- `Assets/UI/UXML/MainMenu.uxml` + `Assets/UI/USS/MainMenu.uss`

### 3. Resource Loading

For automatic loading, move UXML/USS files to:
```
Assets/Resources/UI/UXML/
Assets/Resources/UI/USS/
```

## Manual Setup (Alternative)

### GameHUD Setup:
1. Create GameObject with `UIDocument`
2. Assign `GameHUD.uxml` to Visual Tree Asset
3. Add `GameHUDController` component
4. Add to UIManager registry

### Debug Panel Setup:
1. Create GameObject with `UIDocument`
2. Assign `DebugPanel.uxml` to Visual Tree Asset
3. Add `DebugUIManager` component
4. Press F1 to toggle (default)

### Notification System:
1. Create GameObject named "NotificationManager"
2. Add `NotificationManager` component
3. Will automatically create notification area in UI

## Integration with Existing Systems

### GameManager Integration:
```csharp
// In GameManager.cs Start() method:
var uiIntegration = FindObjectOfType<UIIntegrationManager>();
if (uiIntegration != null) {
    uiIntegration.ShowGameHUD();
}
```

### FishingController Integration:
The system automatically connects with existing FishingController. To add event support:

```csharp
// Add these events to FishingController:
public System.Action OnFishingStarted;
public System.Action OnFishingEnded;
public System.Action<float> OnFishingProgressChanged;

// Call events at appropriate times:
OnFishingStarted?.Invoke();
OnFishingProgressChanged?.Invoke(progress);
OnFishingEnded?.Invoke();
```

### InventorySystem Integration:
Already integrated! The system listens to existing events:
- `OnFishAdded` - Shows catch notifications
- `OnValueChanged` - Updates HUD displays

### TimeManager Integration:
Already integrated! The system listens to:
- `OnTimeOfDayChanged` - Updates time display
- `OnDayChanged` - Shows new day notifications

## Using the Debug Panel

Press **F1** to toggle the debug panel (configurable).

### System Tab:
- Performance metrics (FPS, memory, draw calls)
- Real-time system monitoring

### Game State Tab:
- Time controls (adjust time, time scale)
- Player information
- Inventory management

### Fishing Tab:
- Water zone information
- Fish database viewer
- Catch rate debugging

### Terrain Tab:
- Terrain generation controls
- Parameter adjustment
- Live terrain modification

### Cheats Tab:
- Add fish to inventory
- Teleport player
- System toggles

## Customization

### Adding New Notifications:
```csharp
NotificationManager.Instance.ShowNotification(
    "Title",
    "Message",
    NotificationManager.NotificationType.Success
);
```

### Extending the HUD:
1. Edit `GameHUD.uxml` to add new elements
2. Update `MainHUD.uss` for styling
3. Modify `GameHUDController.cs` to bind new elements

### Adding New Debug Features:
1. Edit `DebugPanel.uxml` to add controls
2. Update `DebugUI.uss` for styling
3. Add functionality in `DebugUIManager.cs`

## Styling Guide

### USS Classes:
- `.debug-panel` - Main debug panel
- `.game-hud` - Main HUD container
- `.notification-toast` - Notification styling
- `.menu-button` - Main menu buttons

### Responsive Design:
The system includes media queries for different screen sizes:
- Desktop: Full features
- Tablet: Adjusted spacing
- Mobile: Compact layout

### Color Scheme:
- Primary: `rgba(80, 150, 220, 0.9)` (Blue)
- Success: `rgba(100, 200, 100, 0.9)` (Green)
- Warning: `rgba(200, 150, 50, 0.9)` (Orange)
- Error: `rgba(200, 80, 80, 0.9)` (Red)
- Background: `rgba(32, 32, 32, 0.85)` (Dark)

## Performance Notes

### UI Toolkit Benefits:
- Hardware-accelerated rendering
- Efficient layout with Flexbox
- Minimal GameObject overhead
- CSS-like styling workflow

### Best Practices:
- Use data binding for reactive updates
- Batch UI updates when possible
- Leverage USS animations over scripts
- Keep notification count reasonable

## Troubleshooting

### UI Not Appearing:
1. Check UIDocument has correct Visual Tree Asset
2. Verify UXML/USS files are in correct locations
3. Ensure UIDocument is active and enabled

### Debug Panel Not Working:
1. Check `enableDebugMode` is true on UIIntegrationManager
2. Verify F1 key binding
3. Ensure DebugUIManager component is present

### Styling Issues:
1. Check USS files are loaded in UIDocument
2. Verify class names match between UXML and USS
3. Use UI Debugger (Window > UI Toolkit > Debugger)

### Performance Issues:
1. Limit active notifications
2. Reduce debug panel update frequency
3. Check for memory leaks in data binding

## Migration from Legacy UI

### Automatic Migration:
The `UIIntegrationManager` can automatically disable legacy UI:
1. Set `replaceOldUI = true`
2. Legacy FishingGameUI will be disabled
3. Canvas objects named "Legacy" or "Old" will be hidden

### Manual Migration:
1. Identify legacy UI components
2. Disable/remove them manually
3. Update any direct references to legacy UI
4. Test all UI functionality

## Further Development

### Adding New Panels:
1. Create UXML layout file
2. Create USS stylesheet
3. Create controller inheriting from `UIToolkitPanel`
4. Register with UIManager

### Extending Data Binding:
Use `DataBindingHelper` for reactive UI:
```csharp
DataBindingHelper.BindText(root, "label-name", () => GetDynamicText());
DataBindingHelper.BindSlider(root, "slider-name", () => GetValue(), SetValue);
```

### Custom Animations:
Create CSS transitions in USS:
```css
.animated-element {
    transition-duration: 0.3s;
    transition-property: opacity, scale;
}
```

For more complex animations, use UIElements API in C#.