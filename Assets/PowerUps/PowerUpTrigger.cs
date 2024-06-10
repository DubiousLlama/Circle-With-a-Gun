using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpTrigger : MonoBehaviour
{
    public string powerUpName;

    void OnTriggerEnter2D (Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        { 
            PowerUpManager powerUpManager = collision.gameObject.GetComponent<PowerUpManager>();

            if (powerUpName != "Bomb") {
                powerUpManager.ActivatePowerUp(powerUpName);
                Debug.Log("Power Up Triggered: " + powerUpName);
            }
            else
            {
                Bomb();
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

            // Figure out if the enemy is within 4 units of the bomb
            float distance = Vector2.Distance(enemy.transform.position, transform.position);
            
            if (distance <= 9f)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                enemyHealth.TakeDamage(100);
            }
        }
    }
}
