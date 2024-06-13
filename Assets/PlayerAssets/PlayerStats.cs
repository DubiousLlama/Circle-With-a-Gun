using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    float maxHealth = 1f;
    float regenRate = 1f;
    float damage = 1f;
    float speed = 1f;
    float explosionRadius = 1f;
    float explosionDamage = 1f;

    public float regenMod = 1f;
    public float damageMod = 1f;
    public float speedMod = 1f;
    public float explosionRadiusMod = 1f;
    public float explosionDamageMod = 1f;

    public float MaxHealth()
    {
        return maxHealth;
    }

    public float RegenRate()
    {
        return regenRate * regenMod;
    }

    public float Damage()
    {
        return damage * damageMod;
    }

    public float Speed()
    {
        return speed * speedMod;
    }

    public float ExplosionRadius()
    {
        return explosionRadius * explosionRadiusMod;
    }

    public float ExplosionDamage()
    {
        return explosionDamage * explosionDamageMod;
    }

    public void ImproveHealth()
    {
        if (maxHealth < 1.5f)
        {
            maxHealth += 0.05f;
        }
        else
        {
            speed = 0.1f;
        }
    }

    public void ImproveSpeed()
    {
        if (speed > 1.25f)
        {
            speed += 0.01f;
        }
        else
        {
            speed += 0.05f;
        }
    }

    public void ImproveDamage()
    {
        damage += 0.05f;
    }

    public void ImproveExplosions()
    {
        explosionDamage += 0.05f;
        explosionRadius += 0.025f;
    }
}
