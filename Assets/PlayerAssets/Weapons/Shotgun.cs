using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Shotgun : BulletWeapon
{
    [Header("Shotgun Settings")]
    int numBullets = 5;
    int spread = 40;
    float rangeLife = 0.25f;
    private Color bulletColor = new Color(1f, 0.5569f, 0f, 1f);

    protected override void Awake()
    {
        // Set default values for shotgun - these can still be overridden in inspector
        if (damage == 20) damage = 35; // Only set if still at default
        if (bulletForce == 20f) bulletForce = 25f; // Only set if still at default
        
        base.Awake();
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

    protected override void LoadBulletPrefab()
    {
        bulletPrefab = Resources.Load<GameObject>("Bullet");
    }

    protected override void FireBullets()
    {
        float spreadAngleChange = spread / (numBullets-1);
        float startAngle = -spread / 2;

        for (int i = 0; i < numBullets; i++)
        {
            FireSingleBullet(startAngle + spreadAngleChange * i);
        }
    }

    private void FireSingleBullet(float rot)
    {
        // The bullet angle is the rotation of the firepoint modified by the "rot" parameter
        Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, rot);

        GameObject bullet = CreateBullet(firePoint.position, bulletRotation);
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.color = bulletColor;
        }

        Destroy(bullet, rangeLife);
    }
}
