using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBulletScript : MonoBehaviour
{
    public GameObject hitEffect;
    public int bonusDamagePerSecond = 0;
    public int damage = 0;

    private float timer = 0f;

    private AudioManager audioManager;

    private HashSet<GameObject> foesHit;

    [HideInInspector] public int pierceCount = 0; // Number of enemies the projectile can pierce through, 0 means no piercing
    [HideInInspector] public bool doesPierce = false;

    // When the bullet is created, destroy it after 5 seconds
    void Start()
    {
        Destroy(gameObject, 5f);
        audioManager = AudioManager.instance;
        if (doesPierce)
        {
            foesHit = new HashSet<GameObject>();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8 || collision.gameObject.layer == 13)
        {
            return;
        }

        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (collision.gameObject != null && enemy != null)
        {
            if (doesPierce)
            {
                if (foesHit.Contains(collision.gameObject))
                {
                    return;
                }

                foesHit.Add(collision.gameObject);
                pierceCount--;
            }

            damage += Mathf.RoundToInt(timer * bonusDamagePerSecond); // Increase damage based on time bullet has been alive
            enemy.TakeDamage(damage);
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

        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(effect, 0.5f);

        if (pierceCount <= 0)
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