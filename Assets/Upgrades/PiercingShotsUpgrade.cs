using UnityEngine;

public class PiercingShotsUpgrade : MonoBehaviour
{
    private Weapon primaryWeapon;

    void Start()
    {
        primaryWeapon = gameObject.GetComponentInParent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary);
        if (primaryWeapon != null)
        {
            primaryWeapon.pierceCount += 2;
        }
    }

    void OnDestroy()
    {
        if (primaryWeapon != null)
        {
            primaryWeapon.pierceCount -= 2;
        }
    }
}
