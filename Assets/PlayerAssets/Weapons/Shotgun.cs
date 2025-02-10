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

    public new void Awake()
    {
        base.Awake();

        // Modify base class variables as needed
        cooldown = 0.38f;
        isAutomatic = true;
        weaponType = WeaponType.Primary;
    }

    public new void Start()
    {
        base.Start();

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
