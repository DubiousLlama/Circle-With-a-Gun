using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerScript : MonoBehaviour
{
    public int damage = 200;

    enum BoomerState { Going, Returning};

    BoomerState state = BoomerState.Going;

    // On collision with player, deal damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Foe")
        {
            collision.gameObject.GetComponent<EnemyHealth>().TakeDamage(damage);

        }
        else if (collision.gameObject.tag == "Wall")
        {
            state = BoomerState.Returning;
        }
        else if (collision.gameObject.tag == "Player")
        {
            if (state == BoomerState.Returning)
            {
                Destroy(gameObject);
            }
        }
    }
}
