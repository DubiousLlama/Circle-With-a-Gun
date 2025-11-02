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

        if (timer >= 0.10f)
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        if (timer >= 0.14f)
        {
            hitEffect.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        }
    }

    protected override int CalculateDamage()
    {
        Debug.Log("Sniper bullet timer: " + timer);
        return damage + Mathf.RoundToInt(timer * bonusDamagePerSecond);
    }
}