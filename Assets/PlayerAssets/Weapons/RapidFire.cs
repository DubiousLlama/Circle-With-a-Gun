using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class RapidFire : Weapon
{
    public int damage = 20;

    [Range(10, 40)]
    public float bulletForce = 20f;

    private GameObject bulletPrefab;
    private string sfx = "Gun";
    private Color bulletColor = new Color(0.561111f, 0f, 1f, 1f);

    public void Awake()
    {
        cooldown = 0.1f;
        weaponType = WeaponType.Primary;
    }

    public override void Equip()
    {
        base.Equip();

        bulletPrefab = Resources.Load<GameObject>("Bullet");

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not found");
        }

        Debug.Log("Weapon equipped: " + gameObject.name);
    }

    protected override void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        BulletScript bs = bullet.GetComponent<BulletScript>();
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        sr.color = bulletColor;
        bs.damage = (int)(damage * playerStats.Damage());

        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);

        audioManager.PlaySfx(sfx);
    }
}
