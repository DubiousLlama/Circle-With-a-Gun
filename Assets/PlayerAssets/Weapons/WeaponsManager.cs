using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;
using System.Linq;

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

    private bool wasPressingRightTrigger = false;
    private bool wasPressingLeftTrigger = false;

    void Awake()
    {
        weapons[WeaponSlot.One] = null;
        weapons[WeaponSlot.Two] = null;
        weapons[WeaponSlot.Three] = null;

        isFiring[WeaponType.Primary] = false;
        isFiring[WeaponType.Secondary] = false;
        isFiring[WeaponType.Legendary] = false;
    }

    void Start()
    {
        player = GameObject.Find("PC");
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // Don't update if game is paused

        // Handle input from old Input System (mouse, keyboard, gamepad)
        if (!Platform.IsMobile())
        {
            // Check triggers
            float rightTrigger = Input.GetAxis("RightTrigger");
            float leftTrigger = Input.GetAxis("LeftTrigger");
            bool pressingRightTrigger = rightTrigger > 0.5f;
            bool pressingLeftTrigger = leftTrigger > 0.5f;
            
            // Primary fire (Left Click OR Right Trigger)
            if (Input.GetButtonDown("Fire1") || (pressingRightTrigger && !wasPressingRightTrigger))
            {
                OnPrimaryDown();
            }
            if (Input.GetButtonUp("Fire1") || (!pressingRightTrigger && wasPressingRightTrigger))
            {
                OnPrimaryUp();
            }
            
            // Secondary fire (Right Click OR Left Trigger)
            if (Input.GetButtonDown("Fire2") || (pressingLeftTrigger && !wasPressingLeftTrigger))
            {
                OnSecondaryDown();
            }
            if (Input.GetButtonUp("Fire2") || (!pressingLeftTrigger && wasPressingLeftTrigger))
            {
                OnSecondaryUp();
            }
            
            wasPressingRightTrigger = pressingRightTrigger;
            wasPressingLeftTrigger = pressingLeftTrigger;
        }
        
        // Mobile joystick buttons are handled by WeaponButton.cs

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

            // Check if Fire1 button is pressed
            if (Input.GetButton("Fire1") || Input.GetAxis("RightTrigger") > 0.5f)
            {
                OnPrimaryDown();
            }
            isFiring[WeaponType.Primary] = false;
        }

        if (weaponComponent.getFinalType() == WeaponType.Primary && (Input.GetButton("Fire1") || Input.GetAxis("RightTrigger") > 0.5f))
        {
            OnPrimaryDown();
        }
    }
}
