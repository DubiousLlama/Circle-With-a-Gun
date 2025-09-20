using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public class Sniper : BulletWeapon
{
    [Header("Sniper Settings")]
    public int bonusDamagePerSecond = 200;

    protected override void Awake()
    {
        // Set default values for sniper - these can still be overridden in inspector
        if (bulletForce == 20f) bulletForce = 40f; // Only set if still at default
        
        isAutomatic = true;
        base.Awake();
    }

    public override string getDisplayName()
    {
        return "Sniper";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Primary;
    }

    public override void Equip()
    {
        base.Equip();
        Debug.Log("Weapon equipped: " + gameObject.name);
    }

    protected override void ConfigureBullet(GameObject bullet)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        SniperBulletScript bs = bullet.GetComponent<SniperBulletScript>();

        if (bs != null)
        {
            bs.damage = damage;
            bs.bonusDamagePerSecond = bonusDamagePerSecond;

            if (pierceCount > 0)
            {
                bs.pierceCount = pierceCount;
                bs.doesPierce = true;
            }
        }

        if (rb != null)
        {
            rb.AddForce(bullet.transform.up * bulletForce, ForceMode2D.Impulse);
        }
    }

    protected override void FireBullets()
    {
        CreateBullet(firePoint.position, firePoint.rotation);
    }
}
