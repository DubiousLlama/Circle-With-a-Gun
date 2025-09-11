using UnityEngine;

public class MissileLauncher : Weapon
{
    public int damage;
    public float blastRadius;
    public float moveSpeed = 5f;

    public GameObject missileProjectilePrefab;

    public void Awake()
    {
        weaponType = getFinalType();
    }

    public override void Equip()
    {
        base.Equip();
    }

    public override string getDisplayName()
    {
        return "Bomb Launcher";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Secondary;
    }

    public override void Fire()
    {
        Vector3 aimDirection = firePoint.up;

        // Create the bomb projectile
        GameObject missile = Instantiate(missileProjectilePrefab, firePoint.position, firePoint.rotation);
        MissileLogic ml = missile.GetComponent<MissileLogic>();
        ml.InitalizeMissile(dmg: damage, radius: blastRadius, move: moveSpeed, homing: true);
    }
}
