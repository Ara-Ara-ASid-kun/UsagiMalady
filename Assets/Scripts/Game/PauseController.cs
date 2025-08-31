using UnityEngine;
using UnityEngine.UI;

/// Handles the pause button in the HUD and wires up the PausePopup controls.
/// Pauses gameplay via GameController and manages scene navigation.
[DisallowMultipleComponent]
public class PauseController : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("Reference to the active GameController in this scene.")]
    public GameController game;

    [Tooltip("Reference to the PausePopup panel (Continue / Retry / Back).")]
    public PausePopup pauseMenu;

    [Tooltip("The top-right pause button in the HUD.")]
    public Button pauseButton;

    void Awake()
    {
        // Fallback if not wired in the Inspector
        if (!game) game = FindFirstObjectByType<GameController>();

        // Ensure popup starts hidden
        if (pauseMenu) pauseMenu.HideImmediate();

        // Hook up pause button
        if (pauseButton) pauseButton.onClick.AddListener(OnPausePressed);

        // Hook up popup buttons
        if (pauseMenu)
        {
            if (pauseMenu.btnContinue) pauseMenu.btnContinue.onClick.AddListener(OnContinue);
            if (pauseMenu.btnRetry)    pauseMenu.btnRetry.onClick.AddListener(OnRetry);
            if (pauseMenu.btnBack)     pauseMenu.btnBack.onClick.AddListener(OnBack);
        }
    }

    void OnDestroy()
    {
        // Clean up listeners to avoid duplicate wiring after reloads
        if (pauseButton) pauseButton.onClick.RemoveListener(OnPausePressed);

        if (pauseMenu)
        {
            if (pauseMenu.btnContinue) pauseMenu.btnContinue.onClick.RemoveListener(OnContinue);
            if (pauseMenu.btnRetry)    pauseMenu.btnRetry.onClick.RemoveListener(OnRetry);
            if (pauseMenu.btnBack)     pauseMenu.btnBack.onClick.RemoveListener(OnBack);
        }
    }

    void OnPausePressed()
    {
        if (!game) return;
        game.TogglePause(); // pauses gameplay + shows popup
    }

    void OnContinue()
    {
        if (!game) return;
        game.ResumeGame();  // hides popup + resumes gameplay + restores music
    }

    void OnRetry()
    {
        if (!game) return;

        // Unpause before changing scenes (prevents stuck timeScale = 0)
        game.ResumeGame();
        SceneNavigator.Go(SceneNavigator.Game); 
    }

    void OnBack()
    {
        if (!game) return;

        // Same: ensure timeScale is restored before navigating away
        game.ResumeGame();
        SceneNavigator.Go(SceneNavigator.StageSelect);
    }
}
