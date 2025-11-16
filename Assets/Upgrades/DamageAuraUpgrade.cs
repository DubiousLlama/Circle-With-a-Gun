using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageAuraUpgrade : MonoBehaviour
{
    public int damagePerTick = 20;
    public float auraRadius = 9f;
    public float timePerTick = 0.8f;
    public bool auraActive = true;

    WaitForSeconds shortWait;
    WaitForSeconds longWait;
    GameObject auraCircle;
    Color flash = new Color(1f, 1f, 1f, 0.1f);
    Color transparent = new Color(0f, 0f, 0f, 0.1f);
    SpriteRenderer auraSpriteRenderer;
    int rotateDirection = -1;

    // Update is called once per frame
    void Start()
    {
        shortWait = new WaitForSeconds(timePerTick / 10);
        longWait = new WaitForSeconds(timePerTick - (timePerTick / 10));
        auraCircle = Resources.Load<GameObject>("DamageAuraCircle");
        auraCircle = Instantiate(auraCircle, transform.parent);
        auraCircle.transform.localScale = new Vector3(auraRadius * 1.2f, auraRadius * 1.2f, 1);
        auraSpriteRenderer = auraCircle.GetComponent<SpriteRenderer>();
        StartCoroutine(DamageAura());
        transform.localPosition = Vector3.zero;
    }

    private void Update()
    {

        // Rotate the aura circle slowly
        auraCircle.transform.Rotate(0f, 0f, 40f * Time.deltaTime * rotateDirection);
    }

    IEnumerator DamageAura()
    {
        while (auraActive)
        {
            bool dealtDamage = false;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, auraRadius);
            foreach (var hitCollider in hitColliders)
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damagePerTick);
                    dealtDamage = true;
                }
            }
            // Flash the aura circle red briefly
            if (dealtDamage) { auraSpriteRenderer.color = flash; }
            yield return shortWait;
            auraSpriteRenderer.color = transparent;
            // 5% chance of reversing rotation direction
            if (Random.value <= 0.05f && dealtDamage){ rotateDirection *= -1;}
            yield return longWait;
        }
    }

    void OnDestroy()
    {
        auraActive = false;
        StopAllCoroutines();
        if (auraCircle != null)
        {
            Destroy(auraCircle);
        }
    }
}
