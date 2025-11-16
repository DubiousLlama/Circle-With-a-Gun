using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Primary, Secondary, Legendary };
public enum WeaponSlot { One, Two, Three };

public enum WeaponRarity { Common, Uncommon, Rare, Legendary };

public class Weapon : MonoBehaviour
{
    public Sprite sprite;
    public WeaponType weaponType;
    public WeaponSlot weaponSlot {
        get {
            if (weaponType == WeaponType.Legendary) {
                return WeaponSlot.Three;
            } else if (weaponType == WeaponType.Secondary) {
                return WeaponSlot.Two;
            } else {
                return WeaponSlot.One;
            }
        }
    }
    public float cooldown = 0.1f;
    public float lifetime = 10f;

    [HideInInspector]
    public bool isAutomatic = false;
    public bool isTemporary {
        get {
            return rarity == WeaponRarity.Legendary;
        }
    }

    public static event Action<OnWeaponUsedArgs> OnWeaponUsed;
    public class OnWeaponUsedArgs : EventArgs
    {
        public string weaponName;
        public WeaponType weaponType;
    }


    [HideInInspector]
    public float lifetimeRemaining = 0f;
    [HideInInspector]
    public float cooldownRemaining = 0f;
    protected AudioManager audioManager;
    protected PlayerStats playerStats;
    protected PlayerMovement playerMovement;
    public List<Transform> firePoint;
    protected RechargeBarController barController;
    
    private bool wasOnCooldown = false;
    
    protected bool isFiring {
        set {
            weaponsManager.isFiring[weaponType] = value;
        }
        get {
            return weaponsManager.GetEquippedWeapon(weaponType) == this && weaponsManager.isFiring[weaponType];
        }
    }

    protected bool isEquipped = false;
    
    [HideInInspector]
    public WeaponsManager weaponsManager;
    public WeaponRarity rarity { get; protected set; } = WeaponRarity.Common;

    // Upgrade related
    [HideInInspector]
    public bool burstMode = false;
    bool isBursting = false;

    [HideInInspector]
    public int pierceCount = 0; // Number of enemies the projectile can pierce through, 0 means no piercing

    // Start is called before the first frame update
    public virtual void Equip()
    {
        Debug.Assert(weaponsManager != null, "WeaponsManager is not set");

        weaponsManager.weapons[weaponSlot] = this;

        isEquipped = true;
        isFiring = false; // Ensure firing is reset when weapon is equipped

        audioManager = AudioManager.instance;
        playerStats = transform.parent.GetComponent<PlayerStats>();
        firePoint.Add(transform.parent.Find("FirePoint"));
        playerMovement = transform.parent.GetComponent<PlayerMovement>();

        GameObject rechargeBar = GameObject.Find("RechargeBar");
        barController = rechargeBar.GetComponent<RechargeBarController>();
        if (weaponType == WeaponType.Secondary)
        {
            barController.SetMaxRecharge(cooldown);
        }

        lifetimeRemaining = lifetime * PlayerStats.instance.GetStatMod(StatTypes.LegendaryDuration);
    }

    public virtual void SetRarity(WeaponRarity rarity)
    {
        this.rarity = rarity;
    }

    public virtual void Fire() // Override this method to implement fire behavior for each weapon
    {
        Debug.Log("No weapon fire behavior implemented");
    }

    public void FireCleanup()
    {
        cooldownRemaining = cooldown;
        if (burstMode && isBursting)
        {
            // Debug.Log("Burst shot fired");
            cooldownRemaining = cooldown / 5f;
            isBursting = false;
        } else if (burstMode && !isBursting)
        {
            isBursting = true;
            cooldownRemaining = cooldown * 1.2f;
        }
        OnWeaponUsed?.Invoke(new OnWeaponUsedArgs { weaponName = getDisplayName(), weaponType = getFinalType() });
    }

    public virtual string getDisplayName()
    {
        return "Name not set.";
    }

    public virtual WeaponType getFinalType()
    {
        return WeaponType.Primary;
    }

    public virtual bool IsExpired()
    {
        return isTemporary ? lifetimeRemaining <= 0 : false;
    }

    public bool CanFire()
    {
        bool canFireCooldown = cooldownRemaining <= 0;
        bool canFireTemporary = isTemporary ? !IsExpired() : true;
        return canFireCooldown && canFireTemporary;
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
            if (weaponType == WeaponType.Primary || weaponType == WeaponType.Legendary)
            {
                cooldownRemaining -= Time.deltaTime * PlayerStats.instance.GetStatMod(StatTypes.AttackSpeed);
            } else
            {
                cooldownRemaining -= Time.deltaTime * PlayerStats.instance.GetStatMod(StatTypes.SecondaryCooldown);
            }
            
            wasOnCooldown = true;
        }
        else if (wasOnCooldown && weaponType == WeaponType.Secondary)
        {
            // Secondary weapon just became ready - play sound
            audioManager.PlaySfx("SecondaryReady");
            barController.BarFlash(true);
            wasOnCooldown = false;
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
                if (getFinalType() == WeaponType.Secondary)
                {
                    if (playerMovement.isMoving())
                    {
                        playerMovement.SecondaryActivationWhileMoving();
                        barController.BarFlash(false);
                        return;
                    }
                }
                Fire();
                FireCleanup();
            } else
            {
                if (getFinalType() == WeaponType.Secondary)
                {
                    AudioManager.instance.PlaySfx("SecondaryWrong");
                    barController.BarFlash(false);
                }
            }
        }
        if (!isAutomatic)
        {
            isFiring = false;
        }

        if (weaponType == WeaponType.Secondary)
        {

            barController.SetMaxRecharge(cooldown);
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
        isFiring = false;
        Destroy(gameObject);
        weaponsManager.OnPrimaryDown();
    }
}
