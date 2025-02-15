using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEDamage : MonoBehaviour
{

    public int damagePerSecond = 100;
    public float slowAmount = 0.9f;

    // While the player is in the area of effect, damage them
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<PlayerHealth>().Damage(damagePerSecond * Time.deltaTime);

                // Slow the player while they are in the area of effect
                string slowID = "OctoSlow";
                float slowDuration = 0.25f;
                collision.gameObject.GetComponent<PlayerMovement>().Slow(slowAmount, slowID, slowDuration);
            }
        }
    }
}
