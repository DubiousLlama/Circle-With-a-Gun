using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class EnemyDamage : MonoBehaviour
{
    
    public float attackSpeed = 1.5f;
    public float attackDamage = 100f;
    public float attackRange = 2f;
    public float fireDelay = 0f;
    public float mustBeInRangeFor = 0.5f;
    public GameObject LineRenderer;
    public float moveSpeed = 100f;

    public float inRangeTimer = 0f;
    public bool isAttacking = false;

    private GameObject player;
    private FlowFieldController flowFieldController;
    private GameObject attackLine1;
    private GameObject attackLine2;

    private Vector3 line1from = new Vector3();
    private Vector3 line2from = new Vector3();

    private float colorChangeTimer = 0f;

    private Color white = new Color(1f, 1f, 1f, 1f);
    private Color attackColor = new Color(0.7f, 0.03f, 0.15f);

    Transform[] corners;

    void Start()
    {
        flowFieldController = GameObject.Find("PlayArea").GetComponent<FlowFieldController>();
        player = GameObject.Find("PC");
        corners = gameObject.transform.GetChild(1).GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

        float distance = Vector3.Distance(player.GetComponent<Transform>().position, transform.position);

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
            gameObject.GetComponent<SpriteRenderer>().color = white;
        }


        if (distance <= attackRange + 0.5f)
        {
            inRangeTimer += Time.deltaTime;
        } else
        {
            inRangeTimer = 0;
            
            // Destroy all line renderers
            if (isAttacking == true)
            {
                Destroy(attackLine1);
                Destroy(attackLine2);
                isAttacking = false;
            }

            return;
        }

        if (isAttacking == false && inRangeTimer > 0.1f && fireDelay <= mustBeInRangeFor)
        {
            isAttacking = true;

            attackLine1 = Instantiate(LineRenderer, transform.position, Quaternion.identity);
            attackLine2 = Instantiate(LineRenderer, transform.position, Quaternion.identity);
        }

        if (isAttacking)
        {
            line1from = GetCorner(0);
            line2from = GetCorner(1);

            attackLine1.GetComponent<LineController>().SetUpLine(line1from, player.transform.position);
            attackLine2.GetComponent<LineController>().SetUpLine(line2from, player.transform.position);
        }


        if (fireDelay <= 0 && distance <= attackRange && inRangeTimer > mustBeInRangeFor && isAttacking == true)
        {
            Attack();

            Destroy(attackLine1);
            Destroy(attackLine2);
            isAttacking = false;

            gameObject.GetComponent<SpriteRenderer>().color = attackColor;
            colorChangeTimer = 0.2f;
        }
        
    }

    void FixedUpdate()
    {
        // Move towards the player using the flow field
        if (!isAttacking)
        {
            Cell currentCell = flowFieldController.flowField.GetCellFromWorldPosition(transform.position);
            if (currentCell != null && !(currentCell.bestDirectionX == 0 && currentCell.bestDirectionY == 0));
            {
                // Convert the best direction to a normalized vector
                Vector3 direction = new Vector3(currentCell.bestDirectionX, currentCell.bestDirectionY, 0).normalized;

                // Apply a force to the rigidbody in that direction
                GetComponent<Rigidbody2D>().AddForce(direction * moveSpeed);

            }
        }
    }

    private void OnDisable()
    {
        Destroy(attackLine1);
        Destroy(attackLine2);
    }

    void Attack()
    {
        player.GetComponent<PlayerHealth>().Damage(attackDamage);
        fireDelay = attackSpeed;
    }

    // Rank all corners by closeness to the player. Return the corner with the given rank
    private Vector3 GetCorner(int closenessRank)
    {
        SortedList<float, Transform> cornerDistances = new SortedList<float, Transform>();

        // This strange little bit of code is because the first element is the parent's transform
        for (int i = 1; i < corners.Length; i++)
        {
            cornerDistances.Add(Vector3.Distance(player.transform.position, corners[i].position), corners[i]);
        }
        
        return cornerDistances.Values[closenessRank].position;
    }
}
