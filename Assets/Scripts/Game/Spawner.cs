using UnityEngine;
using System.Collections.Generic;

/// Spawns falling shapes at random X positions along the top of the screen.
/// Configured by StageConfig through GameController (interval, colors, allowed shapes).
public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Shape prefabs (square, triangle, circle). Assign all here.")]
    public List<Shape> shapePrefabs;

    [Header("Spawn Area (auto or manual)")]
    public float xMin = -3.0f, xMax = 3.0f;
    [Tooltip("Recalculate bounds automatically from the camera each frame.")]
    public bool autoFromCamera = true;
    public Camera cam;
    [Tooltip("Margin from left/right screen edges (world units).")]
    public float sideMargin = 0.3f;
    [Tooltip("How far above the top of the screen shapes spawn (world units).")]
    public float topOffset = 0.6f;

    [Header("Stage Tuning (overridden by GameController)")]
    public float spawnInterval = 1.2f;
    public int maxConcurrent = 30;
    [Range(3, 7)] public int colorsToUse = 7;
    public float baseGravityScale = 0.8f;

    [Header("Allowed Shapes (set by StageConfig)")]
    public bool allowSquare = true;
    public bool allowTriangle = true;
    public bool allowCircle = true;

    float timer;
    GameController game;

    void Awake()
    {
        game = FindFirstObjectByType<GameController>();
        if (!cam) cam = Camera.main;
        ApplyAutoFromCamera();
    }

    void OnEnable() => ApplyAutoFromCamera();

    void OnValidate()
    {
        if (xMax < xMin) xMax = xMin;
        ApplyAutoFromCamera();
    }

    void Update()
    {
        if (autoFromCamera) ApplyAutoFromCamera();
        if (!game || !game.IsRunning) return;

        // Timer-controlled spawning
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            if (game.ActiveShapeCount < maxConcurrent)
                SpawnOne();
        }
    }

    /// Spawn a single random shape from the allowed pool.
    void SpawnOne()
    {
        if (shapePrefabs == null || shapePrefabs.Count == 0) return;

        // Build allowed pool based on StageConfig flags
        List<Shape> pool = new List<Shape>(shapePrefabs.Count);
        foreach (var p in shapePrefabs)
        {
            if (!p) continue;
            switch (p.shapeType)
            {
                case ShapeType.Square:   if (allowSquare)   pool.Add(p); break;
                case ShapeType.Triangle: if (allowTriangle) pool.Add(p); break;
                case ShapeType.Circle:   if (allowCircle)   pool.Add(p); break;
            }
        }

        // Fallback: if misconfigured, just spawn from the full list
        if (pool.Count == 0) pool.AddRange(shapePrefabs);

        var prefab = pool[Random.Range(0, pool.Count)];
        var pos = new Vector3(Random.Range(xMin, xMax), transform.position.y, 0);

        int c = Mathf.Clamp(colorsToUse, 3, 7);
        RainbowColor rc = (RainbowColor)Random.Range(0, c);

        var s = Instantiate(prefab, pos, Quaternion.identity);
        s.Configure(prefab.shapeType, rc, baseGravityScale);

        if (game != null) game.RegisterShape(s);
    }

    /// Adjust spawn X range and Y position from the camera’s visible bounds.
    void ApplyAutoFromCamera()
    {
        if (!autoFromCamera) return;
        if (!cam) cam = Camera.main;
        if (!cam || !cam.orthographic) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth  = halfHeight * cam.aspect;

        xMin = -halfWidth + sideMargin;
        xMax =  halfWidth - sideMargin;

        var p = transform.position;
        p.y = halfHeight + topOffset;
        p.x = 0f;
        transform.position = p;
    }

    /// Visualize spawn line in Scene view.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 a = new Vector3(xMin, transform.position.y, 0f);
        Vector3 b = new Vector3(xMax, transform.position.y, 0f);
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, 0.05f);
        Gizmos.DrawSphere(b, 0.05f);
    }
}
