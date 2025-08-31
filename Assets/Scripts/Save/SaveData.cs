using System;

[Serializable]
public class SaveData
{
    // Which stage we last played (for Continue / default selection)
    public int lastStageIndex = 0;

    // How many stages are unlocked (1 = only stage 0 unlocked)
    public int unlockedStageCount = 1;

    // High score per stage
    public int[] highScores;

    // ✅ Track if the player has started the game before
    public bool hasStarted = false;
}