using UnityEngine;

/// Represents a falling shape in the stage.
/// - Configured by Spawner with type, color, and gravity
/// - Knows how to update its visual (SpriteRenderer tint)
/// - Reports collisions to GameController for clash logic
/// - Can temporarily boost gravity when hit by a projectile
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Shape : MonoBehaviour
{
    [Header("Identity")]
    public ShapeType shapeType;       // e.g. Square / Circle / Triangle
    public RainbowColor color;        // enum for 7 rainbow colors
    public SpriteRenderer sr;         // assigned in prefab; visual renderer

    [Header("Runtime")]
    public bool isMarkedForRemoval;   // flagged when matched for deletion

    Rigidbody2D rb;
    GameController game;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!sr) sr = GetComponent<SpriteRenderer>();
        ApplyVisual();
    }

    void Start()
    {
        // Cache controller reference for reporting collisions
        game = FindFirstObjectByType<GameController>();
    }

    /// Called by Spawner to set type, color, and fall speed.
    public void Configure(ShapeType type, RainbowColor c, float gravityScale)
    {
        shapeType = type;
        color = c;

        if (!rb) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        ApplyVisual();
    }

    /// Updates the sprite’s color tint based on RainbowColor enum.
    void ApplyVisual()
    {
        if (sr) sr.color = color.ToColor();
    }

    /// Called by projectiles to temporarily increase gravity (makes it fall faster).
    public void BumpGravity(float extra, float duration, MonoBehaviour host)
    {
        host.StartCoroutine(BoostGravity(extra, duration));
    }

    System.Collections.IEnumerator BoostGravity(float extra, float dur)
    {
        rb.gravityScale += extra;
        yield return new WaitForSeconds(dur);
        rb.gravityScale -= extra;
    }

    // --- Collision + bookkeeping ---

    void OnCollisionEnter2D(Collision2D col)
    {
        // Only care about other shapes (not walls or floor)
        var other = col.collider.GetComponent<Shape>();
        if (other != null && game != null)
        {
            game.ReportCollision(this, other);
        }
    }

    void OnDestroy()
    {
        if (game != null)
            game.UnregisterShape(this);
    }
}
