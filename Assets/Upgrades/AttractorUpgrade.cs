using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractorUpgrade : MonoBehaviour
{
    public float radiusIncrease = 0.7f;

    void Start()
    {
        PlayerStats.instance.ModifyMultStat(StatTypes.AttractorRadius, radiusIncrease, true, tag: "AttractorUpgrade");
    }

    void OnDestroy()
    {
        PlayerStats.instance.RemoveAllModifiersWithTag("AttractorUpgrade");
    }
}
