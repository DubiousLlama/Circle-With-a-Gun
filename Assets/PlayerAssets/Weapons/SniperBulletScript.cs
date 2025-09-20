using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBulletScript : BulletScript
{
    public int bonusDamagePerSecond = 0;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
    }

    protected override int CalculateDamage()
    {
        return damage + Mathf.RoundToInt(timer * bonusDamagePerSecond);
    }
}