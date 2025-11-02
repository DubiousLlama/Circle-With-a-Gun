using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPPickup : MonoBehaviour
{
    public int xpAmount = 100;
    public float baseSize = 0.012f;
    public string sfx1;
    public string sfx2;
    public string sfx3;

    public static event Action<OnXPPickupEventArgs> XPPickedUp;
    public class OnXPPickupEventArgs : EventArgs {
        public int xpAmount;
    }

    public void SetXPAmount(int xpAmount)
    {
        this.xpAmount = xpAmount;
        transform.localScale = (Mathf.Log10(xpAmount*2 + 1) + 0.6f) * Vector3.one * baseSize;
        gameObject.GetComponent<XPBounce>().baseScale = transform.localScale;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Trigger the event
        if (col.CompareTag("Player"))
        {
            XPPickedUp?.Invoke(new OnXPPickupEventArgs { xpAmount = xpAmount });
            // Debug.Log($"Picked up {xpAmount} XP");
            Destroy(gameObject);

            string useSfx;
            if (xpAmount <= 5)
                useSfx = sfx1;
            else if (xpAmount <= 10)
                useSfx = sfx2;
            else
                useSfx = sfx3;
            AudioManager.instance.PlaySfx(useSfx, 0.35f);
        }
    }
}
