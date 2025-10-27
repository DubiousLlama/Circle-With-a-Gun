using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    float bonus = 0f;
    public Rigidbody2D rb;
    public Camera cam;
    public Joystick movementJoystick;
    public Joystick directionJoystick;
    private WeaponsManager weaponsManager;
    private PlayerStats stats;

    [HideInInspector]
    public Vector2 movement;
    Vector2 lookDir;
    public bool secondaryMoveStop = false;

    float initialScale = 0.4f;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isUsingGamepad = false;

    void Awake()
    {
        weaponsManager = GetComponent<WeaponsManager>();
        
        // Initialize new Input System
        inputActions = new PlayerInputActions();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    private void Start()
    {
        stats = PlayerStats.instance;
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
        
        // Detect if using gamepad based on the control device
        isUsingGamepad = context.control.device is Gamepad;
    }

    // Update is called once per frame
    void Update()
    {
            // TEMPORARY DEBUG for new Input System
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Debug.Log($"Gamepad detected: {gamepad.name}");
            if (gamepad.leftStick.ReadValue().magnitude > 0.1f)
            {
                Debug.Log($"Left Stick: {gamepad.leftStick.ReadValue()}");
            }
            if (gamepad.rightStick.ReadValue().magnitude > 0.1f)
            {
                Debug.Log($"Right Stick: {gamepad.rightStick.ReadValue()}");
            }
            if (gamepad.rightTrigger.ReadValue() > 0.1f)
            {
                Debug.Log($"Right Trigger: {gamepad.rightTrigger.ReadValue()}");
            }
            if (gamepad.leftTrigger.ReadValue() > 0.1f)
            {
                Debug.Log($"Left Trigger: {gamepad.leftTrigger.ReadValue()}");
            }
        }
        else
        {
            Debug.LogWarning("No gamepad detected by new Input System!");
        }
        
        Debug.Log($"Move input: {moveInput}, Look input: {lookInput}, Using gamepad: {isUsingGamepad}");
        // END DEBUG

        // Get input from the joysticks or keyboard/mouse and set the movement and look direction vectors accordingly
        HandlePlayerInput();

        // If the player has a speed bonus, shrink them porportionally
        HandleSpeedBonusResize();
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb.velocity.magnitude < 0.01f)
        {
            rb.velocity = Vector2.zero;
        }

        if (secondaryMoveStop)
        {
            if (rb.velocity.magnitude < 0.01f)
            {
                Invoke(nameof(SecondaryFire), 0.15f);
            }
            else
            {
                // Slow the player down quickly if they can't move
                rb.velocity = rb.velocity * 0.8f;
            }

        } else
        {
            rb.AddForce(movement.normalized * (moveSpeed + bonus) * stats.GetStatMod(StatTypes.MoveSpeed));

            if (movement == Vector2.zero)
            {
                // Slow the player down quickly if they can't move
                rb.velocity = rb.velocity * 0.8f;
            }
        }

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + 90f;

        rb.rotation = angle;
    }

    private void SecondaryFire()
    {
        Weapon weapon = weaponsManager.GetEquippedWeapon(WeaponType.Secondary);
        if (weapon == null || weapon.cooldownRemaining > 0)
        {
            secondaryMoveStopDisable();
            return;
        }
        weapon.Fire();
        weapon.FireCleanup();
        Invoke(nameof(secondaryMoveStopDisable), 0.05f);
    }

    private void secondaryMoveStopDisable()
    {
        secondaryMoveStop = false;
    }

    public void SecondaryActivationWhileMoving()
    {
        if (!secondaryMoveStop)
        {
            AudioManager.instance.PlaySfx("SecondaryWrong");
        }
        secondaryMoveStop = true;
    }

    private void HandleSpeedBonusResize()
    {
        // Shrink the player by the ratio of the bonus to their moveSpeed
        float bonusRatio = Mathf.Min(PlayerStats.instance.GetStatMod(StatTypes.MoveSpeed) - 1, 1);
        float scale = (1 - bonusRatio / 5) * 0.4f;
        scale = Mathf.Clamp(scale, 0.2f, initialScale);
        transform.localScale = new Vector3(scale, scale, 1);
    }

    public bool isMoving()
    {
        return movement != Vector2.zero;
    }

    private void HandlePlayerInput()
    {
        rb = GetComponent<Rigidbody2D>();

        // Check mobile joysticks first
        if (movementJoystick.Horizontal != 0 || movementJoystick.Vertical != 0)
        {
            movement.x = movementJoystick.Horizontal;
            movement.y = movementJoystick.Vertical;
        }
        else
        {
            // Use new Input System for keyboard/gamepad
            movement = moveInput;
        }

        // Check mobile direction joystick first
        if (directionJoystick.Horizontal != 0 || directionJoystick.Vertical != 0)
        {
            lookDir.x = directionJoystick.Horizontal;
            lookDir.y = directionJoystick.Vertical;
            if (Platform.IsMobile())
            {
                weaponsManager.OnPrimaryDown();
            }
        }
        else
        {
            if (isUsingGamepad)
            {
                // Using gamepad right stick for aiming
                lookDir = lookInput;
            }
            else
            {
                // Using mouse for aiming (lookInput contains screen position)
                Vector2 mousePos = cam.ScreenToWorldPoint(lookInput);
                lookDir = mousePos - rb.position;
            }
            
            if (Platform.IsMobile())
            {
                weaponsManager.OnPrimaryUp();
            }
        }
    }
}
