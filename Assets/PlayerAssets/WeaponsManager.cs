using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class WeaponsManager : MonoBehaviour
{
    [HideInInspector]
    public Weapon primaryWeapon;
    [HideInInspector]
    public Weapon secondaryWeapon;

    private GameObject player;

    void Start()
    {
        player = GameObject.Find("PC");
    }

    void Update()
    {
        UpdateWeapons();

        // WeaponButton is used for mobile
        if (Platform.IsDesktop()) {
            // Primary weapon
            if (Input.GetButtonDown("Fire1"))
            {
                primaryWeapon?.OnPointerDown();
            }
            if (Input.GetButtonUp("Fire1"))
            {
                primaryWeapon?.OnPointerUp();
            }

            // Secondary weapon
            if (Input.GetButtonDown("Fire2"))
            {
                secondaryWeapon?.OnPointerDown();
            }
            if (Input.GetButtonUp("Fire2"))
            {
                secondaryWeapon?.OnPointerUp();
            }
        }
    }

    private void UpdateWeapons()
    {
        Weapon[] weapons = player.GetComponents<Weapon>();
        foreach (Weapon weapon in weapons)
        {
            if (weapon.weaponType == WeaponType.Primary)
            {
                primaryWeapon = weapon;
            }
            if (weapon.weaponType == WeaponType.Secondary)
            {
                secondaryWeapon = weapon;
            }
        }
    }
}
