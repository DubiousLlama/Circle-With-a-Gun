using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NoAimAttacks2 : MonoBehaviour
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

    public GameObject rechargeBar;

    private float beamCooldown = 0f;
    private RechargeBarController barController;
    private GameObject[] foes;
    private int closest;
    public float stationaryTime = 0f;


    private LineRenderer lr;

    void Start()
    {
        barController = rechargeBar.GetComponent<RechargeBarController>();
        barController.SetMaxRecharge(beamRate);
    }

    // Update is called once per frame
    void Update()
    {
        beamCooldown += Time.deltaTime;
        barController.SetRecharge(beamCooldown);

        if (gameObject.GetComponent<Rigidbody2D>().velocity.magnitude < 0.1f)
        {
            stationaryTime += Time.deltaTime;
        }
        else
        {
            stationaryTime = 0f;
        }

        // Get all the objects with the tag "Foe"
        foes = GameObject.FindGameObjectsWithTag("Foe");

        if (foes.Length == 0)
        {
            return;
        }

        // check if the attack is off cooldown and the player is stationary
        if (stationaryTime > 0.1f)
        {
            if (closest == -1)
            {
                closest = closestFoe(foes);
            }


            if (closest != -1 && Vector3.Distance(transform.position, foes[closest].transform.position) < range)
            {
                // Create a line renderer from the player to the targe foe. Slowly increase the width of the line renderer over time

                if (lr == null)
                {
                    lr = CreateBeam();
                }

                if (beamCooldown > beamRate && beamAttackValid(foes[closest]))
                {
                    BeamAttack(foes[closest]);
                    beamCooldown = 0f;
                    stationaryTime = 0f;
                    closest = -1;
                    lr = null;

                }
            }
            else
            {
                closest = -1;
                if (lr != null)
                {
                    Destroy(lr);
                    lr = null;
                }
            }
        }
        else
        {
            closest = -1;
            if (lr != null)
            {
                Destroy(lr);
                lr = null;
            }
        }

        if (lr != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, foes[closest].transform.position);

            // Increase the width of the line renderer over time
            lr.startWidth += Time.deltaTime * 0.1f;
            lr.endWidth += Time.deltaTime * 0.1f;
        }


    }

    // Assumes that foes is not empty
    int closestFoe(GameObject[] foes)
    {

        // Find the closest foe
        int closest = 0;
        bool oneInSight = false;
        for (int i = 0; i < foes.Length; i++)
        {
            // check if the foe is within 45 degrees of the player's forward direction
            Vector3 direction = (foes[i].transform.position - transform.position).normalized;
            if (Vector3.Angle(firePoint.up, direction) > 45)
            {
                continue;
            }
            else
            {
                oneInSight = true;
            }

            if (Vector3.Distance(transform.position, foes[i].transform.position) < Vector3.Distance(transform.position, foes[closest].transform.position))
            {
                closest = i;
            }
        }

        if (!oneInSight)
        {
            return -1;
        }

        return closest;

    }

    LineRenderer CreateBeam()
    {
        lr = new GameObject("Beam").AddComponent<LineRenderer>();
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.startColor = beamColor;
        lr.endColor = beamColor;
        return lr;
    }

    bool beamAttackValid(GameObject foe)
    {
        // check if we are moving
        if (GetComponent<Rigidbody2D>().velocity.magnitude > stopTime)
        {
            return false;
        }

        if (Vector3.Distance(firePoint.position, foe.transform.position) > range)
        {
            return false;
        }

        if (stationaryTime < 0.1f)
        {
            return false;
        }

        return true;

    }

    void BeamAttack(GameObject foe)
    {
        // Destroy the line renderer after 0.4 seconds
        if (beamRate > 0.4f)
        {
            Destroy(lr, 0.4f);
        }
        else
        {
            Destroy(lr, beamRate / 2f);
        }

        // Deal damage to the foe
        foe.GetComponent<EnemyHealth>().TakeDamage(damage);

        // Get all foes within 3 units of the foe
        Collider2D[] foesInRange = Physics2D.OverlapCircleAll(foe.transform.position, beamRadius);

        // Deal 40 damage to each of them
        foreach (Collider2D foeInRange in foesInRange)
        {
            if (foeInRange.gameObject.tag == "Foe")
            {

                //Get the distance between the foe and the foeInRange
                float distance = Vector3.Distance(foe.transform.position, foeInRange.transform.position);

                if (distance > 0.75f * beamRadius)
                {
                    foeInRange.gameObject.GetComponent<EnemyHealth>().TakeDamage((int)(damage * 0.4f));
                }
                if (distance > 0.4f * beamRadius)
                {
                    foeInRange.gameObject.GetComponent<EnemyHealth>().TakeDamage((int)(damage * 0.6f));
                }
                else
                {
                    foeInRange.gameObject.GetComponent<EnemyHealth>().TakeDamage((int)(damage * 0.8f));
                }

            }
        }
    }
}
