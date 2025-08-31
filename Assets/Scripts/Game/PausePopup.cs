using UnityEngine;
using UnityEngine.UI;

/// Pause menu overlay with Continue / Retry / Back buttons.
/// Visibility is controlled by CanvasGroup and the optional child "Window" panel.
[DisallowMultipleComponent]
public class PausePopup : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("CanvasGroup on this root (controls alpha + interaction).")]
    public CanvasGroup canvasGroup;

    [Tooltip("Child panel that contains the buttons (usually called 'Window').")]
    public RectTransform window;

    [Header("Buttons")]
    public Button btnContinue;
    public Button btnRetry;
    public Button btnBack;

    void Reset()
    {
        // Auto-wire common layout when first added
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!window)
        {
            var t = transform.Find("Window");
            if (t) window = t as RectTransform;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Re-capture references if renamed in the Editor
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
    }
#endif

    /// Hides immediately (used on Awake to ensure it doesn’t flash at scene start).
    public void HideImmediate()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (window) window.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    /// Show the popup: enables root + window and makes it interactive.
    public void Show()
    {
        gameObject.SetActive(true);

        if (window && !window.gameObject.activeSelf)
            window.gameObject.SetActive(true);

        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// Hide the popup gracefully (same as HideImmediate but can be called mid-game).
    public void Hide()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (window) window.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}
