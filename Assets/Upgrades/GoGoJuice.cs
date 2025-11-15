using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoGoJuice : MonoBehaviour
{
    [SerializeField]
    private float speedIncreasePercentage = 0.3f;

    [SerializeField]
    private float secondsWithoutWeapon = 3f;

    private float timeSinceLastWeaponUse = 0f;
    private bool isBoosted = false;

    void Start()
    {
        Weapon.OnWeaponUsed += OnWeaponUsed;
    }

    void Update()
    {
        timeSinceLastWeaponUse += Time.deltaTime;

        if (!isBoosted && timeSinceLastWeaponUse >= secondsWithoutWeapon)
        {
            PlayerStats.instance.ModifyMultStat(StatTypes.MoveSpeed, speedIncreasePercentage, true, tag: "GoGoJuice");
            isBoosted = true;
        }
    }

    private void OnWeaponUsed(Weapon.OnWeaponUsedArgs args)
    {
        timeSinceLastWeaponUse = 0f;

        if (isBoosted)
        {
            PlayerStats.instance.RemoveAllModifiersWithTag("GoGoJuice");
            isBoosted = false;
        }
    }

    void OnDestroy()
    {
        Weapon.OnWeaponUsed -= OnWeaponUsed;
        if (isBoosted)
        {
            PlayerStats.instance.RemoveAllModifiersWithTag("GoGoJuice");
        }
    }
}
