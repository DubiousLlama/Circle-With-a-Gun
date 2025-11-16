using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletScript;

public class BoomerScript : MonoBehaviour
{
    public int damage = 200;
    public float lifetime = 3.5f;

    enum BoomerState { Going, Returning };

    private BoomerState state = BoomerState.Going;

    private Vector2 direction;

    private float speed = 10f;

    private float timer = 0f;

    private Transform playerTransform;
    private Collider2D col;

    private HashSet<GameObject> hitGoing = new HashSet<GameObject>();
    private HashSet<GameObject> hitReturning = new HashSet<GameObject>();

    private static readonly Vector3 ROTATION_VECTOR = new Vector3(0, 0, 540);
    private Vector3 playerDirection;

    public static event Action<OnBoomerHitEventArgs> BoomerHit;
    public class OnBoomerHitEventArgs : EventArgs
    {
        public Vector3 position = Vector3.zero;
        public EnemyHealth eh = null;
    }

    OnBoomerHitEventArgs recentHit = new OnBoomerHitEventArgs();

    // On collision with player, deal damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Foe")
        {
            // Ensure that enemies are hit a maximim of once going and once returning
            if (state == BoomerState.Returning && !hitReturning.Contains(collision.gameObject))
            {
                hitReturning.Add(collision.gameObject);
                EnemyHealth eh = collision.gameObject.GetComponent<EnemyHealth>();
                eh.TakeDamage(damage);
                recentHit.eh = eh;
                recentHit.position = transform.position;
                BoomerHit?.Invoke(recentHit);
            }
            if (state == BoomerState.Going && !hitGoing.Contains(collision.gameObject))
            {
                hitGoing.Add(collision.gameObject);
                EnemyHealth eh = collision.gameObject.GetComponent<EnemyHealth>();
                eh.TakeDamage(damage);
                recentHit.eh = eh;
                recentHit.position = transform.position;
                BoomerHit?.Invoke(recentHit);
            }
        }
        else if (collision.gameObject.tag == "Wall")
        {
            state = BoomerState.Returning;
        }
    }

    // Seperated to prevent boomerang from getting stuck in player
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (state == BoomerState.Returning && collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector2 direction, float speed)
    {
        this.direction = direction;
        this.speed = speed;
    }

    private void OnDestroy()
    {
        if (col != null)
        {
            col.enabled = false;
        }
    }

    private void Start()
    {
        playerTransform = GameObject.Find("PC").transform;
        col = GetComponent<Collider2D>();
        if (PlayerStats.instance.poisonedStrikes)
        {
            transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (timer >= lifetime)
        {
            state = BoomerState.Returning;
        }
        switch (state)
        {
            case BoomerState.Going:
                // Spin the boomerang
                transform.Rotate(ROTATION_VECTOR * Time.deltaTime);
                transform.Translate(direction * speed * Time.deltaTime, Space.World);
                timer += Time.deltaTime;
                break;
            case BoomerState.Returning:
                // Move at speed in direction of player
                transform.Rotate(ROTATION_VECTOR * Time.deltaTime);
                playerDirection = (playerTransform.position - transform.position).normalized;
                transform.Translate(playerDirection * speed * Time.deltaTime, Space.World);
                break;
        }
    }
}
