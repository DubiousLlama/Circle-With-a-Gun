using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// World-space crosshair placed at the mouse via <see cref="Camera.ScreenToWorldPoint"/>.
/// Visibility uses <see cref="MenuInputMode"/> (same source of truth as menu/cursor switching).
/// While visible, suppresses the hardware cursor so the crosshair replaces it.
/// </summary>
public class CrosshairController : MonoBehaviour
{
    /// <summary>
    /// True while this crosshair is shown and should hide the OS cursor.
    /// Read by <see cref="GameManager"/> so cursor state stays consistent.
    /// </summary>
    public static bool IsReplacingCursor { get; private set; }

    [SerializeField] private Camera cam;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (cam == null)
            cam = Camera.main;
    }

    private void OnDisable()
    {
        IsReplacingCursor = false;
    }

    private void LateUpdate()
    {
        if (Platform.IsMobile())
        {
            SetVisible(false);
            return;
        }

        MenuInputMode.Tick();

        bool mouseMode = !MenuInputMode.ShouldSelectForController;
        bool gameplayActive = GameManager.Instance == null || !GameManager.Instance.IsPaused();
        bool show = mouseMode && gameplayActive;

        SetVisible(show);

        if (!show || Mouse.current == null)
            return;

        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return;

        // Same conversion PlayerMovement uses for mouse aim
        Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        transform.position = mousePos;
    }

    private void SetVisible(bool visible)
    {
        bool wasReplacing = IsReplacingCursor;
        IsReplacingCursor = visible;

        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        // Apply immediately so we don't wait for the next MenuInputMode mouse/gamepad edge in GameManager
        if (visible && !wasReplacing)
            Cursor.visible = false;
        else if (!visible && wasReplacing && !MenuInputMode.ShouldSelectForController)
            Cursor.visible = true;
    }
}
