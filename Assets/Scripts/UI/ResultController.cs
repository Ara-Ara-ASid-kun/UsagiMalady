using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ResultController : MonoBehaviour
{
    [Header("Stage Data")]
    public StageDatabase stageDatabase;

    [Header("Texts")]
    public TMP_Text txtHeader;
    public TMP_Text txtNumbers;
    public TMP_Text txtHighScore;

    [Header("Buttons")]
    public Button btnNext;
    public Button btnRetry;
    public Button btnBack;
    public TMP_Text btnNextLabel;

    [Header("Character")]
    public Image usagiImage;
    public Sprite usagiHappy;
    public Sprite usagiCrying;

    [Header("Navigation")]
    public bool goStraightToGameOnNext = true;

    void Awake()
    {
        // Always reset time in case we came from a paused game
        Time.timeScale = 1f;

        bool win        = GameSession.DidWin;
        int target      = GameSession.TargetScore;
        int score       = GameSession.FinalScore;
        int stageIndex  = GameSession.StageIndex;

        if (txtHeader)  txtHeader.text = "Score";
        if (txtNumbers) txtNumbers.text = $"{score:n0} / {target:n0}";
        if (usagiImage) usagiImage.sprite = win ? usagiHappy : usagiCrying;

        // Show the stored high score for this stage
        if (txtHighScore)
        {
            int best = 0;
            if (SaveManager.Data != null &&
                SaveManager.Data.highScores != null &&
                stageIndex >= 0 &&
                stageIndex < SaveManager.Data.highScores.Length)
            {
                best = SaveManager.Data.highScores[stageIndex];
            }
            txtHighScore.text = $"High Score: {best:n0}";
        }

        // Work out if the next stage should be unlocked
        int total = (stageDatabase != null) ? stageDatabase.Count : 1;
        bool hasNext    = SaveManager.HasNextStage(stageIndex, total);
        bool canGoNext  = win && hasNext;

        if (btnNext)
        {
            btnNext.interactable = canGoNext;
            if (!canGoNext && btnNextLabel != null) btnNextLabel.text = "Locked";
            btnNext.onClick.AddListener(() =>
            {
                if (!canGoNext) return;
                int next = Mathf.Min(stageIndex + 1, total - 1);
                SaveManager.Data.lastStageIndex = next;
                SaveManager.Save();

                if (goStraightToGameOnNext) SceneNavigator.Go(SceneNavigator.Game);
                else                        SceneNavigator.Go(SceneNavigator.StageSelect);
            });
        }

        if (btnRetry)
        {
            btnRetry.onClick.AddListener(() =>
            {
                SaveManager.Data.lastStageIndex = stageIndex;
                SaveManager.Save();
                SceneNavigator.Go(SceneNavigator.Game);
            });
        }

        if (btnBack)
        {
            btnBack.onClick.AddListener(() => SceneNavigator.Go(SceneNavigator.StageSelect));
        }

        // Preselect the Next button so the first click always registers
        var es = EventSystem.current;
        if (es != null && btnNext != null)
        {
            es.SetSelectedGameObject(btnNext.gameObject);
        }
    }
}
