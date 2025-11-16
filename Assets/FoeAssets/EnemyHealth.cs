using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public int scoreValue = 10;
    private int startingHealth;

    public int xpMax = 1;
    public int xpMin = 1;
    [Range(0.0f, 1.0f)]
    public float xpDropChance = 1.0f; // 0.0 to 1.0

    public GameObject xpOrb;

    public GameObject deathEffect;

    public Sprite damage1;
    public Sprite damage2;
    public Sprite damage3;

    [OptionalField]
    public Sprite damage4;

    [OptionalField]
    public Sprite damage5;

    [OptionalField]
    public int threshold1 = 60;
    [OptionalField]
    public int threshold2 = 40;
    [OptionalField]
    public int threshold3 = 20;
    [OptionalField]
    public int threshold4 = 10;
    [OptionalField]
    public int threshold5 = 0;

    private EnemyTracker enemyTracker;
    private SpriteRenderer spriteRenderer;
    private AIPath aiPath;

    private bool dead = false;
    Coroutine statusEffect = null;
    private WaitForSeconds flashTime = new (0.1f);
    private WaitForSeconds poisonDamageTime = new (0.4f);
    private WaitForSeconds freezeDamageTime = new (0.9f);
    private Color poisonedColor = new Color(0.3618068f, 0f, 04264151f, 0.4f);
    private Color poisonedFlashColor = new Color(0.3618068f, 0f, 04264151f, 0.6705883f);
    private Color frozenColor = new Color(0.6169811f, 1f, 0.9899716f, 0.64f);
    private Color frozenFlashColor = new Color(0f, 0.6622642f, 1f, 0.64f);


    public static event Action<OnDeathEventArgs> FoeDied;
    public class OnDeathEventArgs : EventArgs {
        public GameObject enemy;
    }

    public static event Action<OnDamageEventArgs> FoeDamaged;
    public class OnDamageEventArgs : EventArgs
    {
        public Transform position;
    }
    OnDamageEventArgs recentFoeDamaged = new OnDamageEventArgs();

    void Start()
    {
        GameObject spawner = GameObject.Find("Spawner");
        enemyTracker = spawner.GetComponent<EnemyTracker>();
        spriteRenderer = transform.Find("EffectDisplay").GetComponent<SpriteRenderer>();
        aiPath = GetComponent<AIPath>();
        startingHealth = health;
    }

    public void ApplyPoison(float duration)
    {
        if (statusEffect != null)
        {
            StopCoroutine(statusEffect);
        }
        statusEffect = StartCoroutine(Poisioned(duration));
    }

    public void ApplyFreeze(float duration)
    {
        if (statusEffect != null)
        {
            StopCoroutine(statusEffect);
        }
        statusEffect = StartCoroutine(Frozen(duration));
    }

    IEnumerator Poisioned(float duration)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = poisonedColor;
        }

        yield return flashTime;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (spriteRenderer != null) { spriteRenderer.color = poisonedFlashColor; }
            TakeDamage(startingHealth/10);
            elapsed += 0.1f;
            yield return flashTime;
            if (spriteRenderer != null) { spriteRenderer.color = poisonedColor; }
            elapsed += 0.4f;
            yield return poisonDamageTime;
        }
        statusEffect = null;
        spriteRenderer.enabled = false;
    }

    IEnumerator Frozen(float duration)
    {
        float origionalSpeed = 0;
        float origionalDuration = 0;
        int origionalDamage = 0;
        TriangleController triangleController = GetComponent<TriangleController>();
        if (triangleController != null)
        {
            origionalDuration = triangleController.chargeDuration;
            origionalSpeed = triangleController.chargePace;
            origionalDamage = triangleController.damage;
            triangleController.chargeDuration *= 1.25f;
            triangleController.chargePace = 3.5f;
            triangleController.damage = triangleController.damage / 2;
        }


        float originalSpeed = 0;
        if (aiPath != null)
        {
            originalSpeed = aiPath.maxSpeed;
            aiPath.maxSpeed *= 0.25f;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = frozenColor;
        }

        yield return flashTime;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (spriteRenderer != null) { spriteRenderer.color = frozenFlashColor; }
            TakeDamage(startingHealth / 20);
            elapsed += 0.1f;
            yield return flashTime;
            if (spriteRenderer != null) { spriteRenderer.color = frozenColor; }
            elapsed += 0.9f;
            yield return freezeDamageTime;
        }

        if (aiPath != null)
        {
            aiPath.maxSpeed = originalSpeed;
        }
        if (triangleController != null)
        {
            triangleController.chargeDuration = origionalDuration;
            triangleController.chargePace = origionalSpeed;
            triangleController.damage = origionalDamage;
        }

        statusEffect = null;
        spriteRenderer.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        recentFoeDamaged.position = transform;
        FoeDamaged?.Invoke(recentFoeDamaged);

        if (health <= threshold1)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = damage1;
        }

        if (health <= threshold2)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = damage2;
        }

        if (health <= threshold3)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = damage3;
        }

        if (health <= threshold4 && damage4 != null)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = damage4;
        }

        if (health <= threshold5 && damage5 != null)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = damage5;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (dead)
        {
            return;
        }

        enemyTracker.UnregisterEnemy(gameObject);

        dead = true;
        deathEffect.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(effect, 0.5f);
        Destroy(gameObject);

        FoeDied?.Invoke(new OnDeathEventArgs { enemy = gameObject });

        // Spawn XP orbs
        List<int> orbs = DetermineXPOrbs();
        foreach (int xp in orbs)
        {
            if (xp == 0)
                continue;

            // Get a random position within a radius of 0.4 units
            float scale = UnityEngine.Random.Range(0.2f, 0.4f);
            Vector3 offset = UnityEngine.Random.insideUnitCircle * scale;

            offset.z = 1;


            GameObject orb = Instantiate(xpOrb, transform.position + offset, Quaternion.identity);
            orb.GetComponent<XPPickup>().SetXPAmount(xp);
        }
    }

    List<int> DetermineXPOrbs()
    {
        // Determine whether to drop XP
        if (UnityEngine.Random.value > xpDropChance)
        {
            return new List<int> { 0 };
        }
        int totalXP = UnityEngine.Random.Range(xpMin, xpMax + 1);
        List<int> orbs = new List<int>();

        if (totalXP <= 5 || UnityEngine.Random.value < 0.1f)
        {
            orbs.Add(totalXP);
            return orbs;
        }
        
        if (totalXP < 10 || UnityEngine.Random.value < 0.25f)
        {
            int orb1 = UnityEngine.Random.Range(2, totalXP);
            orbs.Add(orb1);
            orbs.Add(totalXP - orb1);
            return orbs;

        } else
        {
            int orb1 = UnityEngine.Random.Range(2, Mathf.RoundToInt(totalXP * 0.5f));
            int orb2 = UnityEngine.Random.Range(2, Mathf.RoundToInt((totalXP - orb1) * 0.75f));
            orbs.Add(orb1);
            orbs.Add(orb2);
            orbs.Add(totalXP - orb1 - orb2);
        }

        return orbs;
    }
}