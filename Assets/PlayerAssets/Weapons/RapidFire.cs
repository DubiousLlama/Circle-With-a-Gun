using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class RapidFire : BulletWeapon
{
    protected override void Awake()
    {
        isAutomatic = true;
        base.Awake();
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
        Debug.Log("Weapon equipped: " + gameObject.name);
    }

    protected override void FireBullets()
    {
        CreateBullet(firePoint.position, firePoint.rotation);
    }
}
