using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeedUpgrade : MonoBehaviour
{
    public float attackSpeedIncrease = 0.2f;

    void Start()
    {
        PlayerStats.instance.ModifyMultStat(StatTypes.AttackSpeed, attackSpeedIncrease, true);
    }
}
