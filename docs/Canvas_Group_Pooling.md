# Canvas Group Pooling Enhancement

## Overview

The Canvas Group Pooling enhancement extends the existing UI Pooling system to properly handle Unity's CanvasGroup components. It ensures that pooled UI elements maintain correct alpha, interaction, and raycast states when reused.

## Key Benefits

- **Proper UI State Reset**: CanvasGroup properties are correctly reset between uses
- **Interaction Management**: Ensures proper interactable and raycast states
- **Visual Consistency**: Maintains correct alpha transparency across UI transitions
- **Seamless Integration**: Extends existing UIPoolManager without breaking changes

## Background

CanvasGroup components control several important UI properties:
- **Alpha**: Transparency/opacity of the UI element and children
- **Interactable**: Whether the UI element responds to user input
- **Blocks Raycasts**: Whether the UI element blocks raycast events

When UI elements are pooled and reused, these properties must be reset to default values to ensure consistent behavior.

## API Changes

### Enhanced ResetObjectState Method

The `UIPoolManager.ResetObjectState()` method now includes CanvasGroup reset logic:

```csharp
private void ResetObjectState(GameObject obj)
{
    // ... existing reset logic ...

    // Reset Canvas Group
    CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
    if (canvasGroup != null)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // ... rest of reset logic ...
}
```

## Default Reset Values

| Property | Default Value | Purpose |
|----------|---------------|---------|
| `alpha` | `1.0f` | Fully opaque UI elements |
| `interactable` | `true` | UI elements respond to input |
| `blocksRaycasts` | `true` | UI elements participate in raycast events |

## Usage Examples

### Panel Transitions
```csharp
using NightBlade.UI.Utils.Pooling;

public class PanelManager
{
    private const string PANEL_POOL_KEY = "UIPanel";

    public void ShowPanel()
    {
        GameObject panel = UIPoolManager.Instance.GetObject(PANEL_POOL_KEY);

        // CanvasGroup is automatically reset to:
        // alpha = 1.0f, interactable = true, blocksRaycasts = true
        panel.SetActive(true);
    }

    public void HidePanel(GameObject panel)
    {
        // Animate out with CanvasGroup
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        StartCoroutine(FadeOut(panel, canvasGroup));
    }

    private System.Collections.IEnumerator FadeOut(GameObject panel, CanvasGroup canvasGroup)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        panel.SetActive(false);

        // Return to pool - CanvasGroup will be reset when reused
        UIPoolManager.Instance.ReturnObject(PANEL_POOL_KEY, panel);
    }
}
```

### Modal Dialogs
```csharp
public class ModalDialogSystem
{
    private const string MODAL_POOL_KEY = "ModalDialog";

    public void ShowConfirmDialog(string message, Action onConfirm, Action onCancel)
    {
        GameObject modal = UIPoolManager.Instance.GetObject(MODAL_POOL_KEY);

        // CanvasGroup automatically reset - modal is fully visible and interactive
        CanvasGroup canvasGroup = modal.GetComponent<CanvasGroup>();

        // Set up dialog content and event handlers
        SetupDialogContent(modal, message, onConfirm, onCancel);

        // Animate in
        StartCoroutine(AnimateModalIn(modal, canvasGroup));
    }

    private System.Collections.IEnumerator AnimateModalIn(GameObject modal, CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        modal.SetActive(true);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}
```

### Tooltip System
```csharp
public class TooltipSystem
{
    private const string TOOLTIP_POOL_KEY = "Tooltip";

    public void ShowTooltip(string text, Vector2 position)
    {
        GameObject tooltip = UIPoolManager.Instance.GetObject(TOOLTIP_POOL_KEY);

        // CanvasGroup automatically reset to visible state
        CanvasGroup canvasGroup = tooltip.GetComponent<CanvasGroup>();

        // Position and setup tooltip
        RectTransform rectTransform = tooltip.GetComponent<RectTransform>();
        rectTransform.position = position;

        TextMeshProUGUI textComponent = tooltip.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = text;

        // Fade in animation
        StartCoroutine(FadeInTooltip(tooltip, canvasGroup));
    }

    private System.Collections.IEnumerator FadeInTooltip(GameObject tooltip, CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        tooltip.SetActive(true);

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Auto-hide after delay
        yield return new WaitForSeconds(3f);
        StartCoroutine(FadeOutTooltip(tooltip, canvasGroup));
    }

    private System.Collections.IEnumerator FadeOutTooltip(GameObject tooltip, CanvasGroup canvasGroup)
    {
        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        tooltip.SetActive(false);

        // Return to pool - CanvasGroup will be reset for next use
        UIPoolManager.Instance.ReturnObject(TOOLTIP_POOL_KEY, tooltip);
    }
}
```

### Loading Screens
```csharp
public class LoadingScreenManager
{
    private const string LOADING_POOL_KEY = "LoadingScreen";
    private GameObject _currentLoadingScreen;

    public void ShowLoadingScreen(string message = "Loading...")
    {
        if (_currentLoadingScreen != null)
        {
            return; // Already showing
        }

        _currentLoadingScreen = UIPoolManager.Instance.GetObject(LOADING_POOL_KEY);

        // CanvasGroup automatically reset to interactive state
        CanvasGroup canvasGroup = _currentLoadingScreen.GetComponent<CanvasGroup>();

        // Set loading message
        TextMeshProUGUI textComponent = _currentLoadingScreen.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = message;

        // Animate in
        StartCoroutine(AnimateLoadingIn(_currentLoadingScreen, canvasGroup));
    }

    public void HideLoadingScreen()
    {
        if (_currentLoadingScreen == null)
        {
            return;
        }

        CanvasGroup canvasGroup = _currentLoadingScreen.GetComponent<CanvasGroup>();
        StartCoroutine(AnimateLoadingOut(_currentLoadingScreen, canvasGroup));
    }

    private System.Collections.IEnumerator AnimateLoadingIn(GameObject loading, CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;
        loading.SetActive(true);

        // Disable interaction during animation
        canvasGroup.interactable = false;

        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true; // Re-enable interaction
    }

    private System.Collections.IEnumerator AnimateLoadingOut(GameObject loading, CanvasGroup canvasGroup)
    {
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        loading.SetActive(false);

        // Return to pool - CanvasGroup will be reset to default state
        UIPoolManager.Instance.ReturnObject(LOADING_POOL_KEY, loading);
        _currentLoadingScreen = null;
    }
}
```

## Advanced Usage Patterns

### Complex UI State Management
```csharp
public class UIStateManager
{
    public enum UIState
    {
        Hidden,     // alpha = 0, interactable = false, blocksRaycasts = false
        Disabled,   // alpha = 0.5, interactable = false, blocksRaycasts = true
        Visible,    // alpha = 1, interactable = true, blocksRaycasts = true
        Modal       // alpha = 1, interactable = true, blocksRaycasts = false (blocks everything behind)
    }

    public void SetUIState(GameObject uiElement, UIState state)
    {
        CanvasGroup canvasGroup = uiElement.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = uiElement.AddComponent<CanvasGroup>();
        }

        switch (state)
        {
            case UIState.Hidden:
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                break;

            case UIState.Disabled:
                canvasGroup.alpha = 0.5f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = true;
                break;

            case UIState.Visible:
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                break;

            case UIState.Modal:
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = false; // Let modal block everything behind
                break;
        }
    }
}
```

### Group-Based Canvas Control
```csharp
public class CanvasGroupController
{
    public void FadeGroup(CanvasGroup[] groups, float targetAlpha, float duration, Action onComplete = null)
    {
        StartCoroutine(FadeGroupCoroutine(groups, targetAlpha, duration, onComplete));
    }

    private System.Collections.IEnumerator FadeGroupCoroutine(CanvasGroup[] groups, float targetAlpha, float duration, Action onComplete)
    {
        float[] startAlphas = new float[groups.Length];
        for (int i = 0; i < groups.Length; i++)
        {
            startAlphas[i] = groups[i].alpha;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < groups.Length; i++)
            {
                groups[i].alpha = Mathf.Lerp(startAlphas[i], targetAlpha, t);
            }

            yield return null;
        }

        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].alpha = targetAlpha;
        }

        onComplete?.Invoke();
    }
}
```

## Integration with Performance Monitor

Canvas Group pooling integrates with the existing UI pooling statistics:

```csharp
// View UI pool statistics (includes CanvasGroup reset behavior)
int uiPoolSize = PerformanceMonitor.Instance.GetStats().UIPoolSize;
string poolDetails = PerformanceMonitor.Instance.GetUIPoolStats();

// Performance profiling
PerformanceMonitor.ProfileUIRender(() => {
    // Your UI operations with CanvasGroup
    var panel = UIPoolManager.Instance.GetObject("Panel");
    var canvasGroup = panel.GetComponent<CanvasGroup>();
    // CanvasGroup is automatically reset
});
```

## Best Practices

1. **Rely on Automatic Reset**: Let the pooling system handle CanvasGroup reset
2. **Manual State Changes**: Only modify CanvasGroup properties when needed for animations
3. **Consistent Default State**: Design UI elements to work with the default reset values
4. **Animation Cleanup**: Ensure animations complete before returning objects to pool
5. **State Preservation**: Store custom states separately from pooled objects

## Troubleshooting

### UI Elements Not Responding
Check that CanvasGroup.interactable is true after pooling reset.

### Incorrect Transparency
Verify that CanvasGroup.alpha is reset to 1.0f. Custom animations may leave it at different values.

### Raycast Issues
Ensure CanvasGroup.blocksRaycasts is appropriate for your UI element type.

## Migration Guide

### Before (Manual CanvasGroup Management)
```csharp
public GameObject GetPanel()
{
    GameObject panel = Instantiate(panelPrefab);

    // Manual CanvasGroup setup - error prone
    CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
    if (canvasGroup != null)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    return panel;
}
```

### After (Automatic Reset)
```csharp
public GameObject GetPanel()
{
    GameObject panel = UIPoolManager.Instance.GetObject("Panel");

    // CanvasGroup automatically reset to correct defaults
    // No manual setup required!

    return panel;
}
```

## Unity Integration Notes

- **Component Requirements**: Works with any GameObject that has a CanvasGroup component
- **Hierarchy Support**: Affects the GameObject and all its children
- **Animation Compatibility**: Works with Unity's animation system and tweening libraries
- **UI System Integration**: Fully compatible with Unity's UI system

## Performance Impact

- **Setup Time**: Eliminates manual CanvasGroup initialization
- **Consistency**: Ensures reliable UI state across all pooled elements
- **Animation Performance**: No impact on animation systems
- **Memory Efficiency**: No additional memory overhead

## Version History

- **v4.0.0**: Initial CanvasGroup reset implementation
- **v4.0.1**: Enhanced with comprehensive state management examples
- **v4.0.2**: Added advanced UI state patterns and group controls
- **v4.0.3**: Performance optimizations and best practice documentation