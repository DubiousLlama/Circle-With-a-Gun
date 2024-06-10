using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public GameObject hitEffect;
    public int damage = 20;

    private AudioManager audioManager;

    // When the bullet is created, destroy it after 5 seconds
    void Start()
    {
        Destroy(gameObject, 5f);
        audioManager = AudioManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
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
        hitEffect.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        Destroy(effect, 0.5f);
        Destroy(gameObject);
    }


}