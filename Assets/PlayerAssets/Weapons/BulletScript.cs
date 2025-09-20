using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public GameObject hitEffect;
    public int damage = 20;

    protected AudioManager audioManager;
    protected HashSet<GameObject> foesHit;

    [HideInInspector] public int pierceCount = 0; // Number of enemies the projectile can pierce through, 0 means no piercing
    [HideInInspector] public bool doesPierce = false;

    // When the bullet is created, destroy it after 5 seconds
    protected virtual void Start()
    {
        Destroy(gameObject, 5f);
        audioManager = AudioManager.instance;

        if (doesPierce)
        {
            foesHit = new HashSet<GameObject>();
        }
    }

    // Virtual method for calculating damage - can be overridden by subclasses
    protected virtual int CalculateDamage()
    {
        return damage;
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
        if (collision.gameObject.layer == 8 || collision.gameObject.layer == 13)
        {
            return;
        }

        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            OnEnemyHit(enemy, collision.gameObject);
        }

        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GameObject he = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(he, 0.5f);

        if (!doesPierce || pierceCount <= 0)
        {
            Destroy(gameObject);
        }
    }

    //Destroy the bullet if it hits an object
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8 || (doesPierce && collision.gameObject.layer == 10))
        {
            return;
        }

        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(effect, 0.5f);
        Destroy(gameObject);
    }
}