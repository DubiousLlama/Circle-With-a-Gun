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

    void Awake()
    {
        weaponsManager = GetComponent<WeaponsManager>();
        
        if (!Platform.IsMobile())
        {
            inputActions = new PlayerInputActions();
        }
    }

    private void Start()
    {
        stats = PlayerStats.instance;
    }

    private void OnEnable()
    {
        if (!Platform.IsMobile())
        {
            inputActions.Enable();
        }
    }

    private void OnDisable()
    {
        if (!Platform.IsMobile())
        {
            inputActions.Disable();
        }
    }

    void Update()
    {
        if (!Platform.IsMobile())
        {
            moveInput = inputActions.Player.Move.ReadValue<Vector2>();
            lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        }
        
        HandlePlayerInput();
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
                rb.velocity = rb.velocity * 0.8f;
            }

        } else
        {
            rb.AddForce(movement.normalized * (moveSpeed + bonus) * stats.GetStatMod(StatTypes.MoveSpeed));

            if (movement == Vector2.zero)
            {
                rb.velocity = rb.velocity * 0.8f;
            }
        }

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + 90f;

        Transform firePoint = transform.Find("FirePoint");
        if (lookDir != Vector2.zero)
        {
            // Always rotate the player to face the aim direction
            rb.rotation = angle;

            // Rotate the FirePoint as the source of bullets
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
                var currentDevice = InputSystem.GetDevice<Gamepad>();
                bool isGamepadActive = currentDevice != null && 
                    (Mathf.Abs(currentDevice.rightStick.x.ReadValue()) > 0.1f || 
                     Mathf.Abs(currentDevice.rightStick.y.ReadValue()) > 0.1f);

                if (isGamepadActive)
                {
                    lookDir = lookInput;
                }
                else
                {
                    Vector2 mousePos = cam.ScreenToWorldPoint(lookInput);
                    Transform firePoint = transform.Find("FirePoint");
                    Vector2 referencePos = firePoint != null ? (Vector2)firePoint.position : rb.position;
                    lookDir = mousePos - referencePos;
                }
            }
            
            if (Platform.IsMobile())
            {
                weaponsManager.OnPrimaryUp();
            }
        }
    }
}
