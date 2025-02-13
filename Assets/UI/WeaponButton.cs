using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponButton : MonoBehaviour
{
    public WeaponType weaponType;

    private TMP_Text text;
    private Image fill;
    private Button button;
    private WeaponsManager weaponsManager;
    private Image icon;
    private Sprite defaultSprite;
    private CanvasGroup canvasGroup;

    void Start()
    {
        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();

        text = transform.Find("Text").GetComponent<TMP_Text>();
        fill = transform.Find("Fill").GetComponent<Image>();
        button = GetComponent<Button>();
        icon = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        defaultSprite = icon.sprite;
    }

    void Update()
    {
        Weapon weapon = weaponsManager.GetEquippedWeapon(weaponType);

        if (weapon == null) {
            canvasGroup.alpha = 0.0f;
        } else {
            canvasGroup.alpha = 1.0f;
        }

        button.interactable = weapon?.CanFire() ?? false;

        icon.sprite = weapon?.sprite ?? defaultSprite;

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
