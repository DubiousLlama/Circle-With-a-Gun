using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BulletWeapon : Weapon
{
    [Header("Bullet Settings")]
    public int damage = 20;
    
    [Range(10, 40)]
    public float bulletForce = 20f;
    
    public GameObject bulletPrefab;
    
    protected string sfx = "Gun";

    public bool wallBounce = false;

    protected virtual void Awake()
    {
        weaponType = WeaponType.Primary;
        SetRarity(WeaponRarity.Common);
    }

    public override void Equip()
    {
        base.Equip();
        
        if (bulletPrefab == null)
        {
            LoadBulletPrefab();
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab not found for " + gameObject.name);
        }
    }

    /// <summary>
    /// Virtual method to load the bullet prefab. Override if weapon needs special bullet loading logic.
    /// </summary>
    protected virtual void LoadBulletPrefab()
    {
        // Default implementation - weapons can override this if they need different bullet loading
        if (bulletPrefab == null)
        {
            bulletPrefab = Resources.Load<GameObject>("Bullet");
        }
    }

    /// <summary>
    /// Creates and configures a bullet instance with common properties
    /// </summary>
    /// <param name="position">Position to spawn the bullet</param>
    /// <param name="rotation">Rotation of the bullet</param>
    /// <returns>The configured bullet GameObject</returns>
    protected virtual GameObject CreateBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        ConfigureBullet(bullet);
        return bullet;
    }

    /// <summary>
    /// Configures a bullet with standard properties (damage, pierce, physics)
    /// </summary>
    /// <param name="bullet">The bullet GameObject to configure</param>
    protected virtual void ConfigureBullet(GameObject bullet)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        BulletScript bs = bullet.GetComponent<BulletScript>();

        if (bs != null)
        {
            bs.damage = damage;

            if (pierceCount > 0)
            {
                bs.pierceCount = pierceCount;
            }

            bs.wallBounce = wallBounce;
            Debug.Log("Configured bullet with damage: " + damage + ", pierceCount: " + bs.pierceCount + ", wallBounce: " + wallBounce);
        } else
        {
            Debug.LogWarning("BulletScript component not found on bullet prefab.");
        }



        if (rb != null)
        {
            rb.AddForce(bullet.transform.up * bulletForce * rb.mass, ForceMode2D.Impulse);
        }
    }

    /// <summary>
    /// Plays the weapon's sound effect
    /// </summary>
    protected virtual void PlayFireSound()
    {
        audioManager.PlaySfx(sfx);
    }

    public override void Fire()
    {
        FireBullets();
        PlayFireSound();
    }

    /// <summary>
    /// Abstract method that must be implemented by child classes to define their specific firing pattern
    /// </summary>
    protected abstract void FireBullets();
}
