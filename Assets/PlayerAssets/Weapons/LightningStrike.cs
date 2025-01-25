using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningStrike : Weapon
{
    // Start is called before the first frame update
    public int damage = 80;

    GameObject strikePrefab;
    private string sfx = "Lightning";

    public new void Start()
    {
        base.Start();

        strikePrefab = Resources.Load<GameObject>("LightningAttack");

        if (strikePrefab == null)
        {
            Debug.LogError("Strike prefab not found");
        }

        // Modify base class variables as needed
        cooldown = 1f;
        isAutomatic = false;
    }

    public override void Fire()
    {
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, 90);

        Vector3 offset = new Vector3(strikePrefab.GetComponent<BoxCollider2D>().size.x * 0.5f, 0, 1);
        // Rotate the offset by the firepoint's rotation
        offset = rot * offset;


        GameObject strike = Instantiate(strikePrefab, firePoint.position + offset, rot);

        Collider2D collider = strike.GetComponent<Collider2D>();

        // Get all the colliders that the strike is touching
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        List<Collider2D> results = new List<Collider2D>();
        int count = collider.OverlapCollider(filter, results);
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
