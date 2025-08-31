using UnityEngine;
using System.Collections;

/// Centralized audio manager for music and sound effects.
/// Spawns a persistent hidden GameObject (~AudioManager) the first time it is accessed.
public class AudioManager : MonoBehaviour
{
    static AudioManager _inst;

    /// Singleton-style access. Ensures there is always one AudioManager alive.
    public static AudioManager I
    {
        get
        {
            if (_inst == null)
            {
                var go = new GameObject("~AudioManager");
                _inst = go.AddComponent<AudioManager>();
                DontDestroyOnLoad(go);
                _inst.EnsureSources();
            }
            return _inst;
        }
    }

    AudioSource musicSrc;
    AudioSource sfxSrc;

    void Awake()
    {
        // Destroy duplicates if one already exists
        if (_inst != null && _inst != this) { Destroy(gameObject); return; }

        _inst = this;
        DontDestroyOnLoad(gameObject);
        EnsureSources();
    }

    // Make sure both AudioSources exist and are configured
    void EnsureSources()
    {
        if (!musicSrc)
        {
            musicSrc = gameObject.AddComponent<AudioSource>();
            musicSrc.playOnAwake = false;
            musicSrc.loop = true;
            musicSrc.spatialBlend = 0f;
            musicSrc.volume = 0.8f;
        }

        if (!sfxSrc)
        {
            sfxSrc = gameObject.AddComponent<AudioSource>();
            sfxSrc.playOnAwake = false;
            sfxSrc.loop = false;
            sfxSrc.spatialBlend = 0f;
            sfxSrc.volume = 1f;
        }
    }

    // --- Music volume control ---
    public static float CurrentMusicVolume => I.musicSrc ? I.musicSrc.volume : 0f;

    public static void SetMusicVolume(float v)
    {
        if (I.musicSrc) I.musicSrc.volume = Mathf.Clamp01(v);
    }

    // Smoothly lower or restore music volume (works even when Time.timeScale = 0)
    public static void DuckMusic(bool duck, float duckTo = 0.35f, float fade = 0.2f, float? restoreVol = null)
    {
        var am = I; am.EnsureSources();
        float from = am.musicSrc.volume;
        float to   = duck ? duckTo : (restoreVol ?? 0.8f);

        am.StopAllCoroutines();
        am.StartCoroutine(am.FadeVolume(am.musicSrc, from, to, fade));
    }

    // --- Music playback ---
    public static void PlayMusic(AudioClip clip, float volume = 0.8f)
    {
        if (!clip) return;

        var am = I; am.EnsureSources();

        // If same track is already playing, just adjust volume
        if (am.musicSrc.clip == clip && am.musicSrc.isPlaying)
        {
            am.musicSrc.volume = volume;
            return;
        }

        am.musicSrc.clip = clip;
        am.musicSrc.volume = volume;
        am.musicSrc.loop = true;
        am.musicSrc.Play();
    }

    public static void PlayMusicIfDifferent(AudioClip clip, float volume = 0.8f)
    {
        if (!clip) return;

        var am = I; am.EnsureSources();
        if (am.musicSrc.clip == clip && am.musicSrc.isPlaying)
        {
            am.musicSrc.volume = volume;
            return;
        }

        PlayMusic(clip, volume);
    }

    public static void StopMusic()
    {
        if (I.musicSrc) I.musicSrc.Stop();
    }

    // --- Sound effects ---
    public static void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;
        var am = I; am.EnsureSources();
        am.sfxSrc.PlayOneShot(clip, volume);
    }

    // --- Music fade helper ---
    // Cross-fades to a new music clip over fadeOut/fadeIn seconds.
    // Uses unscaled time so it works during pauses.
    public static IEnumerator FadeToMusic(AudioClip clip, float fadeOut = 0.3f, float fadeIn = 0.3f, float targetVol = 0.8f)
    {
        var am = I; am.EnsureSources();

        // If same track is already playing, just fade volume to target
        if (am.musicSrc.clip == clip && am.musicSrc.isPlaying)
        {
            yield return am.FadeVolume(am.musicSrc, am.musicSrc.volume, targetVol, fadeIn);
            yield break;
        }

        // Fade out old track
        yield return am.FadeVolume(am.musicSrc, am.musicSrc.volume, 0f, fadeOut);

        // Switch to new track
        am.musicSrc.Stop();
        am.musicSrc.clip = clip;
        am.musicSrc.volume = 0f;
        am.musicSrc.loop = true;

        if (clip) am.musicSrc.Play();

        // Fade in new track
        yield return am.FadeVolume(am.musicSrc, 0f, targetVol, fadeIn);
    }

    // --- Internal coroutine to fade a volume over time ---
    IEnumerator FadeVolume(AudioSource src, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(from, to, dur <= 0f ? 1f : t / dur);
            yield return null;
        }
        src.volume = to;
    }
}
