using System.Collections;
using UnityEngine;

public class OctoController : MonoBehaviour
{
    [Range(5, 16)]
    public float range;

    [Range(2, 30)]
    public float attackCooldown;

    [Range (0.1f, 4)]
    public float attackWindup;

    [Range(4, 12)]
    public float attackLead;

    public GameObject AoE;
    public GameObject projectile;

    GameObject player;
    AudioManager audioManager;
    Pathfinding.AIPath pathfinding;
    SpriteRenderer spriteRenderer;
    private string state;
    private float attackRecharge = 0;
    private float attackingFor = 0;
    private Vector3 target;
    private float OctoSize = 0.6f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PC");
        pathfinding = GetComponent<Pathfinding.AIPath>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        state = "pathfinding";
        audioManager = AudioManager.instance;

        if (pathfinding == null)
        {
            Debug.LogError("No AIPath component found on octo");
        }
        if (player == null)
        {
            Debug.LogError("No Player found");
        }

        transform.localScale = new Vector3(OctoSize, OctoSize, 1);
    }

    // Update is called once per frame
    void Update()
    {
        //First, set the state of the octo
        float playerDistance = Vector2.Distance(transform.position, player.transform.position);

        if (playerDistance < range && state == "pathfinding" && attackRecharge >= attackCooldown)
        {
            state = "windup";
            spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f);

            Invoke("ResetOcto", attackWindup + 0.2f);
            
            // Disable pathfinding
            pathfinding.enabled = false;
        }
        else if (state == "windup" && attackingFor >= attackWindup)
        {

            target = GetOctoTarget();
            audioManager.PlaySfx("Woosh");
            StartCoroutine(Attack(target));

            attackingFor = 0;
            attackRecharge = 0;
            Invoke("EnablePathfinding", 0.2f);
            state = "pathfinding";
        }



        // Now, act according to the state of the octo
        if (state == "pathfinding")
        {
            pathfinding.destination = player.transform.position;

            if (attackRecharge < attackCooldown)
            {
                attackRecharge += Time.deltaTime;
            }

            if (transform.localScale.x < 1)
            {
                // Slowly return the octo to its original size
                transform.localScale = Vector3.Slerp(transform.localScale, new Vector3(OctoSize, OctoSize, 1), 0.2f);
            }
            
        }
        else if (state == "windup")
        {
            if (attackingFor < attackWindup)
            {
                attackingFor += Time.deltaTime;
                //Gradually shrink the octo's size to indicate that it is charging
                transform.localScale = new Vector3(OctoSize - 0.15f * OctoSize * (attackingFor / attackWindup), OctoSize - 0.15f * OctoSize * (attackingFor / attackWindup), 1);
            }
        }
    }

    private void EnablePathfinding()
    {
        pathfinding.enabled = true;
    }

    private void ResetOcto()
    {
        spriteRenderer.color = Color.white;
    }

    private Vector3 GetOctoTarget()
    {
        // Get the player's position
        Vector3 playerPos = player.transform.position;

        // Get the player's velocity
        Vector3 playerVel = player.GetComponent<Rigidbody2D>().linearVelocity;

        playerVel = playerVel.normalized;
        
        // Get a location 9 units in front of the player
        Vector3 ledSpot = playerPos + playerVel * attackLead;

        ledSpot.z = 2;

        return ledSpot;
    }

    private IEnumerator Attack(Vector3 target)
    {
        Vector3 OctoPos = transform.position;
        Vector3 direction = (target - OctoPos).normalized;
        Vector3 projectilePos = OctoPos + direction * 0.5f;

        while (projectilePos != target)
        {
            // Instantiate a projectile
            GameObject CurrentProjectile = Instantiate(projectile, projectilePos, Quaternion.identity);
            
            // Rotate the projectile to a random X angle

            CurrentProjectile.transform.Rotate(0, 0, Random.Range(-360, 360));
            // StartCoroutine(FadeOut(CurrentProjectile));
            Destroy(CurrentProjectile, 2f);

            projectilePos = Vector3.MoveTowards(projectilePos, target, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }

        GameObject CurrentAoE = Instantiate(AoE, target, Quaternion.identity);
        Destroy(CurrentAoE, 3.417f);
        yield return new WaitForSeconds(1f);
        CurrentAoE = Instantiate(AoE, target, Quaternion.identity);
        Destroy(CurrentAoE, 3.417f);
    }

}


