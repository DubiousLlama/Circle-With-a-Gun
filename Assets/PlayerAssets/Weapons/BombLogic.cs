using System;
using UnityEngine;

public class BombLogic : MonoBehaviour
{
    private float moveSpeed = 0f;
    private float rotationSpeed = 0f; // degrees per second
    private Vector3 launchVector = Vector3.zero;
    private int damage = 0;
    private float blastRadius = 0f;

    private float timer;
    private LayerMask enemyLayer;

    public GameObject explosion;
    private bool hasExploded = false;
    private float expansionFactor = 0.1f;
    private float expansionTime = 0.25f;
    private float baseScale;

    public static event Action<OnBombSecondaryExplosionEventArgs> OnBombSecondaryExplosion;
    public class OnBombSecondaryExplosionEventArgs : EventArgs
    {
        public int foesKilled;
    }

    public void InitalizeBomb(Vector3 launch, int dmg, float radius, float move, float rot = 360f, float falloff = 4.5f)
    {
        damage = dmg;
        blastRadius = radius;
        moveSpeed = move;
        rotationSpeed = rot;
        launchVector = launch;
        baseScale = transform.localScale.x;

        gameObject.GetComponent<Rigidbody2D>().AddForce(launchVector * moveSpeed);
        gameObject.gameObject.GetComponent<Rigidbody2D>().rotation = UnityEngine.Random.Range(0f, 360f);
        gameObject.GetComponent<Rigidbody2D>().angularVelocity = rotationSpeed * (UnityEngine.Random.value > 0.5f ? 1 : -1); // Multiply by either 1 or -1 to get a random direction

    }

    void Awake()
    {
        enemyLayer = LayerMask.GetMask("Foes");
        timer = 0f;
    }

    void Update()
    {
        if (hasExploded) return;

        if (timer < expansionTime)
        {
            float portionElapsed = timer / expansionTime;
            float expansionAmount = (-1 * Mathf.Pow(portionElapsed*2 - 1, 2) + 1) * expansionFactor;
            transform.localScale = new Vector3((1 + expansionAmount) * baseScale, (1 + expansionAmount) * baseScale, 1);
        } else
        {

        }

            timer += Time.deltaTime;
        if (timer >= 4f)
        {
            SummonExplosion();
            Invoke("Explode", 0.25f);
            hasExploded = true;
        }
    }

    private void SummonExplosion()
    {
        GameObject b = Instantiate(explosion, transform.position, Quaternion.identity);
        b.transform.localScale = new Vector3(blastRadius * 0.4f, blastRadius * 0.4f, 1);
        Destroy(b, 0.5f);
        AudioManager.instance.PlaySfx("WooshLightning", 0.45f);
    }

    private void Explode()
    {
        int foesKilled = 0;
        // Get all foes in the AoE
        Collider2D[] foes = Physics2D.OverlapCircleAll(transform.position, blastRadius, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                float distance = Vector3.Distance(foe.transform.position, transform.position);
                if (distance > blastRadius) continue;
                int aoeDamage = 40;
                if (distance < blastRadius * 0.75f) { aoeDamage = 60; }
                if (distance < blastRadius * 0.6f) { aoeDamage = 80; }
                if (distance < blastRadius * 0.5f) { aoeDamage = damage; }

                EnemyHealth eh = foe.GetComponentInParent<EnemyHealth>();
                if (eh != null)
                {
                    if (aoeDamage >= eh.health)
                    {
                        foesKilled++;
                    }
                    eh.TakeDamage(aoeDamage);
                }

            }
        }

        OnBombSecondaryExplosion?.Invoke(new OnBombSecondaryExplosionEventArgs { foesKilled = foesKilled });

        Destroy(gameObject);
    }
}
