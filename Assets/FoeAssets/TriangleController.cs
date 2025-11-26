using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TriangleController : MonoBehaviour
{
    public int damage = 400;

    public float range = 7f;

    public float attackWindup = 1f;

    public float attackCooldown = 1;

    public float chargePace = 8;

    public float chargeDuration = 3f;

    public bool legendaryFoe = true;

    private string state;
    private GameObject player;
    private float attackRecharge = 0;
    private float attackingFor = 0;
    private float wait = 0;
    private Pathfinding.AIPath pathfinding;
    private Vector3 target;
    private bool dealtDamage = false;

    //private int messageTrack = 0;
    
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

        if (state == "pathfinding" && range / 2 < playerDistance && playerDistance < range  && attackRecharge <= 0)
        {
            pathfinding.enabled = false;
            state = "aiming";

            // Change the color of the triangle to indicate that it is charging
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.8f, 0.8f, 0.8f);
            Invoke("ResetColor", chargeDuration + attackWindup);


            //Debug.Log(messageTrack + ": State Change from Pathfinding to Aiming");
            //messageTrack++;
        }
        else if (state == "aiming" && attackingFor >= attackWindup)
        {
            wait = 0;
            state = "charging";

            // Lock the rotation of the foe
            gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;

            Charge();

            if (legendaryFoe)
            {
                // Disable the PhysicsCollider child
                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(true);

                // Ignore Physics collisions with all foes
                gameObject.layer = LayerMask.NameToLayer("LegendaryFoes");
            }

            //Debug.Log(messageTrack + ": State Change from Aiming to Charging");
            //messageTrack++;
        }
        else if (state == "charging" && wait >= chargeDuration)
        {
            gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;

            state = "pathfinding";
            pathfinding.enabled = true;
            attackRecharge = attackCooldown;
            attackingFor = 0;
            wait = 0;
            dealtDamage = false;

            // Set the triangle's velocity to zero
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            if (legendaryFoe)
            {
                // Re-enable the PhysicsCollider child
                transform.GetChild(0).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);

                // Restore Physics collisions with all foes
                gameObject.layer = LayerMask.NameToLayer("Foes");
            }

            //Debug.Log(messageTrack + ": State Change from Charging to Pathfinding");
            //messageTrack++;
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
        if (collision.gameObject == player && state == "charging" && !dealtDamage)
        {
            dealtDamage = true;
            collision.gameObject.GetComponent<PlayerHealth>().Damage(damage);
            if (!legendaryFoe) { gameObject.GetComponent<EnemyHealth>().TakeDamage(1000); }
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
        }
        if (collision.gameObject.tag == "Wall" && state == "charging")
        {
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
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
