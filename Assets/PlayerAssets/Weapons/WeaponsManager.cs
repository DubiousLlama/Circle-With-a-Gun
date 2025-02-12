using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using System;

public class WeaponsManager : MonoBehaviour
{
    public Weapon primaryWeapon;
    public Weapon secondaryWeapon;

    public GameObject defaultPrimaryWeapon;

    public GameObject testingSecondaryWeapon;

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
        Weapon[] weapons = player.transform.GetComponentsInChildren<Weapon>();
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

        if (primaryWeapon == null)
        {
            GameObject weaponInstance = Instantiate(defaultPrimaryWeapon, player.transform);
            primaryWeapon = weaponInstance.GetComponent<Weapon>();
        }

        #if UNITY_EDITOR
        if (secondaryWeapon == null && testingSecondaryWeapon != null)
        {
            GameObject weaponInstance = Instantiate(testingSecondaryWeapon, player.transform);
            secondaryWeapon = weaponInstance.GetComponent<Weapon>();
        }       
        #endif

    }

    public void EquipWeapon(GameObject weapon)
    {
        GameObject weaponInstance = Instantiate(weapon, player.transform);
        Weapon weaponComponent = weaponInstance.GetComponent<Weapon>();
        if (weaponComponent.weaponType == WeaponType.Primary)
        {
            if (primaryWeapon != null)
            {
                Destroy(primaryWeapon.gameObject);
            }
            primaryWeapon = weaponComponent;
            primaryWeapon.gameObject.SetActive(true);
        }
        else if (weaponComponent.weaponType == WeaponType.Secondary)
        {
            if (secondaryWeapon != null)
            {
                Destroy(secondaryWeapon.gameObject);
            }
            secondaryWeapon = weaponComponent;
            primaryWeapon.gameObject.SetActive(true);
        }
    }
}
