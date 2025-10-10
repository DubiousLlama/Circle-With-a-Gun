using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryPackUpgrade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PlayerStats.instance.ModifyMultStat(StatTypes.LegendaryDuration, 1, true);
    }

}
