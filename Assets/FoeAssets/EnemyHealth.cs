using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Events;

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

    private ItemSpawner itemSpawner;
    private EnemyTracker enemyTracker;

    private bool dead = false;

    public static event Action<OnDeathEventArgs> FoeDied;
    public class OnDeathEventArgs : EventArgs {
        public GameObject enemy;
    }

    void Start()
    {
        GameObject spawner = GameObject.Find("Spawner");
        itemSpawner = spawner.GetComponent<ItemSpawner>();
        enemyTracker = spawner.GetComponent<EnemyTracker>();
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