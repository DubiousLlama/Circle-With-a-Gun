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
        SpriteRenderer weaponIcon = transform.Find("WeaponIcon").GetComponent<SpriteRenderer>();
        SpriteRenderer background = transform.Find("Background").GetComponent<SpriteRenderer>();
        weaponIcon.sprite = weaponComponent.sprite;
        background.color = WeaponIndicator.rarityColors[weaponComponent.rarity];
    }

    void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        { 
            WeaponsManager weaponsManager = collision.gameObject.GetComponent<WeaponsManager>();
            weaponsManager.EquipWeapon(weapon);
            // Set the weapon item to (0.9,0.9, 0.9) for 0.1 seconds, then destroy it
            SpriteRenderer sr = gameObject.transform.Find("Background").GetComponent<SpriteRenderer>();
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.9f);
            Destroy(gameObject, 0.1f);
        }
    }
}
