using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Shotgun : Weapon
{
    int damage = 35;
    int numBullets = 5;
    int spread = 40;
    float rangeLife = 0.25f;

    [Range(10, 40)]
    float bulletForce = 25f;

    private GameObject bulletPrefab;
    private string sfx = "Gun";
    private Color bulletColor = new Color(1f, 0.5569f, 0f, 1f);

    public void Awake()
    {
        weaponType = WeaponType.Primary;
        SetRarity(WeaponRarity.Common);
    }

    private Dictionary<WeaponRarity, int> r_damage = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Rare, 25 },
        { WeaponRarity.Uncommon, 25 },
        { WeaponRarity.Common, 25 }
    };

    private Dictionary<WeaponRarity, int> r_spread = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Rare, 35 },
        { WeaponRarity.Uncommon, 45 },
        { WeaponRarity.Common, 60 }
    };

    private Dictionary<WeaponRarity, int> r_numbullets = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Rare, 9 },
        { WeaponRarity.Uncommon, 7 },
        { WeaponRarity.Common, 5 }
    };

    public override string getDisplayName()
    {
        return "Shotgun";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Primary;
    }

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
        spread = r_spread[rarity];
        damage = r_damage[rarity];
        numBullets = r_numbullets[rarity];
        cooldown = 0.38f;
        isAutomatic = true;
    }

    public override void Equip()
    {
        base.Equip();

        bulletPrefab = Resources.Load<GameObject>("Bullet");

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not found");
        }
    }

    public override void Fire()
    {
        float spreadAngleChange = spread / (numBullets-1);
        float startAngle = -spread / 2;

        /* if (numBullets % 2 == 1)
        {
            FireOne(spreadAngle * numBullets / 2);
        } */

        for (int i = 0; i < numBullets; i++)
        {
            FireOne(startAngle + spreadAngleChange * i);
        }

        audioManager.PlaySfx(sfx);
        
    }

    private void FireOne(float rot)
    {
        // The bullet angle is the rotation of the firepoint modified by the "rot" parameter
        Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, rot);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, bulletRotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        BulletScript bs = bullet.GetComponent<BulletScript>();
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        sr.color = bulletColor;
        bs.damage = damage;

        Destroy(bullet, rangeLife);

        rb.AddForce(bullet.transform.up * bulletForce, ForceMode2D.Impulse);
    }
}
