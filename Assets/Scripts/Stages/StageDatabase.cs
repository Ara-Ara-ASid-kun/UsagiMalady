using System.Collections.Generic;
using UnityEngine;

/// Holds the list of all stages in the game.
/// This ScriptableObject is created once and referenced by menus / game logic.
[CreateAssetMenu(menuName = "ClashShapes/Stage Database", fileName = "StageDatabase")]
public class StageDatabase : ScriptableObject
{
    [Tooltip("All stage configurations, in order.")]
    public List<StageConfig> stages = new();

    /// Total number of stages in this database.
    public int Count => stages != null ? stages.Count : 0;

    /// Gets a stage safely by index. Returns null if index is invalid.
    public StageConfig Get(int index)
    {
        if (stages == null || index < 0 || index >= stages.Count)
            return null;
        return stages[index];
    }
}