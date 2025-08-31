using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [Tooltip("Apply safe area offsets even in the Editor (useful with Device Simulator).")]
    public bool simulateInEditor = true;

    [Tooltip("Extra padding (in pixels) inside the safe area: X = top, Y = bottom.")]
    public Vector2 extraPaddingTopBottom = Vector2.zero;

    [Tooltip("Extra padding (in pixels) inside the safe area: X = left, Y = right.")]
    public Vector2 extraPaddingLeftRight = Vector2.zero;

    // Cache of the last applied safe area, so we only update when it changes
    Rect lastApplied;

    void OnEnable()
    {
        Apply();
    }

    void OnRectTransformDimensionsChange()
    {
        Apply();
    }

    void Update()
    {
        // When the game is running, keep watching for orientation or resolution changes
        if (Application.isPlaying && lastApplied != Screen.safeArea)
            Apply();
    }

    void Apply()
    {
        var rect = GetComponent<RectTransform>();
        if (!rect) return;

        // Start with the platform's reported safe area
        Rect sa = Screen.safeArea;

#if UNITY_EDITOR
        // In Editor, optionally ignore safe area so UI uses the full screen
        if (!simulateInEditor)
            sa = new Rect(0, 0, Screen.width, Screen.height);
#endif

        // Apply optional extra padding
        sa.xMin += extraPaddingLeftRight.x;
        sa.xMax -= extraPaddingLeftRight.y;
        sa.yMax -= extraPaddingTopBottom.x; // reduce from top
        sa.yMin += extraPaddingTopBottom.y; // raise from bottom

        // Clamp the safe area to screen bounds
        sa.xMin = Mathf.Max(0, sa.xMin);
        sa.yMin = Mathf.Max(0, sa.yMin);
        sa.xMax = Mathf.Min(Screen.width,  sa.xMax);
        sa.yMax = Mathf.Min(Screen.height, sa.yMax);

        // Convert from pixel rect to normalized anchors
        Vector2 min = sa.position;
        Vector2 max = sa.position + sa.size;
        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        lastApplied = Screen.safeArea;
    }
}
