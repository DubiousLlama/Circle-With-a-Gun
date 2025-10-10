using UnityEngine;


public class Sniper : BulletWeapon
{
    [Header("Sniper Settings")]
    public int bonusDamagePerSecond = 200;

    protected override void Awake()
    {
        // Set default values for sniper - these can still be overridden in inspector
        if (bulletForce == 20f) bulletForce = 40f; // Only set if still at default
        
        isAutomatic = true;
        base.Awake();
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
        Debug.Log("Weapon equipped: " + gameObject.name);
    }

    protected override void ConfigureBullet(GameObject bullet)
    {
        base.ConfigureBullet(bullet);

        SniperBulletScript bs = bullet.GetComponent<SniperBulletScript>();
        bs.bonusDamagePerSecond = bonusDamagePerSecond;
    }

    protected override void FireBullets()
    {
        CreateBullet(firePoint.position, firePoint.rotation);
    }
}
