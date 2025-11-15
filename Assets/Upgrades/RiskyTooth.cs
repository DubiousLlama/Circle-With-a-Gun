using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiskyTooth : MonoBehaviour
{
    public int HealMultipler = 10;

    PlayerHealth playerHealth;

    // Start is called before the first frame update
    void Start()
    {
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
        XPPickup.XPPickedUp += (e) => OnXPPickedUp(e.xpAmount);
    }

    void OnXPPickedUp(int amount)
    {
        playerHealth.Heal(amount * HealMultipler, false);
    }
    
}
