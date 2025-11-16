using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagonalShotsUpgrade : MonoBehaviour
{
    public float angleOffset = 15f;

    private List<Transform> addedFirePoints = new List<Transform>();

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
            addedFirePoints.Add(newFP.transform);
        }
    }

    void OnDestroy()
    {
        Weapon wep = transform.parent.GetComponent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary);
        if (wep != null)
        {
            foreach (Transform fp in addedFirePoints)
            {
                if (fp != null)
                {
                    wep.firePoint.Remove(fp);
                    Destroy(fp.gameObject);
                }
            }
        }
        addedFirePoints.Clear();
    }
}
