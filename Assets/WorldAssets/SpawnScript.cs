using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnScript : MonoBehaviour
{
    public GameObject squarePrefab;
    public GameObject trianglePrefab;
    public GameObject octoPrefab;
    public RectTransform playArea;

    [Range(1, 10)]
    public float spawnRate = 2f;

    [Range(3, 20)]
    public float spawnGroupRate = 10f;

    [Range (1, 10)]
    public float spawnTriangleRate = 3f;

    [Range(3, 30)]
    public float spawnOctoRate = 3f;

    [Range(8, 20)]
    public float spawnRange = 10f;

    [Range(0, 0.5f)]
    public float spawnRateVariation = 0.33f;

    private float spawnTimer = 0f;
    private float spawnGroupTimer = 0f;
    private float spawnTriangleTimer = 0f;
    private float spawnOctoTimer = 0f;

    // Prevent this from being modified
    [ContextStatic]
    public float difficulty = 1f;
    private GameObject player;

    private void Start()
    {
        player = GameObject.Find("PC");
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        difficulty += Time.deltaTime * 0.005f;


        spawnTimer -= (Time.deltaTime * difficulty);
        spawnGroupTimer -= (Time.deltaTime * difficulty);
        spawnTriangleTimer -= (Time.deltaTime * difficulty);
        spawnOctoTimer -= (Time.deltaTime * difficulty);

        if (spawnTimer <= 0)
        {
            SpawnEnemy(squarePrefab);

            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation*spawnRate, spawnRateVariation * spawnRate);
            spawnTimer = spawnRate + randomDelay;
        }

        if (spawnGroupTimer <= 0)
        {
            int groupSize = UnityEngine.Random.Range(2, 5);
            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation * spawnGroupRate, spawnRateVariation * spawnGroupRate);
            SpawnEnemyGroup(squarePrefab, groupSize);
            spawnGroupTimer = spawnGroupRate + randomDelay;
        }

        if (spawnTriangleTimer <= 0)
        {
            SpawnEnemy(trianglePrefab);
            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation * spawnTriangleRate, spawnRateVariation * spawnTriangleRate);
            spawnTriangleTimer = spawnTriangleRate + randomDelay;
        }
        if (spawnOctoTimer <= 0)
        {
            SpawnEnemy(octoPrefab);
            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation * spawnOctoRate, spawnRateVariation * spawnOctoRate);
            spawnOctoTimer = spawnOctoRate + (randomDelay * 2);
        }
        
    }

    void SpawnEnemy(GameObject enemy, int i = 0)
    {

        if (i > 50)
        {
            Debug.Log("SpawnGroup Error: Could not find a valid spawn position");
            return;
        }

        //Generate a random float between -2 and 5
        float spawnDistVariation = UnityEngine.Random.Range(-2f, 5f);

        // Get the player's position in 2d space
        Vector2 playerPos = player.GetComponent<Transform>().position;

        // Spawn an enemy at a random position appoximately SpawnRange units away from the player
        Vector2 spawnPosition =  playerPos + UnityEngine.Random.insideUnitCircle * (spawnRange + spawnDistVariation);

        // Check if the spawn position intersects with any other colliders
        Collider2D hitCollider = Physics2D.OverlapPoint(spawnPosition);
        if (hitCollider != null && hitCollider.gameObject.tag != "PowerUp")
        {
            SpawnEnemy(enemy, i+1);
            return;
        }

        // Check if the enemy is within the play area
        if (!playArea.rect.Contains(spawnPosition - (Vector2)playArea.position))
        {
            SpawnEnemy(enemy, i + 1);
            return;
        }

        // Instantiate the enemy prefab at the spawn position
        Vector3 v3 = spawnPosition;
        Spawn(v3, enemy);
    }

    void SpawnEnemyGroup(GameObject enemy, int numFoes, int j = 0)
    {
        if (j > 50)
        {
            Debug.Log("SpawnGroup Error: Could not find a valid spawn position");
            return;
        }

        //Generate a random float between 4 and 6
        float spawnDistVariation = UnityEngine.Random.Range(4f, 6f);

        // Get the player's position in 2d space
        Vector2 playerPos = player.GetComponent<Transform>().position;

        // Generate the group spawn position
        Vector2 spawnPosition = playerPos + UnityEngine.Random.insideUnitCircle * (spawnRange + spawnDistVariation);

        Vector2[] spawnPositions = new Vector2[numFoes];
        spawnPositions[0] = spawnPosition;


        // Check if the spawn position intersects with any other colliders
        Collider2D hitCollider = Physics2D.OverlapPoint(spawnPosition);
        
        if (hitCollider != null && hitCollider.gameObject.tag != "PowerUp")
        {
            SpawnEnemyGroup(enemy, numFoes, j+1);
            return;
        }

        if (!playArea.rect.Contains(spawnPosition - (Vector2)playArea.position))
        {
            SpawnEnemyGroup(enemy, numFoes, j + 1);
            return;
        }

        for (int i = 1; i < numFoes; i++)
        {
            int l = 0;
            while (true) {
                // Spawn each enemy at a random position 3 units away from the previous enemy
                spawnPositions[i] = spawnPositions[i - 1] + (UnityEngine.Random.insideUnitCircle * 3f);

                // Check if the spawn position intersects with any other colliders
                hitCollider = Physics2D.OverlapPoint(spawnPosition);
                l += 1;

                if ((hitCollider == null ||  hitCollider.gameObject.tag == "PowerUp") && playArea.rect.Contains(spawnPosition - (Vector2)playArea.position))
                {
                    break;
                }
                if (l > 20)
                {
                    Debug.Log("SpawnGroup Error: Could not find a valid spawn position");
                    return;
                }
            }
        }

        // Instantiate each enemy prefab at its spawn position
        for (int i = 0; i < numFoes; i++)
        {
            Vector3 v3 = spawnPositions[i];
            Spawn(v3, enemy);
        }
    }
    
    void Spawn(Vector3 spawnPosition, GameObject enemy)
    {
        // First, check if the spawn position is within 5 units of the player
        if (Vector3.Distance(spawnPosition, player.GetComponent<Transform>().position) < 5f)
        {
            return;
        }

        Instantiate(enemy, spawnPosition, Quaternion.identity);
    }
}


