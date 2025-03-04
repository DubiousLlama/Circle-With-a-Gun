using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System;

public class WeaponsManager : MonoBehaviour
{
    [HideInInspector]
    public Dictionary<WeaponSlot, Weapon> weapons = new Dictionary<WeaponSlot, Weapon>();
    [HideInInspector]
    public Dictionary<WeaponType, bool> isFiring = new Dictionary<WeaponType, bool>();

    public GameObject defaultPrimaryWeapon;

    public GameObject testingSecondaryWeapon;

    private GameObject player;

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
        
        EquipWeapon(Instantiate(defaultPrimaryWeapon));

        #if UNITY_EDITOR
        if (testingSecondaryWeapon != null)
        {
            EquipWeapon(Instantiate(testingSecondaryWeapon));
        }
        #endif
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

    void Update()
    {
        // WeaponButton is used for mobile
        if (Platform.IsDesktop()) {
            // Primary weapon
            if (Input.GetButtonDown("Fire1"))
            {
                if (weapons[WeaponSlot.Three] == null)
                {
                    OnPointerDown(WeaponType.Primary);
                } else
                {
                    OnPointerDown(WeaponType.Legendary);
                }
                
            }
            if (Input.GetButtonUp("Fire1"))
            {
                OnPointerUp(WeaponType.Primary);
                OnPointerUp(WeaponType.Legendary);
            }

            // Secondary weapon
            if (Input.GetButtonDown("Fire2"))
            {
                OnPointerDown(WeaponType.Secondary);
            }
            if (Input.GetButtonUp("Fire2"))
            {
                OnPointerUp(WeaponType.Secondary);
            }
        }
    }

    public void OnPointerDown(WeaponType weaponType)
    {
        isFiring[weaponType] = true;
    }

    public void OnPointerUp(WeaponType weaponType)
    {
        isFiring[weaponType] = false;
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
            OnPointerUp(WeaponType.Primary);
        }
    }
}
