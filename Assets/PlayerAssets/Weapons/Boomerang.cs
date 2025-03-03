using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : Weapon
{
    private int damage;
    private float speed;

    private GameObject boomerangPrefab;

    public void Awake()
    {
        isAutomatic = true;
        weaponType = WeaponType.Primary;
        SetRarity(WeaponRarity.Common);
    }

    private Dictionary<WeaponRarity, float> cooldowns = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 0.25f },
        { WeaponRarity.Rare, 0.35f },
        { WeaponRarity.Uncommon, 0.45f },
        { WeaponRarity.Common, 0.5f }
    };

    private Dictionary<WeaponRarity, int> r_damage = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Legendary, 100 },
        { WeaponRarity.Rare, 70 },
        { WeaponRarity.Uncommon, 40 },
        { WeaponRarity.Common, 30 }
    };

    private Dictionary<WeaponRarity, float> r_speed = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 20f },
        { WeaponRarity.Rare, 18f },
        { WeaponRarity.Uncommon, 15f },
        { WeaponRarity.Common, 10f }
    };

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
        cooldown = cooldowns[rarity];

    }

    public override string getDisplayName()
    {
        return "Boomerang";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Primary;
    }

    public override void Equip()
    {
        base.Equip();

        boomerangPrefab = Resources.Load<GameObject>("Boomerang");

        if (boomerangPrefab == null)
        {
            Debug.LogError("boomerangPrefab prefab not found");
        }

        Debug.Log("Weapon equipped: " + gameObject.name);
    }

    protected override void Fire()
    {
        GameObject boomerang = Instantiate(boomerangPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = boomerang.GetComponent<Rigidbody2D>();
        BulletScript bs = boomerang.GetComponent<BoomerScript>();
        SpriteRenderer sr = boomerang.GetComponent<SpriteRenderer>();

        bs.damage = damage;

        rb.AddForce(firePoint.up * speed, ForceMode2D.Impulse);
    }
}
