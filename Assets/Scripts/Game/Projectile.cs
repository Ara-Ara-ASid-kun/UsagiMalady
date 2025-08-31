using UnityEngine;

/// Simple upward projectile that "pops" shapes:
/// - applies a sideways shove and upward kick at the contact point
/// - guarantees a minimum upward velocity so the bounce is visible
/// - briefly boosts gravity so the piece falls back down naturally
/// - self-destructs after the first hit (or after lifeTime expires)
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Initial travel speed of the projectile (world units per second).")]
    public float speed = 18f;

    [Tooltip("Time before the projectile despawns automatically (seconds).")]
    public float lifeTime = 3f;

    [Header("Hit Effects")]
    [Tooltip("Sideways shove applied at the contact (derived from collision normal X).")]
    public float lateralImpulse = 6f;

    [Tooltip("Upward kick applied at the contact so the piece pops up.")]
    public float upwardImpulse = 8f;

    [Tooltip("If the shape’s vertical speed is below this after impact, it will be clamped up to it.")]
    public float minUpwardVelocity = 6f;

    [Tooltip("Extra gravity temporarily added to the hit shape, so it arcs back down.")]
    public float extraGravityOnHit = 1.0f;

    [Tooltip("Duration of the extra gravity boost (seconds).")]
    public float extraGravityDuration = 0.45f;

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// Launch the projectile in a direction. Caller decides the spawn position.
    public void Fire(Vector2 dir)
    {
        // Use linearVelocity to align with your 2D physics settings
        rb.linearVelocity = dir.normalized * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        var shape = col.collider.GetComponent<Shape>();
        if (shape != null)
        {
            // Where we hit and what the local surface normal was
            var contact = col.GetContact(0);
            var shapeRb = shape.GetComponent<Rigidbody2D>();

            // Use only the X component for lateral shove (prevents fighting the upward kick)
            Vector2 n = contact.normal;
            Vector2 lateral = new Vector2(n.x, 0f).normalized * lateralImpulse;

            // Upward kick for a satisfying pop
            Vector2 upward = Vector2.up * upwardImpulse;

            // Apply at the contact point for nicer angular motion
            shapeRb.AddForceAtPosition(lateral + upward, contact.point, ForceMode2D.Impulse);

            // Ensure it visibly travels upward after the hit
            Vector2 v = shapeRb.linearVelocity;
            if (v.y < minUpwardVelocity)
            {
                v.y = minUpwardVelocity;
                shapeRb.linearVelocity = v;
            }

            // Temporarily increase gravity so it comes back down naturally
            shape.BumpGravity(extraGravityOnHit, extraGravityDuration, this);

            // Optional extra juice:
            // shapeRb.AddTorque(Random.Range(-20f, 20f), ForceMode2D.Impulse);
        }

        // One-hit projectile. Remove this line if you want piercing shots.
        Destroy(gameObject);
    }
}
