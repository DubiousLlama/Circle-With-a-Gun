using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;


public class Sniper : Weapon
{
    public int damage = 20;
    public int bonusDamagePerSecond = 200;

    public float bulletForce = 40f;

    public GameObject bulletPrefab;
    private string sfx = "Gun";

    public void Awake()
    {
        isAutomatic = true;
        weaponType = WeaponType.Primary;
        SetRarity(WeaponRarity.Common);
    }

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
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
        SniperBulletScript bs = bullet.GetComponent<SniperBulletScript>();

        bs.damage = damage;
        bs.bonusDamagePerSecond = bonusDamagePerSecond;

        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);

        audioManager.PlaySfx(sfx);
    }
}
