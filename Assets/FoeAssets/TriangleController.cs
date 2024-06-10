using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TriangleController : MonoBehaviour
{
    [Range (50, 900)]
    public int damage = 400;

    [Range(2, 12)]
    public float range = 7f;

    [Range(0.1f, 4)]
    public float attackWindup = 0.8f;

    [Range(0.1f, 6)]
    public float attackCooldown = 1;

    [Range (5, 12)]
    public float chargePace = 8;

    [Range(0.5f, 4)]
    public float chargeDuration = 1.5f;

    private string state;
    private GameObject player;
    private float attackRecharge = 0;
    private float attackingFor = 0;
    private float wait = 0;
    private Pathfinding.AIPath pathfinding;
    private Vector3 target;
    
    // Update is called once per frame

    void Start()
    {
        player = GameObject.Find("PC");
        pathfinding = GetComponent<Pathfinding.AIPath>();
        attackRecharge = attackCooldown;
        state = "pathfinding";
    }

    void Update()
    {

        // First, update the state of the triangle
        float playerDistance = Vector2.Distance(transform.position, player.transform.position);

        if (range / 2 < playerDistance && playerDistance < range && state == "pathfinding" && attackRecharge <= 0)
        {
            pathfinding.enabled = false;
            state = "aiming";

            // Change the color of the triangle to indicate that it is charging
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
            Invoke("ResetColor", chargeDuration + attackWindup);
        }
        else if (state == "aiming" && attackingFor >= attackWindup)
        {
            state = "charging";
            Charge();
        }
        else if (state == "charging" && wait >= chargeDuration)
        {
            state = "pathfinding";
            pathfinding.enabled = true;
            attackRecharge = attackCooldown;
            attackingFor = 0;
            wait = 0;

            // Set the triangle's velocity to zero
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        //Now, take action based on the state of the triangle
        if (state == "pathfinding")
        {
            // Return the triangle to its default rotation
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, 0), 300 * Time.deltaTime);

            // Recharge the attack
            attackRecharge -= Time.deltaTime;

            // If the player is outside of the range, move towards a good spot to attack from
            if (playerDistance < range)
            { 
                // Pick the closest spot to the player that is at 75% range
                Vector3 directionToTarget = (player.transform.position - transform.position).normalized;
                Vector3 target = player.transform.position - directionToTarget * range * 0.75f;
                pathfinding.destination = target;
            }
            else
            {
                pathfinding.destination = player.transform.position;
            }
        }
        else if (state == "aiming")
        {
            attackingFor += Time.deltaTime;

            // Cause the triangle to rotate towards the player
            Vector3 directionToTarget = (player.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90), 450 * Time.deltaTime);

            // Cause the triangle to slowly back away from the player
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, -0.75f * Time.deltaTime);

            // Set the target location of the attack to the player's position
            target = player.transform.position;
        }
        else if (state == "charging")
        {
            wait += Time.deltaTime;
        }
        else {
            Debug.Log("Invalid state");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject == player && state == "charging")
        {
            collision.gameObject.GetComponent<PlayerHealth>().Damage(damage);
            gameObject.GetComponent<EnemyHealth>().TakeDamage(1000);
        }
    }

    void ResetColor()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

    void Charge()
    {
        if (state != "charging")
        {
            Debug.Log("Invalid state for charging");
            return;
        }

        // Charge at the player by applying a force in the direction of the target
        Vector3 directionToTarget = (target - transform.position).normalized;
        GetComponent<Rigidbody2D>().AddForce(directionToTarget * chargePace, ForceMode2D.Impulse);
    }


}
