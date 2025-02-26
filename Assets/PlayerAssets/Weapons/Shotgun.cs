using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class Shotgun : Weapon
{
    int damage = 35;
    int numBullets;
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

    private Dictionary<WeaponRarity, float> cooldowns = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 0.2f },
        { WeaponRarity.Rare, 0.3f },
        { WeaponRarity.Uncommon, 0.4f },
        { WeaponRarity.Common, 0.5f }
    };

    private Dictionary<WeaponRarity, int> numBulletss = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Legendary, 11 },
        { WeaponRarity.Rare, 9 },
        { WeaponRarity.Uncommon, 7 },
        { WeaponRarity.Common, 5 }
    };

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
        cooldown = cooldowns[rarity];
        numBullets = numBulletss[rarity];
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

    protected override void Fire()
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
        bs.damage = (int)(damage * playerStats.Damage());

        Destroy(bullet, rangeLife);

        rb.AddForce(bullet.transform.up * bulletForce, ForceMode2D.Impulse);
    }
}
