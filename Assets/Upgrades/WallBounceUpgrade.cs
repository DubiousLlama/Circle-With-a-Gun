using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBounceUpgrade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Weapon weapon = gameObject.GetComponentInParent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary);
        if (weapon is BulletWeapon bulletWeapon)
        {
            bulletWeapon.wallBounce = true;
        } else
        {
            Debug.LogError("WallBounceUpgrade can only be applied to BulletWeapon types.");
        }
    }
}
