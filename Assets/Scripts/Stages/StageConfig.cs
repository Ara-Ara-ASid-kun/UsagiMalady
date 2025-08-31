using UnityEngine;

/// Configuration for a single stage.
/// Defines its identity, win condition, gameplay tuning, allowed shapes, and audio.
[CreateAssetMenu(menuName = "ClashShapes/Stage Config", fileName = "Stage_")]
public class StageConfig : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The name shown in stage select (e.g., 'Stage 1-1').")]
    public string displayName = "Stage 1";

    [Tooltip("Optional longer description, shown in menus if desired.")]
    [TextArea] public string description;

    [Header("Goal")]
    [Tooltip("The minimum score needed to clear the stage.")]
    public int targetScore = 100;

    [Header("Timing")]
    [Tooltip("Stage length in seconds. Player must reach targetScore before this runs out.")]
    public float stageDurationSeconds = 60f;

    [Header("Difficulty Settings")]
    [Tooltip("Seconds between each shape spawn.")]
    public float spawnInterval = 1.2f;

    [Tooltip("Gravity scale applied to falling shapes (multiplied by 2D gravity).")]
    public float baseGravityScale = 0.8f;

    [Tooltip("Maximum number of shapes on screen at once (performance guard).")]
    public int maxConcurrentShapes = 30;

    [Tooltip("How many colors from the rainbow are used (3–7).")]
    [Range(3, 7)] public int rainbowColorsToUse = 7;

    [Header("Allowed Shapes")]
    [Tooltip("Enable or disable squares for this stage.")]
    public bool allowSquare = true;

    [Tooltip("Enable or disable triangles for this stage.")]
    public bool allowTriangle = true;

    [Tooltip("Enable or disable circles for this stage.")]
    public bool allowCircle = true;

    [Header("Audio")]
    [Tooltip("Music track played during this stage.")]
    public AudioClip stageMusic;

    [Range(0f, 1f)]
    [Tooltip("Volume level for the music in this stage (default 0.8).")]
    public float musicVolume = 0.8f;
}