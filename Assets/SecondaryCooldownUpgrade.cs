using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryCooldownIUpgrade : MonoBehaviour
{

    float cooldownReduction = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
        PlayerStats.instance.ModifyMultStat(StatTypes.SecondaryCooldown, cooldownReduction, true);
    }
}
