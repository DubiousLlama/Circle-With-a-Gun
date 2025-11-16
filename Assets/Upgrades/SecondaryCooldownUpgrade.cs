using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryCooldownUpgrade : MonoBehaviour
{
    float cooldownReduction = 0.3f;
    private Guid modifierGuid;

    void Start()
    {
        modifierGuid = PlayerStats.instance.ModifyMultStat(StatTypes.SecondaryCooldown, cooldownReduction, true);
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveMultStatModifier(modifierGuid);
    }
}
