using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryHealUpgrade : MonoBehaviour
{
    public int healAmount = 150;
    PlayerHealth playerHealth;
    // Start is called before the first frame update
    void Start()
    {
        PlayerHealth playerHealth = GameObject.Find("PC").GetComponent<PlayerHealth>();
        Weapon.OnWeaponUsed += (e) => { Heal(e.weaponType); };
    }

    // Update is called once per frame
    void Heal(WeaponType wt)
    {
        if (wt == WeaponType.Secondary)
        {
            playerHealth.Heal(150, false);
        }
    }
}
