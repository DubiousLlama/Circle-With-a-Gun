using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using System.Linq;
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

        // Initialize new Input System
        inputActions = new PlayerInputActions();
        inputActions.Player.Fire.started += OnFireStarted;
        inputActions.Player.Fire.canceled += OnFireCanceled;
        inputActions.Player.FireSecondary.started += OnFireSecondaryStarted;
        inputActions.Player.FireSecondary.canceled += OnFireSecondaryCanceled;
    }

    void Start()
    {
        player = GameObject.Find("PC");
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void OnFireStarted(InputAction.CallbackContext context)
    {
        OnPrimaryDown();
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        OnPrimaryUp();
    }

    private void OnFireSecondaryStarted(InputAction.CallbackContext context)
    {
        OnSecondaryDown();
    }

    private void OnFireSecondaryCanceled(InputAction.CallbackContext context)
    {
        OnSecondaryUp();
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
        return player.GetComponent<PlayerMovement>().isMoving();
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

    void Update()
    {
        if (Time.timeScale == 0f) return; // Don't update if game is paused

        // Mobile joystick buttons are handled by WeaponButton.cs
        // Desktop keyboard/mouse/gamepad is now handled by the new Input System callbacks

        Weapon secondaryWeapon = GetEquippedWeapon(WeaponType.Secondary);
        if (secondaryWeapon != null)
        {
            bool offCooldown = secondaryWeapon.cooldownRemaining <= 0;
            secondaryIndicatorActive.SetActive(offCooldown);
            secondaryActive.SetActive(offCooldown && !isPlayerMoving());
            secondaryInactive.SetActive(!isPlayerMoving());
        }
    }

    public void EquipWeapon(GameObject weapon)
    {
        // Change parent of weapon to player
        weapon.transform.SetParent(player.transform);
        weapon.transform.position = player.transform.position;
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        this.weapons[weaponComponent.weaponSlot]?.Expire();
        weaponComponent.weaponsManager = this;
        weaponComponent.Equip();

        if (weaponComponent.getFinalType() == WeaponType.Legendary)
        {
            GameMusic.instance.PlayEventTrack("legendary");

            // Check if Fire action is pressed (works with both old and new input)
            if (inputActions.Player.Fire.IsPressed())
            {
                OnPrimaryDown();
            }
            isFiring[WeaponType.Primary] = false;
        }

        if (weaponComponent.getFinalType() == WeaponType.Primary && inputActions.Player.Fire.IsPressed())
        {
            OnPrimaryDown();
        }
    }
}
