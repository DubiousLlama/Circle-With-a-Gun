using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
public class WeaponItem : MonoBehaviour
{
    [HideInInspector]
    public GameObject weapon;

    void Start()
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        GetComponent<SpriteRenderer>().sprite = weaponComponent.sprite;
        SpriteRenderer rarityBackground = transform.Find("RarityBackground").GetComponent<SpriteRenderer>();
        TextMeshPro rarityText = rarityBackground.transform.Find("RarityText").GetComponent<TextMeshPro>();
        rarityText.text = ((int) weaponComponent.rarity + 1).ToString();
        rarityBackground.color = WeaponIndicator.rarityColors[weaponComponent.rarity];
    }

    void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        { 
            WeaponsManager weaponsManager = collision.gameObject.GetComponent<WeaponsManager>();
            weaponsManager.EquipWeapon(weapon);
            // Set the weapon item to (0.9,0.9, 0.9) for 0.1 seconds, then destroy it
            GetComponent<SpriteRenderer>().color = new Color(0.9f, 0.9f, 0.9f);
            Destroy(gameObject, 0.1f);
        }
    }
}
