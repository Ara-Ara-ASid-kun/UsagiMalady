using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// Different emotional states the background character can show.
public enum Mood { Innocent, Happy, Crying }

/// Handles showing and updating the background character (Usagi).
/// Controls sprite swapping, idle animation, and mood changes based on game pace.
public class BackgroundCharacter : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("The UI Image component that displays the character.")]
    public Image image;

    [Header("Sprites")]
    [Tooltip("Neutral / innocent face.")]
    public Sprite innocent;

    [Tooltip("Optional alternate frame for neutral mood (for subtle idle swapping).")]
    public Sprite innocentAlt;

    [Tooltip("Happy expression.")]
    public Sprite happy;

    [Tooltip("Crying expression.")]
    public Sprite crying;

    [Header("Idle Swap (only while Innocent)")]
    [Tooltip("Whether to occasionally swap between innocent/innocentAlt.")]
    public bool enableIdleSwap = true;

    [Tooltip("How many seconds between idle sprite swaps.")]
    public float swapEverySeconds = 3.5f;

    private Mood currentMood = Mood.Innocent;
    private Coroutine idleLoop;
    private float stageDuration = 60f; // provided by Begin()

    /// Call once when a stage starts. Resets to Innocent mood.
    public void Begin(float stageDurationSeconds)
    {
        stageDuration = Mathf.Max(1f, stageDurationSeconds);
        SetMood(Mood.Innocent);
    }

    /// Sets the character’s mood explicitly.
    public void SetMood(Mood mood)
    {
        currentMood = mood;

        // Stop idle swapping whenever mood changes
        if (idleLoop != null)
        {
            StopCoroutine(idleLoop);
            idleLoop = null;
        }

        // Update sprite based on new mood
        switch (mood)
        {
            case Mood.Happy:
                SetSprite(happy);
                break;

            case Mood.Crying:
                SetSprite(crying);
                break;

            default:
                SetSprite(innocent);
                if (enableIdleSwap && innocentAlt != null)
                    idleLoop = StartCoroutine(IdleSwapLoop());
                break;
        }
    }

    /// Called by GameController during gameplay.
    /// Compares score vs. expected pace and adjusts mood dynamically.
    public void UpdateMoodByPace(int score, float timeLeft, int targetScore, float slack = 0.10f)
    {
        if (targetScore <= 0) return;

        float elapsed       = Mathf.Clamp(stageDuration - timeLeft, 0f, stageDuration);
        float fracElapsed   = elapsed / stageDuration;            // 0 → 1 over time
        float fracAchieved  = Mathf.Clamp01((float)score / targetScore);

        // If ahead of pace → happy; if behind pace → crying; otherwise neutral
        if (fracAchieved >= fracElapsed + slack)
        {
            if (currentMood != Mood.Happy) SetMood(Mood.Happy);
        }
        else if (fracAchieved <= fracElapsed - slack)
        {
            if (currentMood != Mood.Crying) SetMood(Mood.Crying);
        }
        else
        {
            if (currentMood != Mood.Innocent) SetMood(Mood.Innocent);
        }
    }

    /// Swap the displayed sprite.
    private void SetSprite(Sprite s)
    {
        if (image) image.sprite = s;
    }

    /// Coroutine for swapping between innocent and innocentAlt for subtle idle animation.
    private IEnumerator IdleSwapLoop()
    {
        bool useAlt = false;

        while (true)
        {
            yield return new WaitForSeconds(swapEverySeconds);

            if (currentMood != Mood.Innocent)
                yield break; // stop if mood changed

            useAlt = !useAlt;
            SetSprite(useAlt ? innocentAlt : innocent);
        }
    }
}
