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
        playerHealth = GameObject.Find("PC").GetComponent<PlayerHealth>();
        Weapon.OnWeaponUsed += (e) => { Heal(e.weaponType); };
    }

    void Heal(WeaponType wt)
    {
        if (wt == WeaponType.Secondary)
        {
            Debug.Log("SecondaryHealUpgrade received secondary weapon type");
            playerHealth.Heal(150, false);
        } else
        {
            Debug.Log("SecondaryHealUpgrade received non-secondary weapon type");
        }
    }
}
