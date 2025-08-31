using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;      // New Input System
using System.Collections.Generic;

/// Spawns upward projectiles at the X position of a tap/click.
/// Uses the New Input System only. Make sure:
/// Project Settings → Player → Active Input Handling = "Input System Package (New)".
public class ProjectileShooter : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("Camera used to convert screen → world. If empty, uses Camera.main.")]
    public Camera cam;

    [Tooltip("Projectile prefab with a Projectile component.")]
    public Projectile projectilePrefab;

    [Header("Spawn")]
    [Tooltip("World Y position where projectiles start (slightly above the floor).")]
    public float bottomY = -4.7f;

    [Tooltip("Block firing when the game is paused (Time.timeScale == 0).")]
    public bool blockWhenPaused = true;

    [Header("SFX")]
    [Tooltip("Sound played each time a projectile is spawned.")]
    public AudioClip sfxShoot;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (!cam) Debug.LogWarning("[ProjectileShooter] No Camera assigned and Camera.main not found.");
        if (!projectilePrefab) Debug.LogWarning("[ProjectileShooter] projectilePrefab is not assigned.");
    }

    void Update()
    {
        if (blockWhenPaused && Time.timeScale == 0f) return;
        if (!projectilePrefab || !cam) return;

        if (TapBegan(out Vector2 screenPos))
        {
            // Ignore taps landing on UI (buttons, panels, etc.)
            if (IsPointerOverUI(screenPos)) return;

            // Convert screen → world. For ortho cameras, Z doesn't matter; for perspective,
            // use the distance from camera to your gameplay plane (here: |cam.z|).
            var sp = new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z));
            Vector3 world = cam.ScreenToWorldPoint(sp);

            Vector3 spawn = new Vector3(world.x, bottomY, 0f);
            var proj = Instantiate(projectilePrefab, spawn, Quaternion.identity);
            proj.Fire(Vector2.up);

            if (sfxShoot) AudioManager.PlaySFX(sfxShoot, 0.5f);
        }
    }

    // --- New Input System: multitouch + mouse ---
    bool TapBegan(out Vector2 screenPos)
    {
        // Touches (mobile)
        var ts = Touchscreen.current;
        if (ts != null)
        {
            // Iterate all active touches; fire on the ones that began this frame
            foreach (var t in ts.touches)
            {
                if (t.press.wasPressedThisFrame)
                {
                    screenPos = t.position.ReadValue();
                    return true;
                }
            }
        }

        // Mouse (Editor / desktop)
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }

        screenPos = default;
        return false;
    }

    // --- UI blocking (GraphicRaycaster via EventSystem) ---
    bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
