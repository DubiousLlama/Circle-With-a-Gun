using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIncreaseUpgrade : MonoBehaviour
{
    float healthIncrease = 0.5f;
    private Guid modifierGuid;

    void Start()
    {
       modifierGuid = PlayerStats.instance.ModifyMultStat(StatTypes.MaxHealth, healthIncrease, true);
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveMultStatModifier(modifierGuid);
    }
}
