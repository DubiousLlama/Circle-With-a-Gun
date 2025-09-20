using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiShotUpgrade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.parent.GetComponent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary).burstMode = true;
        if (transform.parent.GetComponent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary) == null) {
            Debug.LogError("No primary weapon equipped for MultiShotUpgrade");
        }
    }
}
