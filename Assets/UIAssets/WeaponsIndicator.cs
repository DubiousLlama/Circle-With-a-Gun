using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsIndicator : MonoBehaviour
{
    private Transform primaryIndicator;
    private Transform secondaryIndicator;
    private Transform specialIndicator;

    private Image primaryWeaponLifetimeFill;
    private Image secondaryWeaponLifetimeFill;
    private Image specialWeaponLifetimeFill;

    private Image primaryWeaponCooldownFill;
    private Image secondaryWeaponCooldownFill;
    private Image specialWeaponCooldownFill;

    private WeaponsManager weaponsManager;

    private Color powerUpGreen = new Color(0.054901960784313725f, 0.7686274509803922f, 0);

    private void Start()
    {
        primaryIndicator = transform.Find("Primary");
        secondaryIndicator = transform.Find("Secondary");
        specialIndicator = transform.Find("Special");

        primaryWeaponLifetimeFill = primaryIndicator.Find("LifetimeFill").GetComponent<Image>();
        secondaryWeaponLifetimeFill = secondaryIndicator.Find("LifetimeFill").GetComponent<Image>();
        specialWeaponLifetimeFill = specialIndicator.Find("LifetimeFill").GetComponent<Image>();

        primaryWeaponCooldownFill = primaryIndicator.Find("CooldownFill").GetComponent<Image>();
        secondaryWeaponCooldownFill = secondaryIndicator.Find("CooldownFill").GetComponent<Image>();
        specialWeaponCooldownFill = specialIndicator.Find("CooldownFill").GetComponent<Image>();

        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();
    }

    private void UpdateIndicator(Transform indicator, Image lifetimeFill, Image cooldownFill, Weapon weapon)
    {
        if (weapon == null) {
            indicator.gameObject.SetActive(false);
            return;
        }
        indicator.gameObject.SetActive(true);
        cooldownFill.fillAmount = weapon.isAutomatic ? 0 : weapon.cooldownRemaining / weapon.cooldown;
        lifetimeFill.fillAmount = weapon.isTemporary ? weapon.lifetimeRemaining / weapon.lifetime : 1;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateIndicator(primaryIndicator, primaryWeaponLifetimeFill, primaryWeaponCooldownFill, weaponsManager.weapons[WeaponSlot.One]);
        UpdateIndicator(secondaryIndicator, secondaryWeaponLifetimeFill, secondaryWeaponCooldownFill, weaponsManager.weapons[WeaponSlot.Two]);
        UpdateIndicator(specialIndicator, specialWeaponLifetimeFill, specialWeaponCooldownFill, weaponsManager.weapons[WeaponSlot.Three]);
    }
}
