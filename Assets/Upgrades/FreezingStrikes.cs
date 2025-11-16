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

    // Start is called before the first frame update
    void Start()
    {
        BoomerScript.BoomerHit += (e) => ApplyFreeze(e.eh);
        BulletScript.BulletHit += (e) => ApplyFreeze(e.eh);
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
        if (eh != null && (Random.value < freezeChance))
        {
            eh.ApplyFreeze(freezeDuration);
        }
    }
}
