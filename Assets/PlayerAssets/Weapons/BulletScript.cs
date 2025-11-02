using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public GameObject hitEffect;
    public int damage = 20;

    protected AudioManager audioManager;
    protected HashSet<GameObject> foesHit;

    [HideInInspector] public int pierceCount = 0; // Number of enemies the projectile can pierce through, 0 means no piercing
    [HideInInspector] public bool doesPierce = false;
    [HideInInspector] public bool wallBounce = false;

    // When the bullet is created, destroy it after 5 seconds
    protected virtual void Start()
    {
        Destroy(gameObject, 5f);
        audioManager = AudioManager.instance;

        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        if (pierceCount > 0)
        {
            doPierce(true);
            foesHit = new HashSet<GameObject>();
        }

        // Configure bullet physics for minimal force impact and clean bouncing
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.mass = 0.001f; // Ultra-light mass to minimize force on enemies
            rb.gravityScale = 0f; // Ensure no gravity affects the bullet
            rb.drag = 0f; // No linear drag to maintain momentum
            rb.angularDrag = 0f; // No angular drag to prevent unwanted rotation
            rb.freezeRotation = true; // Prevent spinning completely
        }
    }

    // Virtual method for calculating damage - can be overridden by subclasses
    protected virtual int CalculateDamage()
    {
        return damage;
    }

    protected void doPierce(bool setTo)
    {
        doesPierce = setTo;
        gameObject.GetComponent<Collider2D>().isTrigger = setTo;
        transform.GetChild(0).gameObject.SetActive(setTo);
    }

    // Virtual method for handling enemy hit logic - can be overridden by subclasses
    protected virtual void OnEnemyHit(EnemyHealth enemy, GameObject enemyObject)
    {
        if (doesPierce)
        {
            if (foesHit.Contains(enemyObject))
            {
                return;
            }

            foesHit.Add(enemyObject);
            pierceCount--;
            if (pierceCount == 0)
            {
                doPierce(false);
            }
        }

        Debug.Log("Hit enemy: " + enemyObject.name);
        int finalDamage = CalculateDamage();
        enemy.TakeDamage(finalDamage);
        
        if (enemy.health <= 0)
        {
            if (enemy.threshold1 > 60)
            {
                audioManager.PlaySfx("Bang", 0.25f);
            }
            else
            {
                audioManager.PlaySfx("SmallShot", 0.15f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NonPhysicsHit(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (wallBounce && collision.gameObject.layer == 9)
        {

            Rigidbody2D rb = GetComponent<Rigidbody2D>();

            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f;


            damage += damage / 2;
            transform.GetChild(1).gameObject.SetActive(true);
            hitEffect.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            wallBounce = false;
            return;
        }

        NonPhysicsHit(collision.collider);
        if (hitEffect.transform.localScale.x != 0.15f) { hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f); }
        GameObject he = Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
        Destroy(he, 0.5f);
    }

    private void NonPhysicsHit(Collider2D collision)
    {
        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            OnEnemyHit(enemy, collision.gameObject);
        }

        if (pierceCount <= 0 || (collision.gameObject.layer != 10 && collision.gameObject.layer != 18))
        {
            // Debug.Log("Bullet destroyed. Pierce count: " + pierceCount);
            Destroy(gameObject);
        }
    }
}