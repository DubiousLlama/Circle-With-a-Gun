using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonStrikes : MonoBehaviour
{
    public float poisonDuration = 3.0f;

    [Header("Poison Chances")]
    public float poisonChanceBoomerang = 0.08f;
    public float poisonChanceNormal = 0.1f;
    public float poisonChanceShotgun = 0.15f;
    public float poisonChanceSniper = 1f;

    private float poisonChance = 0.1f;
    private Action<BoomerScript.OnBoomerHitEventArgs> boomerHandler;
    private Action<BulletScript.OnBulletHitEventArgs> bulletHandler;

    void Start()
    {
        boomerHandler = (e) => ApplyPoision(e.eh);
        bulletHandler = (e) => ApplyPoision(e.eh);
        
        BoomerScript.BoomerHit += boomerHandler;
        BulletScript.BulletHit += bulletHandler;
        PlayerStats.instance.poisonedStrikes = true;

        switch (PlayerStats.instance.primaryWeaponType)
        {
            case PrimaryWeaponType.Boomerang:
                poisonChance = poisonChanceBoomerang;
                break;
            case PrimaryWeaponType.Shotgun:
                poisonChance = poisonChanceShotgun;
                break;
            case PrimaryWeaponType.Sniper:
                poisonChance = poisonChanceSniper;
                break;
            case PrimaryWeaponType.RapidFire:
                poisonChance = poisonChanceNormal;
                break;
            default:
                break;
        }
    }

    void ApplyPoision(EnemyHealth eh)
    {
        if (eh != null && (UnityEngine.Random.value < poisonChance) && PlayerStats.instance.poisonedStrikes)
        {
            eh.ApplyPoison(poisonDuration);
        }
    }

    void OnDestroy()
    {
        if (boomerHandler != null)
        {
            BoomerScript.BoomerHit -= boomerHandler;
        }
        if (bulletHandler != null)
        {
            BulletScript.BulletHit -= bulletHandler;
        }
        PlayerStats.instance.poisonedStrikes = false;
    }
}
