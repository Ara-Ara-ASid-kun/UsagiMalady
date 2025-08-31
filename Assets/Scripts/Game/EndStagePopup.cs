using UnityEngine;
using TMPro;
using System.Collections;

/// Popup shown when a stage ends. Displays a success/fail message with a short fade-in/out.
public class EndStagePopup : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("CanvasGroup controlling visibility and interactivity.")]
    public CanvasGroup group;

    [Tooltip("Text element that displays the message.")]
    public TMP_Text txtMessage;

    [Header("Messages")]
    [Tooltip("Message shown when the player meets the target.")]
    public string successText = "Target Reached!";

    [Tooltip("Message shown when the player fails.")]
    public string failText = "Aww, Challenge Failed!";

    [Header("Timing")]
    [Tooltip("Seconds to fade in the popup.")]
    public float fadeIn = 0.2f;

    [Tooltip("Seconds the popup remains fully visible.")]
    public float hold = 2.3f;

    [Tooltip("Seconds to fade out the popup.")]
    public float fadeOut = 0.8f;

    void Reset()
    {
        // Auto-wire components when first added
        group = GetComponent<CanvasGroup>();
        txtMessage = GetComponentInChildren<TMP_Text>();
    }

    /// Shows the popup with the correct message, then fades it out.
    /// Uses unscaled time so it works even when gameplay is paused.
    public IEnumerator Show(bool win)
    {
        gameObject.SetActive(true);

        if (txtMessage)
            txtMessage.text = win ? successText : failText;

        if (group) group.alpha = 0f;

        // Fade in
        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            if (group) group.alpha = Mathf.Lerp(0, 1, t / fadeIn);
            yield return null;
        }

        // Hold fully visible
        yield return new WaitForSecondsRealtime(hold);

        // Fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            if (group) group.alpha = Mathf.Lerp(1, 0, t / fadeOut);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
