using UnityEngine;

public class BombLogic : MonoBehaviour
{
    private float moveSpeed = 0f;
    private float rotationSpeed = 0f; // degrees per second
    private Vector3 launchVector = Vector3.zero;
    private float damage = 0f;
    private float blastRadius = 0f;
    private float damageFalloff = 0f; // 4.5 is approx full damage out to 1/2 the radius, then exponential falloff to 0 at the edge of the radius

    private float timer;
    private LayerMask enemyLayer;

    public GameObject explosion;
    private bool hasExploded = false;
    private float expansionFactor = 0.1f;
    private float expansionTime = 0.25f;
    private float baseScale;

    // Start is called before the first frame update
    public void InitalizeBomb(Vector3 launch, float dmg, float radius, float move, float rot = 360f, float falloff = 4.5f)
    {
        damage = dmg;
        blastRadius = radius;
        moveSpeed = move;
        rotationSpeed = rot;
        damageFalloff = falloff;
        launchVector = launch;
        baseScale = transform.localScale.x;

        gameObject.GetComponent<Rigidbody2D>().AddForce(launchVector * moveSpeed);
        gameObject.gameObject.GetComponent<Rigidbody2D>().rotation = Random.Range(0f, 360f);
        gameObject.GetComponent<Rigidbody2D>().angularVelocity = rotationSpeed * (Random.value > 0.5f ? 1 : -1); // Multiply by either 1 or -1 to get a random direction

    }

    void Start()
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
        // Get all foes in the AoE
        Collider2D[] foes = Physics2D.OverlapCircleAll(transform.position, blastRadius, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                Debug.Log("Bomb hit foe: " + foe.name);
                float distance = Vector3.Distance(foe.transform.position, transform.position);
                float damageMultiplier = 1 - Mathf.Pow(distance / blastRadius, damageFalloff);
                int aoeDamage = Mathf.Max(0, Mathf.RoundToInt(damage * damageMultiplier));
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(aoeDamage);
            }
        }

        Destroy(gameObject);
    }
}
