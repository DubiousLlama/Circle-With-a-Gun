using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeedUpgrade : MonoBehaviour
{
    public float attackSpeedIncrease = 0.2f;
    private Guid modifierGuid;

    void Start()
    {
        modifierGuid = PlayerStats.instance.ModifyMultStat(StatTypes.AttackSpeed, attackSpeedIncrease, true);
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveMultStatModifier(modifierGuid);
    }
}
