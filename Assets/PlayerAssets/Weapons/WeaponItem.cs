using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WeaponItem : MonoBehaviour
{
    public GameObject weapon;

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

    public void MakeSpecial()
    {
        Weapon weaponComponent = weapon.GetComponent<Weapon>();
        weaponComponent.SetRarity(WeaponRarity.Legendary);
    }
}
