using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiskyTooth : MonoBehaviour
{
    public int HealMultipler = 10;

    PlayerHealth playerHealth;
    private Action<XPPickup.OnXPPickupEventArgs> xpHandler;

    void Start()
    {
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
        xpHandler = (e) => OnXPPickedUp(e.xpAmount);
        XPPickup.XPPickedUp += xpHandler;
    }

    void OnXPPickedUp(int amount)
    {
        playerHealth.Heal(amount * HealMultipler, false);
    }

    void OnDestroy()
    {
        if (xpHandler != null)
        {
            XPPickup.XPPickedUp -= xpHandler;
        }
    }
}
