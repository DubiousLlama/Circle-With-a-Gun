using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class BombSecondary : Weapon
{
    public int damage;
    public float blastRadius;
    public float cooldownConfig;
    public float moveSpeed = 5f;

    public GameObject bombProjectilePrefab;

    public void Awake()
    {
        cooldown = cooldownConfig;
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
        // get a random direction within 8 degrees of the player's aim direction
        Vector3 aimDirection = firePoint.up;
        //float angleOffset = Random.Range(-8f, 8f);

        //Vector3 rotatedDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * aimDirection;

        // Create the bomb projectile
        GameObject bomb = Instantiate(bombProjectilePrefab, firePoint.position, Quaternion.identity);
        BombLogic bl = bomb.GetComponent<BombLogic>();
        bl.InitalizeBomb(aimDirection, damage, blastRadius, moveSpeed * 100, Random.Range(350, 420), blastRadius);
    }
}
