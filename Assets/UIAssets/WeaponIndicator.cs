using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIndicator : MonoBehaviour
{
    public WeaponSlot weaponSlot;

    private CanvasGroup canvasGroup;

    private Image icon;
    private Image lifetimeFill;
    private Image cooldownFill;
    private Image rarityBackground;
    private TextMeshProUGUI rarityText;

    private WeaponsManager weaponsManager;
    
    private Dictionary<WeaponRarity, Color> rarityColors = new Dictionary<WeaponRarity, Color> {
        { WeaponRarity.Common, new Color(0, 0, 0) },
        { WeaponRarity.Uncommon, new Color(0.1176471f, 0.5333334f, 0.9019608f) },
        { WeaponRarity.Rare, new Color(1, 0.7568628f, 0.027451f) },
        { WeaponRarity.Legendary, new Color(0.8470588f, 0.1058824f, 0.3764706f) }
    };

    void Start()
    {
        weaponsManager = GameObject.Find("PC").GetComponent<WeaponsManager>();

        canvasGroup = GetComponent<CanvasGroup>();

        icon = transform.Find("Icon").GetComponent<Image>();
        lifetimeFill = transform.Find("LifetimeFill").GetComponent<Image>();
        cooldownFill = transform.Find("CooldownFill").GetComponent<Image>();
        rarityBackground = transform.Find("RarityBackground").GetComponent<Image>();
        rarityText = rarityBackground.transform.Find("RarityText").GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Weapon weapon = weaponsManager.weapons[weaponSlot];
        if (weapon == null) {
            canvasGroup.alpha = 0f;
            return;
        }
        if (weaponsManager.GetEquippedWeapon(weapon.weaponType) != weapon) {
            canvasGroup.alpha = 0.5f;
        } else {
            canvasGroup.alpha = 1f;
        }
        cooldownFill.fillAmount = weapon.isAutomatic ? 0 : weapon.cooldownRemaining / weapon.cooldown;
        lifetimeFill.fillAmount = weapon.isTemporary ? weapon.lifetimeRemaining / weapon.lifetime : 1;
        icon.sprite = weapon.sprite;

        rarityText.text = ((int) weapon.rarity + 1).ToString();
        Debug.Log(rarityColors[weapon.rarity]);
        rarityBackground.color = rarityColors[weapon.rarity];
    }
}
