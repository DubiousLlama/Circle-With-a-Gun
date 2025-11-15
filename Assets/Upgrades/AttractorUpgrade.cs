using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractorUpgrade : MonoBehaviour
{
    public float radiusIncrease = 0.7f;
    // Start is called before the first frame update
    void Start()
    {
        PlayerStats.instance.ModifyMultStat(StatTypes.AttractorRadius, radiusIncrease, true, tag: "AttractorUpgrade");
    }
}
