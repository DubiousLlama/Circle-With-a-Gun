using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PowerUpTrigger : MonoBehaviour
{
    public string powerUpName;

    public GameObject explosion;

    private PlayerStats stats;

    public static event Action<OnPowerUpPickedUpArgs> PowerUpPickedUp;
    public class OnPowerUpPickedUpArgs : EventArgs
    {
        public string powerUpName;
        public Vector3 position;
    }

    private void Start()
    {
        stats = PlayerStats.instance;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PowerUpPickedUp?.Invoke(new OnPowerUpPickedUpArgs
            {
                powerUpName = powerUpName,
                position = transform.position
            });

            switch (powerUpName)
            {
                case "Speed":
                    string powerUpTag = "SpeedPowerUp";
                    if (stats.DoesTagExist(powerUpTag))
                    {
                        stats.RemoveAllModifiersWithTag(powerUpTag);
                    }
                    stats.ModifyMultStat(StatTypes.MoveSpeed, 1.2f, false, 6f, powerUpTag, 2f);
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
                    b.transform.localScale = new Vector3(9 * 0.4f, 9 * 0.4f, 1);
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
            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh == null) { continue; }

            // Figure out if the enemy is within 9 units of the bomb
            float distance = Vector2.Distance(enemy.transform.position, transform.position);

            if (distance <= 5f)
            {
                eh.TakeDamage(500);
            }

            if (distance <= 9f)
            {
                eh.TakeDamage(100);
            }
        }
    }
}
