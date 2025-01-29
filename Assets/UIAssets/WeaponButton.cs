using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    [Tooltip("The duration of the timer in seconds")]
    public float duration = 10.0f;
    public TMP_Text text;
    public Image fill;
    public Button button;
    public WeaponType weaponType;
    private WeaponsManager weaponsManager;
    private Weapon weapon;

    private float timeLeft;
    private bool isRunning = false;

    void Start()
    {
        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();
    }

    void Update()
    {
        if (isRunning)
        {
            timeLeft -= Time.deltaTime;
            text.text = timeLeft.ToString("F0");
            fill.fillAmount = timeLeft / duration;
            if (timeLeft <= 0)
            {
                isRunning = false;
                button.interactable = true;
                text.text = "0";
                fill.fillAmount = 0.0f;
            }
        }

        if (weaponType == WeaponType.Primary) {
            weapon = weaponsManager.primaryWeapon;
        } else {
            weapon = weaponsManager.secondaryWeapon;
        }

        button.interactable = weapon != null && weapon.cooldownRemaining <= 0;

        if (weapon == null || !weapon.isTemporary) {
            text.text = "";
            fill.fillAmount = 1.0f;
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
