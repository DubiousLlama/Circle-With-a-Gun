using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStormUpgrade : MonoBehaviour
{
    public GameObject weaponPrefab;

    private void Start()
    {
        WeaponsManager wm = GameObject.Find("PC").GetComponent<WeaponsManager>();
        GameObject wep = Instantiate(weaponPrefab);
        wm.EquipWeapon(wep);
    }
}
