using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TrapzController : MonoBehaviour
{

    // public paramaters
    public float range = 7f;
    public float attackWindup = 0.8f;
    public float attackCooldown = 1f;
    public int damage = 200;
    public float bulletSpeed = 10f;

    // Object references
    SpriteRenderer eyes;
    GameObject player;
    Pathfinding.AIPath pathfinding;
    public GameObject projectile;
    GameObject firePoint;

    // Internal variables
    TrapzState state;
    float recharge;
    AudioManager audioManager;


    // Start is called before the first frame update
    void Start()
    {
        eyes = gameObject.transform.Find("TrapzEyes").GetComponent<SpriteRenderer>();
        player = GameObject.Find("PC");
        state = TrapzState.moving;
        pathfinding = GetComponent<Pathfinding.AIPath>();
        firePoint = gameObject.transform.Find("FirePoint").gameObject;

        audioManager = AudioManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        recharge -= Time.deltaTime;

        Vector3 directionToTarget = (player.transform.position - transform.position).normalized;



        // First, calculate state changes
        switch (state)
        {
            case TrapzState.moving:
                // Check if the player distance is within range
                float playerDistance = Vector2.Distance(transform.position, player.transform.position);
                if (playerDistance < range)
                {
                    state = TrapzState.poweringUp;
                    recharge = attackWindup;
                    audioManager.PlaySfx("ChargeUp");
                }
                break;
            case TrapzState.poweringUp:
                if (recharge <= 0)
                {
                    state = TrapzState.recovering;
                    recharge = attackCooldown;
                    Attack();
                }
                if (Vector2.Distance(transform.position, player.transform.position) > range)
                {
                    state = TrapzState.recovering;
                    recharge = attackCooldown/8;
                }
                break;
            case TrapzState.recovering:
                if (recharge <= 0)
                {
                    state = TrapzState.moving;
                }

                break;
        }

        // Then, take action based on the state
        switch(state)
        {
            case TrapzState.moving:
                eyes.color = Color.black;
                pathfinding.enabled = true;

                // Set that as the pathfinding target
                pathfinding.destination = player.transform.position;

                break;
            case TrapzState.poweringUp:
                eyes.color = new Color((attackWindup-recharge)/attackWindup, 0f, 0f);
                pathfinding.enabled = false;

                float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90), 600 * Time.deltaTime);


                break;
            case TrapzState.recovering:
                pathfinding.enabled = false;
                eyes.color = Color.black;

                // Return the triangle to its default rotation
                if (recharge < attackCooldown*0.75f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 180), 400 * Time.deltaTime);
                }
                
                break;
        }
    }

    enum TrapzState
    {
        moving,
        poweringUp,
        recovering
    }

    void Attack()
    {
        // Get the player's position
        Vector3 playerPosition = player.transform.position;

        // Get the player's velocity
        Vector3 playerVelocity = player.GetComponent<Rigidbody2D>().velocity;

        // Calculate where the player will be when the bullet reaches them (intentionally crude approximation, the real solution involves solving a quadratic I think)
        Vector3 futurePosition = playerPosition + playerVelocity/2;

        
        // Instantiate a bullet at the trapz's FirePoint
        GameObject bullet = Instantiate(projectile, firePoint.transform.position, Quaternion.identity);

        bullet.layer = 8;

        // Get the bullet's rigidbody
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        // Get the bullet's controller
        TrapzBullet bulletController = bullet.GetComponent<TrapzBullet>();
        bulletController.damage = damage;

        // Get the direction to the future position
        Vector3 direction = futurePosition - firePoint.transform.position;

        rb.velocity = direction.normalized * bulletSpeed;
    }
}
