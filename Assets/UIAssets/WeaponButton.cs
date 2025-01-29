using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    public TMP_Text text;
    public Image fill;
    public Button button;
    public WeaponType weaponType;
    private WeaponsManager weaponsManager;
    private Weapon weapon;

    void Start()
    {
        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();
    }

    void Update()
    {
        if (weaponType == WeaponType.Primary) {
            weapon = weaponsManager.primaryWeapon;
        } else {
            weapon = weaponsManager.secondaryWeapon;
        }

        button.interactable = weapon?.CanFire() ?? false;

        if (weapon == null || !weapon.isTemporary) {
            text.text = "";
            fill.fillAmount = 1.0f;
        } else {
            text.text = weapon.lifetimeRemaining.ToString("F0");
            fill.fillAmount = weapon.lifetimeRemaining / weapon.lifetime;
        }
    }

    public void OnPointerDown()
    {
        weapon?.OnPointerDown();
    }

    public void OnPointerUp()
    {
        weapon?.OnPointerUp();
    }
}
