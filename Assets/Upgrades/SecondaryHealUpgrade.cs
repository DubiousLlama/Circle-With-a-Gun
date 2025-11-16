using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryHealUpgrade : MonoBehaviour
{
    public int healAmount = 150;
    PlayerHealth playerHealth;
    private Action<Weapon.OnWeaponUsedArgs> weaponHandler;

    void Start()
    {
        playerHealth = GameObject.Find("PC").GetComponent<PlayerHealth>();
        weaponHandler = (e) => { Heal(e.weaponType); };
        Weapon.OnWeaponUsed += weaponHandler;
    }

    void Heal(WeaponType wt)
    {
        if (wt == WeaponType.Secondary)
        {
            Debug.Log("SecondaryHealUpgrade received secondary weapon type");
            playerHealth.Heal(150, false);
        }
        else
        {
            Debug.Log("SecondaryHealUpgrade received non-secondary weapon type");
        }
    }

    void OnDestroy()
    {
        if (weaponHandler != null)
        {
            Weapon.OnWeaponUsed -= weaponHandler;
        }
    }
}
