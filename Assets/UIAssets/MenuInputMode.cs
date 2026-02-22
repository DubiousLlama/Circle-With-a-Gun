using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single source of truth for whether the user is in "controller mode" for menu navigation.
/// Call <see cref="Tick"/> once per frame (idempotent); read <see cref="LastInputWasGamepad"/>
/// or <see cref="ShouldSelectForController"/> from any menu script.
/// </summary>
public static class MenuInputMode
{
    private const float StickDeadzone = 0.25f;
    private const float DpadThreshold = 0.5f;
    private const float MouseDeltaThreshold = 0.1f;

    public static bool LastInputWasGamepad { get; private set; } = false;

    /// <summary>True when a gamepad is connected and the most recent input came from it.</summary>
    public static bool ShouldSelectForController =>
        Gamepad.current != null && LastInputWasGamepad;

    /// <summary>True if the mouse produced input this frame (delta or button).</summary>
    public static bool MouseActiveThisFrame { get; private set; }

    /// <summary>True if a gamepad produced navigation-relevant input this frame.</summary>
    public static bool GamepadActiveThisFrame { get; private set; }

    private static int lastUpdatedFrame = -1;

    /// <summary>
    /// Update input-mode state for the current frame.
    /// Safe to call from multiple scripts; only runs once per frame.
    /// </summary>
    public static void Tick()
    {
        int frame = Time.frameCount;
        if (frame == lastUpdatedFrame) return;
        lastUpdatedFrame = frame;

        MouseActiveThisFrame = false;
        GamepadActiveThisFrame = false;

        if (Mouse.current != null)
        {
            bool mouseMoved = Mouse.current.delta.ReadValue().sqrMagnitude > MouseDeltaThreshold;
            bool mouseButton = Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed;
            if (mouseMoved || mouseButton)
            {
                MouseActiveThisFrame = true;
                LastInputWasGamepad = false;
            }
        }

        if (Gamepad.current != null)
        {
            var stick = Gamepad.current.leftStick.ReadValue();
            var dpad = Gamepad.current.dpad.ReadValue();

            bool stickActive = Mathf.Abs(stick.x) > StickDeadzone || Mathf.Abs(stick.y) > StickDeadzone;
            bool dpadActive = Mathf.Abs(dpad.x) > DpadThreshold || Mathf.Abs(dpad.y) > DpadThreshold;
            bool buttonPressed =
                Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.leftShoulder.wasPressedThisFrame ||
                Gamepad.current.rightShoulder.wasPressedThisFrame;

            if (stickActive || dpadActive || buttonPressed)
            {
                GamepadActiveThisFrame = true;
                LastInputWasGamepad = true;
            }
        }
    }
}
