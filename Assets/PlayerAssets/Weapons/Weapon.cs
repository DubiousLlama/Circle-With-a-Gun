using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Primary, Secondary };
public enum WeaponSlot { One, Two, Three };

public enum WeaponRarity { Common, Uncommon, Rare, Legendary };

public class Weapon : MonoBehaviour
{
    public WeaponType weaponType;
    public WeaponSlot weaponSlot {
        get {
            if (rarity == WeaponRarity.Legendary) {
                return WeaponSlot.Three;
            } else if (weaponType == WeaponType.Secondary) {
                return WeaponSlot.Two;
            } else {
                return WeaponSlot.One;
            }
        }
    }
    public float cooldown = 0.1f;
    public float lifetime = 0f;

    public bool isAutomatic {
        get {
            return weaponType == WeaponType.Primary;
        }
    }
    public bool isTemporary {
        get {
            return rarity == WeaponRarity.Legendary;
        }
    }


    public float lifetimeRemaining = 0f;
    public float cooldownRemaining = 0f;
    protected AudioManager audioManager;
    protected PlayerStats playerStats;
    protected Transform firePoint;
    protected RechargeBarController barController;
    
    protected bool isFiring {
        set {
            weaponsManager.isFiring[weaponType] = value;
        }
        get {
            return weaponsManager.GetEquippedWeapon(weaponType) == this && weaponsManager.isFiring[weaponType];
        }
    }

    protected bool isEquipped = false;
    public WeaponsManager weaponsManager;
    protected WeaponRarity rarity = WeaponRarity.Common;

    // Start is called before the first frame update
    public virtual void Equip()
    {
        Debug.Assert(weaponsManager != null, "WeaponsManager is not set");

        weaponsManager.weapons[weaponSlot] = this;

        isEquipped = true;

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

    public void SetRarity(WeaponRarity rarity)
    {
        this.rarity = rarity;
    }

    protected virtual void Fire() // Override this method to implement fire behavior for each weapon
    {
        Debug.Log("No weapon fire behavior implemented");
    }

    public virtual bool IsExpired()
    {
        return isTemporary ? lifetimeRemaining <= 0 : false;
    }

    public bool CanFire()
    {
        return cooldownRemaining <= 0 && (isTemporary ? !IsExpired() : true);
    }

    // Update is called once per frame
    public void Update()
    {
        if (!isEquipped)
        {
            return;
        }

        if (cooldownRemaining > 0)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if (isTemporary)
        {
            if (IsExpired())
            {
                Expire();
            }
            else
            {
                lifetimeRemaining -= Time.deltaTime;
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

    public void Expire()
    {
        weaponsManager.weapons[weaponSlot] = null;
        Destroy(gameObject);
    }
}
