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

    // Start is called before the first frame update
    void Start()
    {
        BoomerScript.BoomerHit += (e) => ApplyPoision(e.eh);
        BulletScript.BulletHit += (e) => ApplyPoision(e.eh);
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
        if (eh != null && (Random.value < poisonChance))
        {
            eh.ApplyPoison(poisonDuration);
        }
    }
}
