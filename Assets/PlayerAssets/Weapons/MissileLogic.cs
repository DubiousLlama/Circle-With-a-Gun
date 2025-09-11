using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class MissileLogic : MonoBehaviour
{
    private float moveSpeed = 0f;
    private float damage = 0f;
    private float blastRadius = 0f;
    private float damageFalloff = 0f; // 4.5 is approx full damage out to 1/2 the radius, then exponential falloff to 0 at the edge of the radius
    private bool isHoming = false;

    private LayerMask enemyLayer;
    private LayerMask physicsLayer;

    public GameObject explosion;
    private bool hasExploded = false;
    private Transform target;

    private float timeAlive = 0f;

    private Rigidbody2D rb;

    public void InitalizeMissile(float dmg, float radius, float move, bool homing,  float falloff = 4.5f)
    {
        damage = dmg;
        blastRadius = radius;
        moveSpeed = move;
        damageFalloff = falloff;
        isHoming = homing;

        if (isHoming)
        {
            target = getHomingTarget();
            Debug.Log("Homing target: " + (target != null ? target.name : "None"));
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyLayer = LayerMask.GetMask("Foes");
        physicsLayer = LayerMask.GetMask("Foes", "Obstacle");

        Debug.Log(Convert.ToString(physicsLayer.value, 2).PadLeft(32, '0'));
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        // Explode on contact with anything in the physics layer
        if ((physicsLayer & (1 << collision.gameObject.layer)) != 0)
        {
            hasExploded = true;
            SummonExplosion();
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        // Explode on contact with anything in the physics layer
        if ((physicsLayer & (1 << collision.gameObject.layer)) != 0)
        {
            hasExploded = true;
            SummonExplosion();
            Explode();
        }
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

            float maxAngle = (Vector3.Distance(target.transform.position, transform.position) < 4) ? 250 : 80;

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90), maxAngle * Time.fixedDeltaTime);
        }

        // Move the missile forward
        rb.velocity = transform.up * moveSpeed * (0.5f + timeAlive * 2f);
    }

    private Transform getHomingTarget()
    {
        float coneAngle = 70f; // Total cone angle
        float raycastDistance = 18f;
        int numberOfRays = 20; // Number of rays to cast across the cone

        Vector2 forward = transform.up; // Assuming missile faces up
        Transform bestTarget = null;
        float bestTargetScore = float.MaxValue;
        bool foundTarget = false;

        // Cast rays across the cone
        for (int i = 0; i < numberOfRays; i++)
        {
            // Calculate angle for this ray (-15° to +15° from forward direction)
            float angle = Mathf.Lerp(-coneAngle / 2f, coneAngle / 2f, (float)i / (numberOfRays - 1));
            Vector2 rayDirection = Quaternion.AngleAxis(angle, Vector3.forward) * forward;

            // Cast the ray
            RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up * 0.6f, rayDirection, raycastDistance, ~physicsLayer);
            Debug.DrawLine(transform.position + transform.up * 0.6f, transform.position + transform.up * 0.6f + (Vector3)(rayDirection * raycastDistance), Color.green, 1f);

            if (hit.collider != null)
            {
                Debug.Log("Ray " + i + " hit: " + hit.transform.name + " on layer " + hit.transform.gameObject.layer + " and the foe layer is " + LayerMask.NameToLayer("Foes"));
                if (hit.transform.tag != "Foe" && hit.transform.parent.tag != "Foe") continue;

                Debug.DrawLine(transform.position, hit.transform.position, Color.red, 1f);

                // Calculate how close this enemy is to the center of the cone
                Vector2 directionToEnemy = (hit.transform.position - transform.position).normalized;
                float angleFromCenter = Vector2.Angle(forward, directionToEnemy);

                float distanceToEnemy = Vector3.Distance(hit.transform.position, transform.position);
                int enemyHealth = hit.transform.GetComponentInParent<EnemyHealth>().health;

                float targetScore = angleFromCenter + distanceToEnemy*10 - (enemyHealth * 0.05f) - (12/(0.5f*distanceToEnemy));

                // If this is the closest enemy to the center of the cone so far
                if (targetScore < bestTargetScore)
                {
                    bestTargetScore = targetScore;
                    bestTarget = hit.transform;
                    foundTarget = true;
                }
            }
        }

        // Draw a magenta line to the selected target
        if (bestTarget != null)
        {
            Debug.DrawLine(transform.position, bestTarget.position, Color.magenta, 2f);
        }

        return foundTarget ? bestTarget : null;
    }

    private void SummonExplosion()
    {
        GameObject b = Instantiate(explosion, transform.position, Quaternion.identity);
        b.transform.localScale = new Vector3(blastRadius * 0.4f, blastRadius * 0.4f, 1);
        Destroy(b, 0.5f);
        AudioManager.instance.PlaySfx("SmallShot", 0.55f);
    }

    private void Explode()
    {
        // Get all foes in the AoE
        Collider2D[] foes = Physics2D.OverlapCircleAll(transform.position, blastRadius, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                Debug.Log("Missile hit foe: " + foe.name);
                float distance = Vector3.Distance(foe.transform.position, transform.position);
                float damageMultiplier = 1 - Mathf.Pow(distance / blastRadius, damageFalloff);
                int aoeDamage = Mathf.Max(0, Mathf.RoundToInt(damage * damageMultiplier));
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(aoeDamage);
            }
        }

        Destroy(gameObject);
    }
}
