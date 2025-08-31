using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// Orchestrates a single stage run:
/// - Applies StageConfig to spawner
/// - Tracks time and score
/// - Handles pause/resume and music ducking
/// - Plays clash and end-of-stage SFX
/// - Shows win/lose popup, saves progress, and navigates to Result
public class GameController : MonoBehaviour
{
    [Header("Stage")]
    public StageDatabase stageDatabase;   // source of StageConfig entries
    public TMP_Text txtScore;             // "Score: X / Target"
    public TMP_Text txtTimer;             // "Time: Y"
    public Spawner spawner;               // spawns falling shapes
    public ProjectileShooter shooter;     // spawns projectiles on tap
    public BackgroundCharacter bgChar;    // background mood character

    [Header("Scoring")]
    public int pointsPerClash = 10;       // points gained per valid match

    [Header("Gameplay SFX")]
    public AudioClip sfxClash;            // sound when a valid clash happens

    [Header("End Popup & SFX")]
    public EndStagePopup endPopup;        // short “win/lose” message
    public AudioClip winSfx;              // jingle on win
    public AudioClip loseSfx;             // jingle on fail

    [Header("Pause / Music")]
    public PausePopup pauseMenu;          // pause overlay (Continue/Retry/Back)
    [Range(0f, 1f)] public float pausedMusicVolume = 0.35f;
    [HideInInspector] public float musicVolumeOnPlay = 0.8f; // restored on resume

    // Public state for other scripts
    public bool IsRunning { get; private set; }
    public bool IsPaused  { get; private set; }
    public int ActiveShapeCount => shapes.Count;

    // Internals
    int stageIndex;
    StageConfig stage;
    float timeLeft;
    int score;
    readonly HashSet<Shape> shapes = new HashSet<Shape>();

    void Start()
    {
        // Load save and pick the stage we’re about to play
        int stageCount = stageDatabase ? stageDatabase.Count : 1;
        SaveManager.LoadOrCreate(Mathf.Max(1, stageCount));

        stageIndex = Mathf.Clamp(SaveManager.Data.lastStageIndex, 0, Mathf.Max(0, stageCount - 1));
        stage = stageDatabase ? stageDatabase.Get(stageIndex) : null;

        // Stage music (fade in at the stage's preferred volume if provided)
        musicVolumeOnPlay = stage ? Mathf.Clamp01(stage.musicVolume) : 0.5f;
        if (stage && stage.stageMusic)
            StartCoroutine(AudioManager.FadeToMusic(stage.stageMusic, 0.3f, 0.3f, musicVolumeOnPlay));

        // Apply stage tuning to the spawner (with safe fallbacks)
        if (spawner)
        {
            spawner.spawnInterval     = stage ? stage.spawnInterval       : 1.2f;
            spawner.baseGravityScale  = stage ? stage.baseGravityScale    : 0.9f;
            spawner.maxConcurrent     = stage ? stage.maxConcurrentShapes : 30;
            spawner.colorsToUse       = stage ? stage.rainbowColorsToUse  : 5;

            spawner.allowSquare       = stage ? stage.allowSquare   : true;
            spawner.allowTriangle     = stage ? stage.allowTriangle : true;
            spawner.allowCircle       = stage ? stage.allowCircle   : true;
        }

        // Timer + UI bootstrap
        timeLeft = stage ? stage.stageDurationSeconds : 60f;
        score = 0;
        UpdateUI();

        // Run loop flags
        IsRunning = true;
        IsPaused  = false;

        // Background character starts neutral and will react to pace
        if (bgChar)
        {
            float dur = stage ? stage.stageDurationSeconds : 60f;
            bgChar.Begin(dur);
        }
    }

    void Update()
    {
        if (!IsRunning || IsPaused) return;

        // Countdown
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndStage();
        }

        UpdateUI();

        // Mood pacing: ahead → happy, behind → crying, otherwise neutral
        if (bgChar && stage != null)
            bgChar.UpdateMoodByPace(score, timeLeft, stage.targetScore, 0.10f);
    }

    // --- UI ---

    void UpdateUI()
    {
        int target = stage ? stage.targetScore : 0;
        if (txtScore) txtScore.text = $"Score: {score:n0}/{target:n0}";
        if (txtTimer) txtTimer.text = $"Time: {Mathf.CeilToInt(timeLeft)}";
    }

    // --- Pause / Resume ---

    public void TogglePause()
    {
        if (!IsRunning) return;
        if (IsPaused) ResumeGame();
        else          PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        IsPaused = true;

        // Freeze gameplay; UI animations can still run on unscaled time
        Time.timeScale = 0f;

        // Duck music so it’s audible but softer while paused
        AudioManager.DuckMusic(true, pausedMusicVolume, 0.2f);

        // Show overlay
        if (pauseMenu) pauseMenu.Show();
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;
        IsPaused = false;

        // Hide overlay first so first tap after resume doesn’t hit UI
        if (pauseMenu) pauseMenu.Hide();

        Time.timeScale = 1f;

        // Restore music to the stage’s normal volume
        AudioManager.DuckMusic(false, fade: 0.2f, restoreVol: musicVolumeOnPlay);
    }

    // --- Shape lifecycle (called by Spawner/Shape) ---

    public void RegisterShape(Shape s)   { if (s) shapes.Add(s); }
    public void UnregisterShape(Shape s) { if (s) shapes.Remove(s); }

    // Called when two shapes collide; awards points and removes matched pairs
    public void ReportCollision(Shape a, Shape b)
    {
        if (!IsRunning || IsPaused) return;
        if (!a || !b) return;
        if (a.isMarkedForRemoval || b.isMarkedForRemoval) return;

        bool sameShape = a.shapeType == b.shapeType;
        bool sameColor = a.color     == b.color;

        if (sameShape || sameColor)
        {
            // Audio feedback on successful clash
            if (sfxClash) AudioManager.PlaySFX(sfxClash, 0.5f);

            score += pointsPerClash;
            UpdateUI();

            a.isMarkedForRemoval = true;
            b.isMarkedForRemoval = true;

            shapes.Remove(a);
            shapes.Remove(b);

            Destroy(a.gameObject);
            Destroy(b.gameObject);
        }
    }

    // --- Stage end flow ---

    void EndStage()
    {
        if (!IsRunning) return;
        IsRunning = false;

        int totalStages = stageDatabase ? stageDatabase.Count : 1;
        int target      = stage ? stage.targetScore : 100;
        bool win        = score >= target;

        // Unlock next stage on win
        if (win)
        {
            int nextIndex = Mathf.Min(stageIndex + 1, totalStages - 1);
            SaveManager.UnlockUpTo(nextIndex, totalStages);
        }

        // Save best score and persist
        SaveManager.RecordScore(stageIndex, score);
        SaveManager.Save();

        // Hand off to Result scene
        GameSession.StageIndex  = stageIndex;
        GameSession.TargetScore = target;
        GameSession.FinalScore  = score;
        GameSession.DidWin      = win;

        StartCoroutine(EndFlow(win));
    }

    IEnumerator EndFlow(bool win)
    {
        // Pause gameplay during the popup but keep UI animating
        float oldScale = Time.timeScale;
        Time.timeScale = 0f;

        // Tiny jingle
        if (win && winSfx)   AudioManager.PlaySFX(winSfx, 0.9f);
        if (!win && loseSfx) AudioManager.PlaySFX(loseSfx, 0.9f);

        // Show the short “Target Reached!” / “Challenge Failed!” popup
        if (endPopup != null)
            yield return StartCoroutine(endPopup.Show(win));
        else
            yield return new WaitForSecondsRealtime(1.0f);

        Time.timeScale = oldScale;

        // Move on to results
        SceneNavigator.Go(SceneNavigator.Result);
    }
}
