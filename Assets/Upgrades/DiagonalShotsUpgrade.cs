using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagonalShotsUpgrade : MonoBehaviour
{
    public float angleOffset = 15f;

    // Vector3 offset = new Vector3(1.057f, -1.418f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        Weapon wep = transform.parent.GetComponent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary);

        for (int i = -1; i <= 1; i += 2)
        {
            GameObject newFP = Instantiate(new GameObject("MyEmptyObject"), wep.firePoint[0]);
            newFP.transform.localPosition = Vector3.zero;
            newFP.transform.Rotate(0, 0, angleOffset * i);
            wep.firePoint.Add(newFP.transform);
        }
    }
}
