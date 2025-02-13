using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public struct ItemDrop {
    public GameObject item;
    public float weight;
    public WeaponRarity rarity;
}

public class EnemyHealth : MonoBehaviour
{
    public int health = 100;
    public int scoreValue = 10;

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
    public bool dropItem = false;
    public ItemDrop[] items;

    private GameObject Score;
    private ItemSpawner itemSpawner;
    void Start()
    {
        Score = GameObject.Find("Score");
        itemSpawner = GameObject.Find("Spawner").GetComponent<ItemSpawner>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

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
        deathEffect.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        GameObject effect = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(effect, 0.5f);
        Destroy(gameObject);

        // Increase the score when the enemy dies
        Score.GetComponent<ScoreTracker>().IncreaseScore(scoreValue);

        if (dropItem) {
            float[] weights = new float[items.Length];
            for (int i = 0; i < items.Length; i++) {
                weights[i] = items[i].weight;
            }
            int index = ItemSpawner.WeightedRandom(weights);
            ItemDrop itemDrop = items[index];
            itemSpawner.SpawnItem(itemDrop.item, transform.position, itemDrop.rarity);
        }
    }
}