using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public GameObject hitEffect;
    public int damage = 20;

    // Apply to the instantiated effect only — mutating hitEffect dirties the shared prefab asset.
    protected Vector3 hitEffectScale = new Vector3(0.1f, 0.1f, 0.1f);

    protected AudioManager audioManager;
    protected HashSet<GameObject> foesHit;

    [HideInInspector] public int pierceCount = 0; // Number of enemies the projectile can pierce through, 0 means no piercing
    [HideInInspector] public bool doesPierce = false;
    [HideInInspector] public bool wallBounce = false;

    public static event Action<OnBulletHitEventArgs> BulletHit;
    public class OnBulletHitEventArgs : EventArgs
    {
        public Vector3 position = Vector3.zero;
        public EnemyHealth eh = null;
    }

    OnBulletHitEventArgs recentHit = new OnBulletHitEventArgs();

    private Color poisonStartColor = new Color(0.323594f, 0.001901004f, 0.335849f, 0.48f);
    private Color poisonEndColor = new Color(0.7302355f, 0.1072054f, 0.9018868f, 0f);
    private Color freezeStartColor = new Color(0.38f, 0.82f, 1f, 0.38f);
    private Color freezeEndColor = new Color(0.12f, 0.56f, 1f, 0f);

    // When the bullet is created, destroy it after 5 seconds
    protected virtual void Start()
    {
        Destroy(gameObject, 5f);
        audioManager = AudioManager.instance;

        if (PlayerStats.instance.poisonedStrikes || PlayerStats.instance.freezingStrikes)
        {
            TrailRenderer tr = GetComponent<TrailRenderer>();
            tr.enabled = true;

            if (PlayerStats.instance.poisonedStrikes)
            {
                tr.startColor = poisonStartColor;
                tr.endColor = poisonEndColor;
            }
            else if (PlayerStats.instance.freezingStrikes)
            {
                tr.startColor = freezeStartColor;
                tr.endColor = freezeEndColor;
            }
        } else
        {
            GetComponent<TrailRenderer>().enabled = false;
        }

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
            rb.linearDamping = 0f; // No linear drag to maintain momentum
            rb.angularDamping = 0f; // No angular drag to prevent unwanted rotation
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

        recentHit.position = transform.position;
        recentHit.eh = enemy;
        BulletHit?.Invoke(recentHit);
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

            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f;


            damage += damage;
            transform.GetChild(1).gameObject.SetActive(true);
            hitEffectScale = new Vector3(0.15f, 0.15f, 0.15f);
            wallBounce = false;
            return;
        }

        NonPhysicsHit(collision.collider);
        GameObject he = Instantiate(hitEffect, collision.contacts[0].point, Quaternion.identity);
        he.transform.localScale = hitEffectScale;
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