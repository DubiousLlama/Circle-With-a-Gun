using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponsManager : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<WeaponSlot, Weapon> weapons = new Dictionary<WeaponSlot, Weapon>();
    [HideInInspector]
    public Dictionary<WeaponType, bool> isFiring = new Dictionary<WeaponType, bool>();

    public GameObject defaultPrimaryWeapon;
    public GameObject testingSecondaryWeapon;

    private GameObject player;

    public GameObject secondaryDisplay;
    public GameObject secondaryInactive;
    public GameObject secondaryActive;
    public GameObject secondaryIndicatorActive;

    public PlayerMovement playerMovement;

    public GameObject RechargeBar;

    private PlayerInputActions inputActions;

    void Awake()
    {
        weapons[WeaponSlot.One] = null;
        weapons[WeaponSlot.Two] = null;
        weapons[WeaponSlot.Three] = null;

        isFiring[WeaponType.Primary] = false;
        isFiring[WeaponType.Secondary] = false;
        isFiring[WeaponType.Legendary] = false;

        if (!Platform.IsMobile())
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Fire.performed += OnFirePerformed;
            inputActions.Player.Fire.canceled += OnFireCanceled;
            inputActions.Player.FireSecondary.performed += OnFireSecondaryPerformed;
            inputActions.Player.FireSecondary.canceled += OnFireSecondaryCanceled;
        }
    }

    void Start()
    {
        player = GameObject.Find("PC");
    }

    private void OnEnable()
    {
        if (!Platform.IsMobile())
        {
            inputActions?.Enable();
        }
    }

    private void OnDisable()
    {
        if (!Platform.IsMobile())
        {
            inputActions?.Disable();
        }
    }

    private void OnDestroy()
    {
        if (!Platform.IsMobile() && inputActions != null)
        {
            inputActions.Player.Fire.performed -= OnFirePerformed;
            inputActions.Player.Fire.canceled -= OnFireCanceled;
            inputActions.Player.FireSecondary.performed -= OnFireSecondaryPerformed;
            inputActions.Player.FireSecondary.canceled -= OnFireSecondaryCanceled;
        }
    }

    private void OnFirePerformed(InputAction.CallbackContext context)
    {
        OnPrimaryDown();
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        OnPrimaryUp();
    }

    private void OnFireSecondaryPerformed(InputAction.CallbackContext context)
    {
        OnSecondaryDown();
    }

    private void OnFireSecondaryCanceled(InputAction.CallbackContext context)
    {
        OnSecondaryUp();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        Weapon secondaryWeapon = GetEquippedWeapon(WeaponType.Secondary);
        if (secondaryWeapon != null)
        {
            bool offCooldown = secondaryWeapon.cooldownRemaining <= 0;
            secondaryIndicatorActive.SetActive(offCooldown);
            secondaryActive.SetActive(offCooldown && !isPlayerMoving());
            secondaryInactive.SetActive(!isPlayerMoving());
        }
    }

    public Weapon GetEquippedWeapon(WeaponType weaponType)
    {
        if (weaponType == WeaponType.Primary) {
            return weapons[WeaponSlot.One];
        } else if (weaponType == WeaponType.Secondary) {
            return weapons[WeaponSlot.Two];
        } else {
            return weapons[WeaponSlot.Three];
        }
    }

    public bool isPlayerMoving()
    {
        return playerMovement.isMoving();
    }

    public void OnPrimaryDown()
    {
        if (weapons[WeaponSlot.Three] == null)
        {
            isFiring[WeaponType.Primary] = true;
        } else
        {
            isFiring[WeaponType.Legendary] = true;
        }
    }

    public void OnPrimaryUp()
    {
        isFiring[WeaponType.Primary] = false;
        isFiring[WeaponType.Legendary] = false;
    }

    public void OnSecondaryDown()
    {
        isFiring[WeaponType.Secondary] = true;
    }

    public void OnSecondaryUp()
    {
        isFiring[WeaponType.Secondary] = false;
    }

    public void EquipWeapon(GameObject weapon)
    {
        weapon.transform.SetParent(player.transform);
        weapon.transform.position = player.transform.position;
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        this.weapons[weaponComponent.weaponSlot]?.Expire();
        weaponComponent.weaponsManager = this;
        weaponComponent.Equip();

        if (weaponComponent.getFinalType() == WeaponType.Legendary)
        {
            GameMusic.instance.PlayEventTrack("legendary");

            if (!Platform.IsMobile() && inputActions.Player.Fire.IsPressed())
            {
                OnPrimaryDown();
            }
            isFiring[WeaponType.Primary] = false;
        }

        if (weaponComponent.getFinalType() == WeaponType.Primary && !Platform.IsMobile() && inputActions.Player.Fire.IsPressed())
        {
            OnPrimaryDown();
        }

        if (weaponComponent.getFinalType() == WeaponType.Secondary)
        {
            isFiring[WeaponType.Primary] = false;

            // Ensure secondary weapon doesn't start firing automatically
            isFiring[WeaponType.Secondary] = false;
        }
    }
}
