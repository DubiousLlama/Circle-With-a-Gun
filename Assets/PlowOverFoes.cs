using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlowOverFoes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(9999);
        }
    }
}
