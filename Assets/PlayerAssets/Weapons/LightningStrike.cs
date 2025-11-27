using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class LightningStrike : Weapon
{
    private string strike = "Lightning";
    private string explosion = "Explosion";
    public int damage = 125;
    public int aoeDamage = 75;
    public bool MissMicro = false;

    private Vector3 destination;
    Collider2D foe;
    GameObject strikePrefab;

    LayerMask enemyLayer;

    public void Awake()
    {
        weaponType = getFinalType();
    }

    public void Start()
    {
        string strikeToUse = MissMicro ? "GreyStrike" : "LightningStrikeEffect";
        strikePrefab = Resources.Load<GameObject>(strikeToUse);
    }

    public override void Equip()
    {
        base.Equip();
        PlayerStats.instance.secondaryWeaponType = SecondaryWeaponType.LightningStrike;
        enemyLayer = LayerMask.GetMask("Foes");
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
        GameObject lightningStrike = Instantiate(strikePrefab, destination + strikeOffset, Quaternion.identity);
        Destroy(lightningStrike, 2f);
        AudioManager.instance.PlaySfx(MissMicro ? strike : "StrikeFast");
        Invoke("LightningExplosion", MissMicro ? 0.4f : 0.5f);

    }

    private void LightningExplosion()
    {
        AudioManager.instance.PlaySfx(explosion, 0.8f);
        

        // Get all foes directly contacted
        Collider2D[] foes = Physics2D.OverlapCircleAll(destination, MissMicro ? 1.2f : 0.75f, enemyLayer);
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
        Collider2D[] foes = Physics2D.OverlapCircleAll(destination, MissMicro ? 3f : 2.4f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(aoeDamage);
            }
        }
    }
}
