using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Primary, Secondary };

public class Weapon : MonoBehaviour
{
    public WeaponType weaponType;
    public float cooldown = 0.1f;
    public bool isAutomatic = false; // Is it a good idea for this to have different behavior? Or should slow firing weapons still fire repeatedly when the button is held down?
    public bool isTemporary = false;
    public float lifetime = 0f;


    public float lifetimeRemaining = 0f;
    public float cooldownRemaining = 0f;
    protected AudioManager audioManager;
    protected PlayerStats playerStats;
    protected Transform firePoint;
    protected RechargeBarController barController;

    protected bool isFiring = false;

    protected void Awake() {}

    // Start is called before the first frame update
    protected void Start()
    {
        audioManager = AudioManager.instance;
        playerStats = transform.parent.GetComponent<PlayerStats>();
        firePoint = transform.parent.Find("FirePoint");

        GameObject rechargeBar = GameObject.Find("RechargeBar");
        barController = rechargeBar.GetComponent<RechargeBarController>();
        if (weaponType == WeaponType.Secondary)
        {
            barController.SetMaxRecharge(cooldown);
        }

        lifetimeRemaining = lifetime;
    }

    public void OnPointerDown()
    {
        isFiring = true;
    }

    public void OnPointerUp()
    {
        isFiring = false;
    }

    protected virtual void Fire() // Override this method to implement fire behavior for each weapon
    {
        Debug.Log("No weapon fire behavior implemented");
    }

    public bool CanFire()
    {
        return cooldownRemaining <= 0 && (isTemporary ? lifetimeRemaining > 0 : true);
    }

    // Update is called once per frame
    public void Update()
    {
        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if (isTemporary)
        {
            if (lifetimeRemaining > 0)
            {
                lifetimeRemaining -= Time.deltaTime;
            }
            else
            {
                Expire();
            }
        }

        if (isFiring)
        {
            if (CanFire())
            {
                Fire();
                cooldownRemaining = cooldown;
            }
        }
        if (!isAutomatic)
        {
            isFiring = false;
        }

        if (weaponType == WeaponType.Secondary)
        {
            float charge = cooldown - cooldownRemaining;

            if (charge > 0 && charge <= cooldown)
            {
                barController.SetRecharge(charge);
            }
        }
    }

    private void Expire()
    {
        Destroy(gameObject);
    }
}
