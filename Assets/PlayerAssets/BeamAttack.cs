using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamAttack : MonoBehaviour
{

    public Transform firePoint;

    [Range(0, 5)]
    public float beamRate = 2f;

    [Range(0f, 0.5f)]
    public float stopTime = 0.1f;

    [Range(4f, 12f)]
    public float range = 6.5f;

    [Range(100, 400)]
    public int damage = 100;

    [Range(1f, 4f)]
    public float beamRadius = 2f;

    public Color beamColor = new Color(0.8257952f, 0f, 1f);

    private GameObject[] foes;

    private float stationaryTime = 0f;
    private float timeSinceLastAttack = 0f;
    int closest;

    public GameObject explosion;

    public GameObject rechargeBar;
    private RechargeBarController barController;
    private AudioManager audioManager;
    PlayerStats playerStats;

    void Start()
    {
        barController = rechargeBar.GetComponent<RechargeBarController>();
        barController.SetMaxRecharge(beamRate);
        audioManager = AudioManager.instance;
        playerStats = GetComponent<PlayerStats>();
    }


    // Update is called once per frame
    void Update()
    {
        // Increase the time since the last attack
        if (timeSinceLastAttack < beamRate)
        {
            timeSinceLastAttack += Time.deltaTime;
            barController.SetRecharge(timeSinceLastAttack);
        }

        // Increase the time the player has been stationary
        if (gameObject.GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f)
        {
            stationaryTime += Time.deltaTime;
        }
        else
        {
            stationaryTime = 0f;
        }

        Attack();

    }

    public bool Attack()
    {
        // If the conditions are not met, return
        if (stationaryTime <= stopTime || timeSinceLastAttack <= beamRate)
        {
            return false;
        }

        foes = GameObject.FindGameObjectsWithTag("Foe");

        // Make a list of all valid targets
        List<GameObject> inSight = new List<GameObject>();
        foreach (GameObject foe in foes)
        {
            if (beamAttackValid(foe))
            {
                inSight.Add(foe);
            }
        }
        
        if (inSight.Count == 0)
        {
            return false;
        }

        // Find the closest foe
        closest = 0;
        float closestDistance = Vector3.Distance(transform.position, inSight[0].transform.position);
        for (int i = 0; i < inSight.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, inSight[i].transform.position);
            if (distance < closestDistance)
            {
                closest = i;
                closestDistance = distance;
            }
        }
        
        Vector3 loc = inSight[closest].transform.position;

        // Summon an explosion at the point of impact
        GameObject e = Instantiate(explosion, loc, Quaternion.identity);
        e.transform.localScale = new Vector3(beamRadius*0.3f*playerStats.ExplosionRadius(), beamRadius*0.3f*playerStats.ExplosionRadius(), 1);

        Destroy(e, 0.5f);

        // Draw the beam
        DrawBeam(loc);

        audioManager.PlaySfx("beam");

        int foesDamaged = 0;
        // Damage each enemy in the radius of the explosion
        foreach (GameObject foe in foes)
        {
            float distance = Vector3.Distance(foe.transform.position, loc);
            if (distance < beamRadius * playerStats.ExplosionRadius())
            {
                int d = distanceToDamage(distance);
                foe.GetComponent<EnemyHealth>().TakeDamage(d);
                foesDamaged++;
            }
        }

        if (foesDamaged > 0)
        {
            // vol should be a value that approaches 1 as the number of foes damaged goes to infinity
            float vol = 0.1f + 0.4f * (1 - Mathf.Exp(-0.4f * foesDamaged));
            StartCoroutine(waitForExplosion(0.1f, vol));
        }
        
        timeSinceLastAttack = 0f;
        return true;
    }

    private IEnumerator waitForExplosion(float delay, float vol)
    {
        yield return new WaitForSeconds(delay);
        audioManager.PlaySfx("Explosion", vol);
    }

    private int distanceToDamage(float distance)
    {
        if (distance < 1)
        {
            return (int)(damage * playerStats.ExplosionDamage());
        }
        else if (distance < 1.5f)
        {
            return (int)(damage * playerStats.ExplosionDamage() * 0.8f);
        }
        else if (distance < 2.5f)
        {
            return (int)(damage * playerStats.ExplosionDamage() * 0.6f);
        }
        else if (distance < 3f)
        {
            return (int)(damage * playerStats.ExplosionDamage() * 0.4f);
        }
        else
        {
            return (int)(damage * playerStats.ExplosionDamage() * 0.2f);
        }
    }
    private void DrawBeam(Vector3 location)
    {
        if (gameObject != null)
        {
            // Create a line renderer attached to the player
            LineRenderer lr = gameObject.AddComponent<LineRenderer>();

            lr.startWidth = 0.1f;
            lr.endWidth = 0.07f;

            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, location);

            lr.startColor = beamColor;
            lr.endColor = beamColor;

            // Destroy the line renderer after 0.4 seconds
            if (beamRate > 0.4f)
            {
                Destroy(lr, 0.4f);
            }
            else
            {
                Destroy(lr, beamRate / 2f);
            }
        }
    }

    private bool beamAttackValid(GameObject foe)
    {
        // Check if the foe is within range
        if (Vector3.Distance(firePoint.position, foe.transform.position) > range)
        {
            return false;
        }

        // Check if the foe is within 45 degrees of the direction the player is facing
        Vector3 direction = firePoint.position - transform.position;
        Vector3 directionToFoe = foe.transform.position - transform.position;
        float angle = Vector3.Angle(direction, directionToFoe);

        if (angle > 45)
        {
            return false;
        }

        // Draw a raycast to the foe, and make sure it is unobstructed
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, directionToFoe, range);
        if (hit.collider != null && hit.collider.gameObject.tag != "Foe")
        {
            return false;
        }

        return true;
    }
}
