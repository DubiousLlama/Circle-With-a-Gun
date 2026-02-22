using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapzBullet : MonoBehaviour
{
    public int damage = 200;

    // On collision with player, deal damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().Damage(damage);
            Debug.Log("Bullet player");
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Foe") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Bullet hit wall or foe");
            // Destroy(gameObject);
        }
    }

}
