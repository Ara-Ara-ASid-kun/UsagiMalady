using UnityEngine;

/// Plays a background music track automatically when this scene starts.
/// Attach this to a GameObject in any scene that needs its own music.
public class SceneMusicPlayer : MonoBehaviour
{
    [Header("Music")]
    [Tooltip("The music clip to play when this scene starts.")]
    public AudioClip music;

    [Range(0f, 1f)]
    [Tooltip("Target volume level for this track.")]
    public float volume = 0.8f;

    [Header("Fade Settings")]
    [Tooltip("If true, fades between the old and new track.")]
    public bool fade = true;

    [Tooltip("How long to fade out the previous track (seconds).")]
    public float fadeOut = 0.3f;

    [Tooltip("How long to fade in the new track (seconds).")]
    public float fadeIn = 0.3f;

    void Start()
    {
        if (!music) return;

        if (fade)
        {
            // Smooth transition into this scene’s music
            StartCoroutine(AudioManager.FadeToMusic(music, fadeOut, fadeIn, volume));
        }
        else
        {
            // Switch instantly (but only if the track is different)
            AudioManager.PlayMusicIfDifferent(music, volume);
        }
    }
}