using UnityEngine;

public class PiercingShotsUpgrade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponentInParent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary).pierceCount += 2;
    }
}
