using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectController : MonoBehaviour
{
    [Header("Wiring")]
    public StageDatabase stageDatabase;     // list of stages to build the grid from
    public RectTransform contentRoot;       // ScrollView -> Viewport -> Content
    public Button stageButtonTemplate;      // disabled template in scene (instantiated per stage)
    public Button backButton;               // returns to main menu

    [Header("Stage Icons")]
    public Sprite thumbsUpSprite;           // shown if best score >= target
    public Sprite exclamationSprite;        // shown otherwise
    [Tooltip("Child name of the icon Image inside each button (leave blank to auto-detect).")]
    public string iconChildName = "Icon";

    void Awake()
    {
        int count = stageDatabase ? stageDatabase.Count : 1;
        SaveManager.LoadOrCreate(count);

        BuildList();

        if (backButton)
            backButton.onClick.AddListener(() => SceneNavigator.Go(SceneNavigator.MainMenu));
    }

    void OnDestroy()
    {
        if (backButton)
            backButton.onClick.RemoveAllListeners();
    }

    // Rebuilds the stage list based on current save data and stage definitions.
    void BuildList()
    {
        if (!contentRoot || !stageButtonTemplate)
        {
            Debug.LogWarning("[StageSelect] Missing contentRoot or stageButtonTemplate.");
            return;
        }

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        int total = stageDatabase ? stageDatabase.Count : 0;
        if (total <= 0) return;

        // Highest unlocked index = unlockedStageCount - 1 (at least stage 0)
        int highest = Mathf.Max(0, SaveManager.Data.unlockedStageCount - 1);

        for (int i = 0; i < total; i++)
        {
            var btn = Instantiate(stageButtonTemplate, contentRoot);
            btn.gameObject.SetActive(true);

            // Button label → "Stage X-Y" or whatever displayName is
            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label) label.text = stageDatabase.Get(i).displayName;

            // Interactable only if unlocked
            bool unlocked = i <= highest;
            btn.interactable = unlocked;

            // Compute completion: compare best score vs stage target
            int best = (SaveManager.Data.highScores != null && i < SaveManager.Data.highScores.Length)
                     ? SaveManager.Data.highScores[i]
                     : 0;
            int target = stageDatabase.Get(i).targetScore;

            // If we just returned from this stage and won, reflect it immediately (before disk save reload)
            if (GameSession.StageIndex == i && GameSession.DidWin)
                best = Mathf.Max(best, GameSession.FinalScore);

            // Pick icon based on completion
            var icon = FindIconImage(btn);
            if (icon)
            {
                icon.sprite = (best >= target && thumbsUpSprite) ? thumbsUpSprite : exclamationSprite;
                icon.preserveAspect = true;
                // Optional tint reset in case template had a color
                icon.color = Color.white;
            }

            // Hook up click to select this stage
            int captured = i;
            btn.onClick.AddListener(() => OnPickStage(captured));

            // Visually dim locked items
            var cg = btn.GetComponent<CanvasGroup>() ?? btn.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = unlocked ? 1f : 0.5f;
        }
    }

    // Tries to find a child Image used as the small left icon.
    // 1) If iconChildName is set, use that. 2) Otherwise first child Image that isn't the button's own background.
    Image FindIconImage(Button btn)
    {
        if (!btn) return null;

        if (!string.IsNullOrEmpty(iconChildName))
        {
            var t = btn.transform.Find(iconChildName);
            if (t)
            {
                var img = t.GetComponent<Image>();
                if (img) return img;
            }
        }

        var images = btn.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            // Skip the button's targetGraphic (its background)
            if (btn.image && img == btn.image) continue;
            return img;
        }
        return null;
    }

    // Saves selection and moves into gameplay. GameController reads SaveManager.Data.lastStageIndex.
    void OnPickStage(int stageIndex)
    {
        if (SaveManager.Data != null)
        {
            SaveManager.Data.lastStageIndex = stageIndex;
            SaveManager.Save();
        }
        SceneNavigator.Go(SceneNavigator.Game);
    }
}
