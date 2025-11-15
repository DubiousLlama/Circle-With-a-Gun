using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Shotgun : BulletWeapon
{
    [Header("Shotgun Settings")]
    public int numBullets = 5;
    public int spread = 40;
    public float rangeLife = 0.25f;
    public Color bulletColor = new Color(1f, 0.5569f, 0f, 1f);

    protected override void Awake()
    {
        base.Awake();
    }


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
        isAutomatic = true;
    }

    protected override void LoadBulletPrefab()
    {
        bulletPrefab = Resources.Load<GameObject>("Bullet");
    }

    protected override void FireBullets(Transform firePoint, int i)
    {
        float spreadAngleChange = spread / (numBullets-1);
        float startAngle = -spread / 2;

        for (int n = 0; n < numBullets; n++)
        {
            FireSingleBullet(startAngle + spreadAngleChange * n, firePoint, i);
        }
    }

    private void FireSingleBullet(float rot, Transform firePoint, int i)
    {
        // The bullet angle is the rotation of the firepoint modified by the "rot" parameter
        Quaternion bulletRotation = firePoint.rotation * Quaternion.Euler(0, 0, rot);

        GameObject bullet = CreateBullet(firePoint.position, bulletRotation, i);
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.color = bulletColor;
        }

        Destroy(bullet, rangeLife);
    }
}
