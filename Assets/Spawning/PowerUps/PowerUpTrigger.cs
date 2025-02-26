using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PowerUpTrigger : MonoBehaviour
{
    public string powerUpName;

    public GameObject explosion;

    PlayerStats playerStats;
    PowerUpManager powerUpManager;

    void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        { 
            if (powerUpManager == null )
            {
                powerUpManager = collision.gameObject.GetComponent<PowerUpManager>();
            }
            if (playerStats == null)
            {
                playerStats = collision.gameObject.GetComponent<PlayerStats>();
            }
                
            Debug.Log("Power Up Triggered: " + powerUpName);

            switch (powerUpName)
            {
                case "Speed":
                    PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
                    playerMovement.SetSpeedBonus(3.5f);
                    AudioManager.instance.PlaySfx("Score1", 0.25f);
                    break;
                case "Regen":
                    PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                    playerHealth.Heal(1000);
                    AudioManager.instance.PlaySfx("Score3", 0.25f);
                    break;
                case "Bomb":
                    Bomb();
                    GameObject b = Instantiate(explosion, transform.position, Quaternion.identity);
                    b.transform.localScale = new Vector3(9*0.4f, 9 * 0.4f, 1);
                    Destroy(b, 0.5f);
                    AudioManager.instance.PlaySfx("WooshLightning", 0.6f);
                    break;
                default:
                    break;
            }   

            // Set the color of the power up to (0.9,0.9, 0.9) for 0.1 seconds, then destroy it
            GetComponent<SpriteRenderer>().color = new Color(0.9f, 0.9f, 0.9f);
            Destroy(gameObject, 0.1f);
        }
    }

    private void Bomb()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Foe");

        foreach (GameObject enemy in enemies)
        {

            // Figure out if the enemy is within 9 units of the bomb
            float distance = Vector2.Distance(enemy.transform.position, transform.position);
            
            if (distance <= 5f)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(500);
            }

            if (distance <= 9f)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(100);
            }
        }
    }
}
