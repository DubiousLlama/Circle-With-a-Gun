using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthIncreaseUpgrade : MonoBehaviour
{
    float healthIncrease = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
       PlayerStats.instance.ModifyMultStat(StatTypes.MaxHealth, healthIncrease, true);
    }
}
