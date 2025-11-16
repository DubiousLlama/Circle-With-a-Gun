using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezingStrikes : MonoBehaviour
{
    public float freezeDuration = 3.0f;

    [Header("freeze Chances")]
    public float freezeChanceBoomerang = 0.08f;
    public float freezeChanceNormal = 0.1f;
    public float freezeChanceShotgun = 0.15f;
    public float freezeChanceSniper = 1f;

    private float freezeChance = 0.1f;
    private Action<BoomerScript.OnBoomerHitEventArgs> boomerHandler;
    private Action<BulletScript.OnBulletHitEventArgs> bulletHandler;

    // Start is called before the first frame update
    void Start()
    {
        boomerHandler = (e) => ApplyFreeze(e.eh);
        bulletHandler = (e) => ApplyFreeze(e.eh);
        
        BoomerScript.BoomerHit += boomerHandler;
        BulletScript.BulletHit += bulletHandler;
        PlayerStats.instance.freezingStrikes = true;

        switch (PlayerStats.instance.primaryWeaponType)
        {
            case PrimaryWeaponType.Boomerang:
                freezeChance = freezeChanceBoomerang;
                break;
            case PrimaryWeaponType.Shotgun:
                freezeChance = freezeChanceShotgun;
                break;
            case PrimaryWeaponType.Sniper:
                freezeChance = freezeChanceSniper;
                break;
            case PrimaryWeaponType.RapidFire:
                freezeChance = freezeChanceNormal;
                break;
            default:
                break;
        }
    }

    void ApplyFreeze(EnemyHealth eh)
    {
        if (eh != null && (UnityEngine.Random.value < freezeChance) && PlayerStats.instance.freezingStrikes)
        {
            eh.ApplyFreeze(freezeDuration);
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
        PlayerStats.instance.freezingStrikes = false;
    }
}
