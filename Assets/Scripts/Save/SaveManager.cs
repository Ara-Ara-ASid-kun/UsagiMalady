using UnityEngine;
using System.IO;

/// Single-slot save system for progress and scores.
/// Keeps one JSON file in Application.persistentDataPath.
public static class SaveManager
{
    public static SaveData Data { get; private set; }

    // Where the JSON file lives
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    // Load from disk if present; otherwise create a fresh save sized to the current stage count.
    public static void LoadOrCreate(int stageCount)
    {
        // Already loaded → just make sure arrays match current stage count.
        if (Data != null)
        {
            EnsureSized(stageCount);
            return;
        }

        // Try to read existing file
        if (File.Exists(Path))
        {
            try
            {
                string json = File.ReadAllText(Path);
                Data = JsonUtility.FromJson<SaveData>(json);
            }
            catch
            {
                Debug.LogWarning("[SaveManager] Save read failed; starting fresh.");
                Data = null;
            }
        }

        // Create default save if nothing loaded
        if (Data == null)
        {
            Data = new SaveData
            {
                lastStageIndex = 0,
                unlockedStageCount = 1,
                highScores = new int[Mathf.Max(1, stageCount)],
                hasStarted = false
            };
            Save();
        }

        // Keep lengths and indices valid
        EnsureSized(stageCount);
    }

    // Ensure highScores length matches stage count; clamp indices.
    static void EnsureSized(int stageCount)
    {
        int count = Mathf.Max(1, stageCount);

        // Resize highscores while preserving any existing values
        if (Data.highScores == null || Data.highScores.Length != count)
        {
            var old = Data.highScores ?? new int[0];
            var newer = new int[count];
            int copy = Mathf.Min(old.Length, count);
            for (int i = 0; i < copy; i++) newer[i] = old[i];
            Data.highScores = newer;
        }

        // Keep unlocked count and last index inside valid range
        Data.unlockedStageCount = Mathf.Clamp(Data.unlockedStageCount, 1, count);
        Data.lastStageIndex     = Mathf.Clamp(Data.lastStageIndex, 0, count - 1);

        Save();
    }

    // Write current Data to disk
    public static void Save()
    {
        if (Data == null) return;
        string json = JsonUtility.ToJson(Data, prettyPrint: true);
        File.WriteAllText(Path, json);
    }

    // Update the stored best score for a stage (if the new score is higher)
    public static void RecordScore(int index, int score)
    {
        if (Data == null || Data.highScores == null) return;
        if (index < 0 || index >= Data.highScores.Length) return;

        if (score > Data.highScores[index])
        {
            Data.highScores[index] = score;
            Save();
        }
    }

    // Unlock all stages up to and including the given index
    public static void UnlockUpTo(int indexToUnlock, int totalStages)
    {
        if (Data == null) return;

        int want = Mathf.Clamp(indexToUnlock + 1, 1, Mathf.Max(1, totalStages));
        if (want > Data.unlockedStageCount)
        {
            Data.unlockedStageCount = want;
            Save();
        }
    }

    // Check if a stage index is currently playable
    public static bool IsUnlocked(int stageIndex)
    {
        if (Data == null) return stageIndex == 0;
        return stageIndex < Data.unlockedStageCount;
    }

    // Whether there's another stage after the current index
    public static bool HasNextStage(int currentIndex, int totalStages)
    {
        return (currentIndex + 1) < totalStages;
    }

    // Start a brand-new run: reset everything and mark that the player has begun
    public static void NewGame(int stageCount)
    {
        Data = new SaveData
        {
            lastStageIndex = 0,
            unlockedStageCount = 1,
            highScores = new int[Mathf.Max(1, stageCount)],
            hasStarted = true
        };
        Save();
    }

    // Use this if you want to flip hasStarted when they first enter gameplay (instead of NewGame)
    public static void MarkStarted()
    {
        if (Data != null && !Data.hasStarted)
        {
            Data.hasStarted = true;
            Save();
        }
    }

    // Clear all progress back to a fresh state
    public static void ResetAll(int totalStages)
    {
        Data = new SaveData
        {
            lastStageIndex = 0,
            unlockedStageCount = 1,
            highScores = new int[Mathf.Max(1, totalStages)],
            hasStarted = false
        };
        Save();
    }
}
