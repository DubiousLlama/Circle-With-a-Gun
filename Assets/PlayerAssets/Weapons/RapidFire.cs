using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class RapidFire : Weapon
{
    public int damage = 20;

    [Range(10, 40)]
    public float bulletForce = 20f;

    public GameObject bulletPrefab;
    private string sfx = "Gun";

    public void Awake()
    {
        isAutomatic = true;
        weaponType = WeaponType.Primary;
        SetRarity(WeaponRarity.Common);
    }

    private Dictionary<WeaponRarity, float> cooldowns = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 0.025f },
        { WeaponRarity.Rare, 0.05f },
        { WeaponRarity.Uncommon, 0.075f },
        { WeaponRarity.Common, 0.1f }
    };

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
        cooldown = cooldowns[rarity];
    }

    public override string getDisplayName()
    {
        return "Gun";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Primary;
    }

    public override void Equip()
    {
        base.Equip();

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not found");
        }

        Debug.Log("Weapon equipped: " + gameObject.name);
    }

    public override void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        BulletScript bs = bullet.GetComponent<BulletScript>();

        bs.damage = damage;

        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);

        audioManager.PlaySfx(sfx);
    }
}
