using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    private Button button;
    private WeaponsManager weaponsManager;
    private CanvasGroup canvasGroup;
    public bool isFire1 = true;

    void Start()
    {
        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();

        button = GetComponent<Button>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        Weapon weapon = null;
        if (isFire1) {
            weapon = weaponsManager.GetEquippedWeapon(WeaponType.Legendary) ?? weaponsManager.GetEquippedWeapon(WeaponType.Primary);
        } else {
            weapon = weaponsManager.GetEquippedWeapon(WeaponType.Secondary);
        }

        if (weapon == null) {
            canvasGroup.alpha = 0.0f;
        } else {
            canvasGroup.alpha = 1.0f;
        }

        button.interactable = weapon?.CanFire() ?? false;
    }

    public void OnPointerDown()
    {
        if (isFire1)
        {
            weaponsManager.OnPrimaryDown();
        } else
        {
            weaponsManager.OnSecondaryDown();
        }
    }

    public void OnPointerUp()
    {
        if (isFire1)
        {
            weaponsManager.OnPrimaryUp();
        } else {
            weaponsManager.OnSecondaryUp();
        }
    }
}
