using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Berserk : MonoBehaviour
{
    public float percentageThreshold = 0.3f;

    PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth.GetHealthPercentage() <= percentageThreshold)
        {
            if (!PlayerStats.instance.DoesTagExist("Berserk"))
            {
                PlayerStats.instance.ModifyMultStat(StatTypes.AttackSpeed, 0.66f, true, tag: "Berserk");
                PlayerStats.instance.ModifyMultStat(StatTypes.MoveSpeed, 0.5f, true, tag: "Berserk");
            }
        }
        else
        {
            PlayerStats.instance.RemoveAllModifiersWithTag("Berserk");
        }
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveAllModifiersWithTag("Berserk");
    }
}
