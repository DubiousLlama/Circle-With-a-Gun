using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarHorn : MonoBehaviour
{
    [SerializeField]
    private float attackSpeedBoost = 0.8f;

    [SerializeField]
    private float boostDuration = 2f;

    void Start()
    {
        Weapon.OnWeaponUsed += OnWeaponUsed;
    }

    void OnDestroy()
    {
        Weapon.OnWeaponUsed -= OnWeaponUsed;
        if (PlayerStats.instance.DoesTagExist("WarHorn"))
        {
            PlayerStats.instance.RemoveAllModifiersWithTag("WarHorn");
        }
    }

    private void OnWeaponUsed(Weapon.OnWeaponUsedArgs args)
    {
        if (args.weaponType == WeaponType.Secondary)
        {
            PlayerStats.instance.RemoveAllModifiersWithTag("WarHorn");
            PlayerStats.instance.ModifyMultStat(StatTypes.AttackSpeed, attackSpeedBoost, false, boostDuration, "WarHorn");
        }
    }
}
