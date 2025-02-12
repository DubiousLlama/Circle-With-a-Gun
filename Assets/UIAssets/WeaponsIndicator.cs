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

    private Image primaryWeaponIcon;
    private Image secondaryWeaponIcon;
    private Image specialWeaponIcon;

    private Image primaryWeaponLifetimeFill;
    private Image secondaryWeaponLifetimeFill;
    private Image specialWeaponLifetimeFill;

    private Image primaryWeaponCooldownFill;
    private Image secondaryWeaponCooldownFill;
    private Image specialWeaponCooldownFill;

    private WeaponsManager weaponsManager;

    private Color powerUpGreen = new Color(0.054901960784313725f, 0.7686274509803922f, 0);

    private void UpdateIndicator(Transform indicator, Weapon weapon)
    {
        Image icon = indicator.GetComponent<Image>();
        Image lifetimeFill = indicator.Find("LifetimeFill").GetComponent<Image>();
        Image cooldownFill = indicator.Find("CooldownFill").GetComponent<Image>();
        if (weapon == null) {
            indicator.gameObject.SetActive(false);
            return;
        }
        indicator.gameObject.SetActive(true);
        cooldownFill.fillAmount = weapon.isAutomatic ? 0 : weapon.cooldownRemaining / weapon.cooldown;
        lifetimeFill.fillAmount = weapon.isTemporary ? weapon.lifetimeRemaining / weapon.lifetime : 1;
        icon.sprite = weapon.weaponItem.GetComponent<SpriteRenderer>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
        primaryIndicator = transform.Find("Primary");
        secondaryIndicator = transform.Find("Secondary");
        specialIndicator = transform.Find("Special");

        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();

        UpdateIndicator(primaryIndicator, weaponsManager.weapons[WeaponSlot.One]);
        UpdateIndicator(secondaryIndicator, weaponsManager.weapons[WeaponSlot.Two]);
        UpdateIndicator(specialIndicator, weaponsManager.weapons[WeaponSlot.Three]);
    }
}
