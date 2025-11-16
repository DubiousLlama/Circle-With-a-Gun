using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerEffect : MonoBehaviour
{
    GameObject pc;
    float timer = 0f;
    float healRate = 0f;
    float attackBoost = 0f;
    float effectRadius = 9.59f;

    float healIncrement = (0.05f);
    WaitForSeconds wait;
    PlayerHealth ph;

    private void Start()
    {
        pc = GameObject.Find("PC");
        ph = pc.GetComponent<PlayerHealth>();
        wait = new(healIncrement);
    }

    public void ActivateBannerEffect(float timer, float healRate, float attackBoost)
    {
        this.timer = timer;
        this.healRate = healRate;
        this.attackBoost = attackBoost;

        StartCoroutine(HealBursts(timer));
    }

    private void OnDestroy()
    {
        PlayerStats.instance.RemoveAllModifiersWithTag("Banner");
    }

    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;

        if (timer <= 0)
        {
            Destroy(gameObject.transform.parent.gameObject);
        }

        // Rotate the transform slowly
        transform.Rotate(0f, 0f, 20f * Time.fixedDeltaTime);

        if (pc != null)
        {
            // Get the Distance between pc and this object
            float distance = Vector2.Distance(pc.transform.position, transform.position);

            if (distance <= effectRadius)
            {
                if (!PlayerStats.instance.DoesTagExist("Banner")) {
                    PlayerStats.instance.ModifyMultStat(StatTypes.AttackSpeed, attackBoost, true, tag: "Banner");
                }
            } else
            {
                PlayerStats.instance.RemoveAllModifiersWithTag("Banner");
            }
        }
    }

    IEnumerator HealBursts(float duration)
    {
        yield return wait;

        float timeElapsed = 0f;
        while(timeElapsed < duration)
        {
            float distance = Vector2.Distance(pc.transform.position, transform.position);
            if (distance < effectRadius)
            {
                ph.Heal(Mathf.RoundToInt(healRate * healIncrement), false);
            }

            timeElapsed += healIncrement;
            yield return wait;
        }
    }
}
