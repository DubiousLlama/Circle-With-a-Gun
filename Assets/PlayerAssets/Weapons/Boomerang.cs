using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : Weapon
{
    private int damage;
    private float speed;
    private float  range = 1.25f;

    private GameObject boomerangPrefab;

    public void Awake()
    {
        isAutomatic = true;
        weaponType = WeaponType.Primary;
        SetRarity(WeaponRarity.Common);
    }

    private Dictionary<WeaponRarity, float> cooldowns = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 0.75f },
        { WeaponRarity.Rare, 0.6f },
        { WeaponRarity.Uncommon, 0.7f },
        { WeaponRarity.Common, 0.8f }
    };

    private Dictionary<WeaponRarity, int> r_damage = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Legendary, 100 },
        { WeaponRarity.Rare, 50 },
        { WeaponRarity.Uncommon, 50 },
        { WeaponRarity.Common, 40 }
    };

    private Dictionary<WeaponRarity, float> r_speed = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 20f },
        { WeaponRarity.Rare, 11f },
        { WeaponRarity.Uncommon, 9.5f },
        { WeaponRarity.Common, 8f }
    };

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
        cooldown = cooldowns[rarity];
        damage = r_damage[rarity];
        speed = r_speed[rarity];
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

    public override void Fire()
    {
        GameObject boomerang = Instantiate(boomerangPrefab, firePoint.position, firePoint.rotation);
        BoomerScript bs = boomerang.GetComponent<BoomerScript>();
        
        bs.Launch(firePoint.up, speed);

        bs.damage = damage;
        bs.lifetime = range;
    }
}
