using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class LightningStrike : Weapon
{
    private string strike = "Lightning";
    private string explosion = "Explosion";
    private int damage = 125;
    private int aoeDamage = 75;

    private Vector3 destination;
    Collider2D foe;

    LayerMask enemyLayer;

    public void Awake()
    {
        weaponType = getFinalType();
    }

    private Dictionary<WeaponRarity, int> r_damage = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Rare, 150 },
        { WeaponRarity.Uncommon, 100 },
        { WeaponRarity.Common, 50 }
    };

    private Dictionary<WeaponRarity, int> r_aoeDamage = new Dictionary<WeaponRarity, int> {
        { WeaponRarity.Rare, 150 },
        { WeaponRarity.Uncommon, 100 },
        { WeaponRarity.Common, 80 }
    };

    public override void Equip()
    {
        base.Equip();

        enemyLayer = LayerMask.GetMask("Foes");

        damage = r_damage[rarity];
        aoeDamage = r_aoeDamage[rarity];
    }

    public override string getDisplayName()
    {
        return "Lightning Strike";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Secondary;
    }

    public override void Fire()
    {
        // Get a random foe within 10 units of the player
        Collider2D[] foes = Physics2D.OverlapCircleAll(transform.position, 10f, enemyLayer);
        if (foes.Length == 0)
        {
            Debug.Log("No foes in range");
            return;
        }

        foe = foes[Random.Range(0, foes.Length)];

        destination = foe.transform.position;


        // The lighting strikeSFX object has a child called 'StrikePoint' which is the point where the lightning strikes
        // Adjust the position of the lightning strikeSFX so that the StrikePoint is at the destination

        Vector3 strikeOffset = new Vector3(0.27f * 0.4f, 10.8f * 0.4f, 0);

        // Create the lightning strikeSFX 
        GameObject lightningStrike = Instantiate(Resources.Load<GameObject>("LightningStrikeEffect"), destination + strikeOffset, Quaternion.identity);
        Destroy(lightningStrike, 2f);
        AudioManager.instance.PlaySfx(strike);
        Invoke("LightningExplosion", 0.5f);

    }

    private void LightningExplosion()
    {
        AudioManager.instance.PlaySfx(explosion);
        

        // Get all foes directly contacted
        Collider2D[] foes = Physics2D.OverlapCircleAll(destination, 0.5f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe") {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(damage);
            }
        }

        Invoke("LightningAoE", 0.1f);
    }

    private void LightningAoE()
    {
        // Get all foes in the AoE
        Collider2D[] foes = Physics2D.OverlapCircleAll(destination, 2f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(aoeDamage);
            }
        }
    }
}
