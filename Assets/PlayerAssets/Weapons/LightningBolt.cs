using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : Weapon
{
    public int damage = 120;

    GameObject strikePrefab;
    private string sfx = "Lightning";

    public void Awake()
    {
        isAutomatic = true;
        weaponType = WeaponType.Legendary;
        SetRarity(WeaponRarity.Legendary);
    }

    public override void Equip()
    {
        base.Equip();
        
        strikePrefab = Resources.Load<GameObject>("LightningAttack");

        if (strikePrefab == null)
        {
            Debug.LogError("Strike prefab not found");
        }
    }
    
    private Dictionary<WeaponRarity, float> cooldowns = new Dictionary<WeaponRarity, float> {
        { WeaponRarity.Legendary, 0.4f },
    };

    public override void SetRarity(WeaponRarity rarity)
    {
        base.SetRarity(rarity);
        cooldown = cooldowns[rarity];
    }

    public override string getDisplayName()
    {
        return "Lightning Bolt";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Legendary;
    }

    public override void Fire()
    {
        Quaternion rot = firePoint[0].rotation * Quaternion.Euler(0, 0, 90);

        Vector3 offset = new Vector3(strikePrefab.GetComponent<BoxCollider2D>().size.x * 0.5f, 0, 1);
        // Rotate the offset by the firepoint's rotation
        offset = rot * offset;


        GameObject strike = Instantiate(strikePrefab, firePoint[0].position + offset, rot);

        Collider2D collider = strike.GetComponent<Collider2D>();

        // Get all the colliders that the strikeSFX is touching
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        List<Collider2D> results = new List<Collider2D>();
        int count = collider.Overlap(filter, results);
        Debug.Log(count);

        foreach (Collider2D hit in results)
        {
            // Check if the collider is an enemy
            if (hit.CompareTag("Foe"))
            {
                EnemyHealth health = hit.GetComponent<EnemyHealth>();

                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }
        audioManager.PlaySfx(sfx);
        Destroy(strike, 0.5f);
    }

}
