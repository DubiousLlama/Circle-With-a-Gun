using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEDamage : MonoBehaviour
{

    public int damagePerSecond = 100;
    public float slowAmount = 0.9f;

    private Guid slowTag = Guid.Empty;

    // While the player is in the area of effect, damage them
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<PlayerHealth>().Damage(damagePerSecond * Time.deltaTime);

                if (PlayerStats.instance.DoesTagExist("OctoSlow")) { return; }

                if (PlayerStats.instance.DoesGuidExist(slowTag))
                {
                    PlayerStats.instance.SetModifierDuration(slowTag, 0.25f);
                } else
                {
                    slowTag = PlayerStats.instance.ModifyMultStat(StatTypes.MoveSpeed, -1f * slowAmount, false, 0.25f, "OctoSlow");
                }
            }
        }
    }
}
