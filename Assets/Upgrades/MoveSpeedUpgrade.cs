using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpeedUpgrade : MonoBehaviour
{
    public float moveSpeedIncrease = 0.1f;

    void Start()
    {
        PlayerStats.instance.ModifyMultStat(StatTypes.MoveSpeed, moveSpeedIncrease, true);
    }
}
