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

    // When the bullet is created, destroy it after 5 seconds
    void Start()
    {
        Destroy(gameObject, 5f);
        audioManager = AudioManager.instance;
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            return;
        }

        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            Debug.Log("Bonus damage: " + Mathf.RoundToInt(timer * bonusDamagePerSecond));
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
        Destroy(gameObject);
    }

    //Destroy the bullet if it hits an object
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 8)
        {
            return;
        }

        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(effect, 0.5f);
        Destroy(gameObject);
    }


}