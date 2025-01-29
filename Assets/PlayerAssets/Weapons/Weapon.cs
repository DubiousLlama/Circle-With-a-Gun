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

    protected bool isFiring = false;

    // Start is called before the first frame update
    protected void Start()
    {
        audioManager = AudioManager.instance;
        playerStats = GetComponent<PlayerStats>();
        firePoint = transform.Find("FirePoint");
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

    // Update is called once per frame
    void Update()
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
            if (cooldownRemaining <= 0)
            {
                Fire();
                cooldownRemaining = cooldown;
            }
        }
        if (!isAutomatic)
        {
            isFiring = false;
        }
    }

    private void Expire()
    {
        Debug.Log("No weapon expiration behavior implemented");
        // SCOTT: Once you have weapon switching enabled, implement this method so that the player is restored to the default weapon upon expiration
    }
}
