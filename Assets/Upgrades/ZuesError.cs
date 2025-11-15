using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZuesError : MonoBehaviour
{
    [Header("Upgrade Settings")]
    public int damage = 60;
    public int aoeDamage = 60;
    public float roundingError = 0.01f;
    public float timeoutBetweenStrikes = 0f;

    [Header("Prefabs")]
    public GameObject strikePrefab;

    LayerMask enemyLayer;
    float timeout = 1.5f;

    WaitForSeconds lightningDelay = new(0.5f);
    WaitForSeconds aoeDelay = new(0.1f);

    // Start is called before the first frame update
    void Start()
    {
        EnemyHealth.FoeDamaged += (e) => OnFoeDamaged(e.position);
        enemyLayer = LayerMask.GetMask("Foes");
    }

    private void Update()
    {
        if (timeout > 0f)
        {
            timeout -= Time.deltaTime;
        }
    }

    private void OnFoeDamaged(Transform pos)
    {

        // rounding error chance to summon lightning on damage
        if (UnityEngine.Random.value < roundingError & timeout <= 0f)
        {
            Vector3 loc = pos.position;
            StartCoroutine(Fire(loc));
        }
    }

    IEnumerator Fire(Vector3 targetPos)
    {
        Vector3 strikeOffset = new Vector3(0.27f * 0.4f, 10.8f * 0.4f, 0);

        // Create the lightning strikeSFX 
        GameObject lightningStrike = Instantiate(strikePrefab, targetPos + strikeOffset, Quaternion.identity);
        Destroy(lightningStrike, 2f);
        AudioManager.instance.PlaySfx("Lightning");
        timeout = timeoutBetweenStrikes;
        yield return lightningDelay;
        LightningExplosion(targetPos);
        yield return aoeDelay;
        LightningAoE(targetPos);

    }

    private void LightningExplosion(Vector3 targetPos)
    {
        AudioManager.instance.PlaySfx("Explosion", 0.8f);


        // Get all foes directly contacted
        Collider2D[] foes = Physics2D.OverlapCircleAll(targetPos, 0.5f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(damage);
            }
        }

        Invoke("LightningAoE", 0.1f);
    }

    private void LightningAoE(Vector3 targetPos)
    {
        // Get all foes in the AoE
        Collider2D[] foes = Physics2D.OverlapCircleAll(targetPos, 2.4f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(aoeDamage);
            }
        }
    }
}
