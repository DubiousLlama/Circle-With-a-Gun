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

    void Start()
    {
        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();
    }

    void Update()
    {
        Weapon weapon = weaponsManager.GetEquippedWeapon(weaponType);

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
        weaponsManager?.OnPointerDown(weaponType);
    }

    public void OnPointerUp()
    {
        weaponsManager?.OnPointerUp(weaponType);
    }
}
