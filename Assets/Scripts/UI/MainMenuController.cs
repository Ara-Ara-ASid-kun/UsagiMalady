using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Wiring")]
    public Button btnNewGame;        // main menu "New Game"
    public Button btnContinue;       // main menu "Continue"
    public StageDatabase stageDatabase; // list of all stages (needed to size the save)

    void Awake()
    {
        // If the StageDatabase wasn't dragged in, try the common Resources path.
        if (!stageDatabase)
            stageDatabase = Resources.Load<StageDatabase>("StageDatabase");

        // Ensure a save exists and its arrays match the current number of stages.
        int stageCount = stageDatabase ? stageDatabase.Count : 1;
        SaveManager.LoadOrCreate(stageCount);

        // Wire up buttons once.
        if (btnNewGame)   btnNewGame.onClick.AddListener(OnNewGame);
        if (btnContinue)  btnContinue.onClick.AddListener(OnContinue);

        // Continue is only available after the player has started at least once.
        bool canContinue = SaveManager.Data != null && SaveManager.Data.hasStarted;
        if (btnContinue)
        {
            btnContinue.interactable = canContinue;

            // Dim the button when locked so it "looks" unavailable.
            var cg = btnContinue.GetComponent<CanvasGroup>();
            if (!cg) cg = btnContinue.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = canContinue ? 1f : 0.5f;
        }
    }

    void OnDestroy()
    {
        // Good hygiene: remove listeners to avoid duplicate subscriptions after scene reloads.
        if (btnNewGame)  btnNewGame.onClick.RemoveListener(OnNewGame);
        if (btnContinue) btnContinue.onClick.RemoveListener(OnContinue);
    }

    // Start a brand-new run: wipe progress, unlock only the first stage, and flag that we’ve begun.
    void OnNewGame()
    {
        int totalStages = stageDatabase ? stageDatabase.Count : 1;

        SaveManager.NewGame(totalStages);           // creates a fresh save and sets hasStarted = true
        SaveManager.Data.lastStageIndex = 0;        // make stage 0 the default selection
        SaveManager.Save();

        SceneNavigator.Go(SceneNavigator.StageSelect);
    }

    // Resume an existing run: head to stage select (or jump straight into the last stage if you prefer).
    void OnContinue()
    {
        SceneNavigator.Go(SceneNavigator.StageSelect);
    }
}
