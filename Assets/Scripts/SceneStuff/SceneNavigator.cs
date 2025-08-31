using UnityEngine;
using UnityEngine.SceneManagement;

/// Central place to handle scene navigation.
/// Keeps all scene names in one file so you don’t mistype them elsewhere.
public static class SceneNavigator
{
    // Scene name constants (must match exactly what’s in Build Settings).
    public const string MainMenu   = "MainMenu";
    public const string StageSelect = "StageSelect";
    public const string Game       = "Game";
    public const string Result     = "Result";

    /// Loads the given scene immediately.
    /// Later, you could extend this with fades, transitions, or loading screens.
    public static void Go(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}