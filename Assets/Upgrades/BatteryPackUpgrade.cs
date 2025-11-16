using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryPackUpgrade : MonoBehaviour
{
    private Guid modifierGuid;

    // Start is called before the first frame update
    void Start()
    {
        modifierGuid = PlayerStats.instance.ModifyMultStat(StatTypes.LegendaryDuration, 1, true);
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveMultStatModifier(modifierGuid);
    }
}
