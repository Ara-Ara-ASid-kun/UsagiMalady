using UnityEngine;

/// Builds 2D “walls” that match the visible area of an orthographic camera,
/// leaving configurable margins/offsets. Put this on a parent GameObject
/// (e.g., "Bounds") and let it auto-create/size the BoxCollider2D children.
/// Updates in Play mode and in the Editor so it adapts to device aspect.
[ExecuteAlways]
public class WorldBounds2D : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Which camera defines the visible area. If empty, uses Camera.main.")]
    public Camera cam;

    [Header("Play Area (inside the walls)")]
    [Tooltip("Horizontal padding from the screen edges (world units).")]
    public float sideMargin   = 0.20f;
    [Tooltip("Shorten the top by this much (world units).")]
    public float topOffset    = 0.00f;
    [Tooltip("Raise the floor by this much (world units).")]
    public float bottomOffset = 0.00f;

    [Header("Wall Geometry")]
    [Tooltip("Thickness of the colliders (world units).")]
    public float thickness = 0.50f;

    [Header("Colliders (children)")]
    public BoxCollider2D leftWall;
    public BoxCollider2D rightWall;
    public BoxCollider2D floor;
    [Tooltip("Also create a ceiling collider (optional).")]
    public bool includeCeiling = false;
    public BoxCollider2D ceiling;

    void Reset()
    {
        cam = Camera.main;
        EnsureColliders();
        Apply();
    }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        EnsureColliders();
        Apply();
    }

    void OnEnable()
    {
        Apply();
    }

    void OnValidate()
    {
        if (!cam) cam = Camera.main;
        EnsureColliders();
        Apply();
    }

    /// Finds/creates child BoxCollider2D objects named LeftWall / RightWall / Floor / Ceiling.
    void EnsureColliders()
    {
        // Try to find existing children first
        if (!leftWall)  leftWall  = transform.Find("LeftWall")  ?.GetComponent<BoxCollider2D>();
        if (!rightWall) rightWall = transform.Find("RightWall") ?.GetComponent<BoxCollider2D>();
        if (!floor)     floor     = transform.Find("Floor")     ?.GetComponent<BoxCollider2D>();
        if (!ceiling)   ceiling   = transform.Find("Ceiling")   ?.GetComponent<BoxCollider2D>();

        // Auto-create if missing
        if (!leftWall)  leftWall  = CreateWall("LeftWall");
        if (!rightWall) rightWall = CreateWall("RightWall");
        if (!floor)     floor     = CreateWall("Floor");
        if (includeCeiling && !ceiling) ceiling = CreateWall("Ceiling");
    }

    /// Creates a new child with a BoxCollider2D configured as a solid wall.
    BoxCollider2D CreateWall(string childName)
    {
        var go = new GameObject(childName);
        go.transform.SetParent(transform, false);
        var bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = false;
        return bc;
    }

    /// Computes the inner play rectangle from the camera and sizes/positions the colliders.
    void Apply()
    {
        if (!cam || !cam.orthographic) return;
        if (!leftWall || !rightWall || !floor) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // Inner playable rect (what the walls enclose)
        float innerBottom = -halfH + bottomOffset;
        float innerTop    =  halfH - topOffset;
        float innerLeft   = -halfW + sideMargin;
        float innerRight  =  halfW - sideMargin;

        float innerWidth  = Mathf.Max(0.01f, innerRight - innerLeft);
        float innerHeight = Mathf.Max(0.01f, innerTop - innerBottom);
        float midY        = (innerTop + innerBottom) * 0.5f;

        // Floor: its top edge sits at innerBottom
        floor.transform.position = new Vector3(0f, innerBottom - thickness * 0.5f, 0f);
        floor.size   = new Vector2(innerWidth, thickness);
        floor.offset = Vector2.zero;

        // Left wall: its right edge sits at innerLeft
        leftWall.transform.position = new Vector3(innerLeft - thickness * 0.5f, midY, 0f);
        leftWall.size   = new Vector2(thickness, innerHeight + thickness); // small pad meets floor
        leftWall.offset = Vector2.zero;

        // Right wall: its left edge sits at innerRight
        rightWall.transform.position = new Vector3(innerRight + thickness * 0.5f, midY, 0f);
        rightWall.size   = new Vector2(thickness, innerHeight + thickness);
        rightWall.offset = Vector2.zero;

        // Ceiling (optional): its bottom edge sits at innerTop
        if (includeCeiling)
        {
            if (!ceiling) ceiling = CreateWall("Ceiling");
            ceiling.transform.position = new Vector3(0f, innerTop + thickness * 0.5f, 0f);
            ceiling.size   = new Vector2(innerWidth, thickness);
            ceiling.offset = Vector2.zero;
            ceiling.gameObject.SetActive(true);
        }
        else if (ceiling)
        {
            ceiling.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    void Update()
    {
        // Update continuously in the Editor (and on-device for rotation/aspect changes).
        Apply();
    }

    void OnDrawGizmos()
    {
        if (!cam) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        float innerBottom = -halfH + bottomOffset;
        float innerTop    =  halfH - topOffset;
        float innerLeft   = -halfW + sideMargin;
        float innerRight  =  halfW - sideMargin;

        var inner = new Rect(
            innerLeft,
            innerBottom,
            innerRight - innerLeft,
            innerTop    - innerBottom
        );

        // Play area preview (cyan translucent)
        Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
        Gizmos.DrawCube(
            new Vector3(inner.center.x, inner.center.y, 0f),
            new Vector3(inner.width, inner.height, 0.01f)
        );
    }
#endif
}
