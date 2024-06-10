using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    
    public float attackSpeed = 1.5f;
    public float attackDamage = 100f;
    public float attackRange = 2f;
    public float fireDelay = 0f;
    public float mustBeInRangeFor = 0.5f;
    public GameObject LineRenderer;

    public float inRangeTimer = 0f;

    public bool isAttacking = false;

    private GameObject player;
    private Pathfinding.AIPath pathfinding;
    private GameObject attackLine1;
    private GameObject attackLine2;

    private float colorChangeTimer = 0f;

    void Start()
    {
        player = GameObject.Find("PC");
        pathfinding = GetComponent<Pathfinding.AIPath>();
    }

    // Update is called once per frame
    void Update()
    {
        pathfinding.destination = player.transform.position;

        float distance = Vector3.Distance(player.GetComponent<Transform>().position, transform.position);

        if (distance <= attackRange + 0.5f)
        {
            inRangeTimer += Time.deltaTime;
        }
        else
        {
            inRangeTimer = 0;
            
            // Destroy all line renderers
            if (isAttacking == true)
            {
                Destroy(attackLine1);
                Destroy(attackLine2);
                isAttacking = false;
            }
        }

        if (fireDelay > 0)
        {
            fireDelay -= Time.deltaTime;
        }
        if (colorChangeTimer > 0)
        {
            colorChangeTimer -= Time.deltaTime;
        }
        if (colorChangeTimer <= 0)
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        if (inRangeTimer > 0.1f && isAttacking == false && fireDelay <= mustBeInRangeFor)
        {
            isAttacking = true;
            Transform[] twoClosest = getTwoClosest();

            Transform[] line1 = new Transform[2];
            line1[0] = twoClosest[0];
            line1[1] = player.GetComponent<Transform>();
            Transform[] line2 = new Transform[2];
            line2[0] = twoClosest[1];
            line2[1] = player.GetComponent<Transform>();

            attackLine1 = Instantiate(LineRenderer, transform.position, Quaternion.identity);
            attackLine2 = Instantiate(LineRenderer, transform.position, Quaternion.identity);

            attackLine1.GetComponent<LineController>().SetUpLine(line1);
            attackLine2.GetComponent<LineController>().SetUpLine(line2);
        }


        if (fireDelay <= 0 && distance <= attackRange && inRangeTimer > mustBeInRangeFor && isAttacking == true)
        {
            Attack();

            Destroy(attackLine1);
            Destroy(attackLine2);
            isAttacking = false;

            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.03f, 0.15f);
            colorChangeTimer = 0.2f;

            

        }
        
    }

    void Attack()
    {
        player.GetComponent<PlayerHealth>().Damage(attackDamage);
        fireDelay = attackSpeed;
    }

    private Transform[] getTwoClosest()
    {
        // Identify the two closest corners of the enemy
        Transform[] transform_list = gameObject.transform.GetChild(1).GetComponentsInChildren<Transform>();
        float[] cornerDistances = new float[transform_list.Length];


        for (int i = 0; i < transform_list.Length; i++)
        {
            cornerDistances[i] = (Vector3.Distance(player.GetComponent<Transform>().position, transform_list[i].position));
        }

        // Get the indices of the two smallest elements of cornerDistances
        int[] indices = new int[2];
        float biggest = 0;
        float secondBiggest = 0;

        for (int i = 0; i < cornerDistances.Length; i++)
        {
            
            if (cornerDistances[i] > biggest)
            {
                indices[1] = indices[0];
                secondBiggest = biggest;
                indices[0] = i;
                biggest = cornerDistances[i];
            }

            else if (cornerDistances[i] > secondBiggest)
            {
                indices[1] = i;
                secondBiggest = cornerDistances[i];
            }
        }


        Transform[] twoClosest = new Transform[2];
        twoClosest[0] = transform_list[indices[0]];
        twoClosest[1] = transform_list[indices[1]];


        return twoClosest;
    }
}
