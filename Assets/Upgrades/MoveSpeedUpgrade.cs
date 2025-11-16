using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpeedUpgrade : MonoBehaviour
{
    public float moveSpeedIncrease = 0.1f;
    private Guid modifierGuid;

    void Start()
    {
        modifierGuid = PlayerStats.instance.ModifyMultStat(StatTypes.MoveSpeed, moveSpeedIncrease, true);
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveMultStatModifier(modifierGuid);
    }
}
