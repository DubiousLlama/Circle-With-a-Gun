using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBounceUpgrade : MonoBehaviour
{
    private BulletWeapon bulletWeapon;

    void Start()
    {
        Weapon weapon = gameObject.GetComponentInParent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary);
        if (weapon is BulletWeapon bw)
        {
            bulletWeapon = bw;
            bulletWeapon.wallBounce = true;
        }
        else
        {
            Debug.LogError("WallBounceUpgrade can only be applied to BulletWeapon types.");
        }
    }

    void OnDestroy()
    {
        if (bulletWeapon != null)
        {
            bulletWeapon.wallBounce = false;
        }
    }
}
