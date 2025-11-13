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
    private Vector2 lastGamepadLookDir = Vector2.right; // Store last gamepad aim direction

    void Awake()
    {
        weaponsManager = GetComponent<WeaponsManager>();
        
        if (!Platform.IsMobile())
        {
            inputActions = new PlayerInputActions();
        }
    }

    private void OnEnable()
    {
        if (!Platform.IsMobile() && inputActions != null)
            inputActions.Enable();
    }

    private void OnDisable()
    {
        if (!Platform.IsMobile() && inputActions != null)
            inputActions.Disable();
    }

    private void Start()
    {
        stats = PlayerStats.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Platform.IsMobile() && inputActions != null)
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            Vector2 lookValue = inputActions.Player.Look.ReadValue<Vector2>();

            bool gamepadActive = Gamepad.current != null &&
                                  (Mathf.Abs(Gamepad.current.rightStick.x.ReadValue()) > 0.1f ||
                                   Mathf.Abs(Gamepad.current.rightStick.y.ReadValue()) > 0.1f);

            if (gamepadActive)
            {
                isUsingGamepad = true;
                lookDir = lookValue;
                if (lookDir.sqrMagnitude > 0.0001f)
                    lastGamepadLookDir = lookDir.normalized;
            }
            else
            {
                isUsingGamepad = false;
                if (Mouse.current != null)
                {
                    lookInput = Mouse.current.position.ReadValue();
                }
                // if no mouse, keep lastGamepadLookDir as fallback
            }

            // If the player moved the mouse physically, switch to mouse mode
            if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0f)
            {
                isUsingGamepad = false;
            }
        }

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

        Transform firePoint = transform.Find("FirePoint");
        if (lookDir != Vector2.zero)
        {
            rb.rotation = angle;

            if (firePoint != null)
            {
                firePoint.up = lookDir.normalized;
            }
        }
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

        if (movementJoystick.Horizontal != 0 || movementJoystick.Vertical != 0)
        {
            movement.x = movementJoystick.Horizontal;
            movement.y = movementJoystick.Vertical;
        }
        else
        {
            movement = moveInput;
        }

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
            if (!Platform.IsMobile())
            {
                if (isUsingGamepad)
                {
                    // Using gamepad right stick for aiming - lookDir already set in Update
                    if (lastGamepadLookDir.sqrMagnitude > 0.0001f)
                        lookDir = lastGamepadLookDir;
                }
                else
                {
                    Vector2 mousePos = cam.ScreenToWorldPoint(lookInput);

                    Transform firePoint = transform.Find("FirePoint");
                    Vector2 playerCenter = rb.position;

                    float cursorToPlayerDist = Vector2.Distance(mousePos, playerCenter);

                    float inner = 1f; // distance at/inside which use player center fully
                    float outer = 2f; // distance at/above which use firePoint fully
                    float t = 0f;
                    if (outer > inner)
                        t = Mathf.Clamp01((cursorToPlayerDist - inner) / (outer - inner));

                    Vector2 referencePos = playerCenter;
                    if (firePoint != null)
                    {
                        referencePos = Vector2.Lerp(playerCenter, firePoint.position, t);
                    }

                    Vector2 newLook = mousePos - referencePos;
                    if (newLook.sqrMagnitude > 0.0001f)
                    {
                        lookDir = newLook;
                    }
                }
            }
            
            if (Platform.IsMobile())
            {
                weaponsManager.OnPrimaryUp();
            }
        }
    }
}
