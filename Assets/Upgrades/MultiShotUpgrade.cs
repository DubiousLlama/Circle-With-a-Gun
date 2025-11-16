using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiShotUpgrade : MonoBehaviour
{
    private Weapon primaryWeapon;

    void Start()
    {
        primaryWeapon = transform.parent.GetComponent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary);
        if (primaryWeapon == null)
        {
            Debug.LogError("No primary weapon equipped for MultiShotUpgrade");
        }
        else
        {
            primaryWeapon.burstMode = true;
        }
    }

    void OnDestroy()
    {
        if (primaryWeapon != null)
        {
            primaryWeapon.burstMode = false;
        }
    }
}
