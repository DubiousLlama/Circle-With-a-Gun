using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnScript : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject squarePrefab;
    public GameObject trianglePrefab;
    public GameObject octoPrefab;
    public GameObject trapzPrefab;
    public RectTransform playArea;

    [Header("Settings")]
    public float difficulty = 1f;

    [Range(0f, 0.1f)]
    public float difficultyIncrease = 0.008f;

    [Range(1, 10)]
    public float spawnRate = 2f;

    [Range(3, 20)]
    public float spawnGroupRate = 10f;

    [Range (1, 10)]
    public float spawnTriangleRate = 3f;

    [Range(8, 30)]
    public float spawnOctoRate = 3f;

    [Range(5, 20)]
    public float spawnTrapzRate = 10f;

    [Range(8, 20)]
    public float spawnRange = 10f;

    [Range(0, 0.5f)]
    public float spawnRateVariation = 0.33f;

    private float spawnTimer = 0f;
    private float spawnGroupTimer = 0f;
    private float spawnTriangleTimer = 0f;
    private float spawnOctoTimer = 0f;
    private float spawnTrapzTimer = 0f;

    private GameObject player;
    private EnemyTracker enemyTracker;

    private bool isHardMode = false;

    private void Start()
    {
        player = GameObject.Find("PC");
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
        enemyTracker = GetComponent<EnemyTracker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyTracker.enableSpawning == false) { return; }

        difficulty += Time.deltaTime * difficultyIncrease;

        if (difficulty > 2.5f && !isHardMode)
        {
            difficultyIncrease *= 0.25f;
            isHardMode = true;
        }

        spawnTimer -= (Time.deltaTime * difficulty * (isHardMode ? 0.5f : 1f));
        spawnGroupTimer -= (Time.deltaTime * difficulty * (isHardMode ? 0.25f : 1f));
        spawnTriangleTimer -= (Time.deltaTime * difficulty * (isHardMode ? 2f : 1f));
        spawnOctoTimer -= (Time.deltaTime * difficulty * (isHardMode ? 1.2f : 1f));
        spawnTrapzTimer -= (Time.deltaTime * difficulty * (isHardMode ? 1.4f : 1f));

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

        if (spawnTriangleTimer <= 0 && difficulty > 1.1f)
        {
            SpawnEnemy(trianglePrefab);
            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation * spawnTriangleRate, spawnRateVariation * spawnTriangleRate);
            spawnTriangleTimer = spawnTriangleRate + randomDelay;
        }
        if (spawnOctoTimer <= 0 && difficulty > 1.3f)
        {
            SpawnEnemy(octoPrefab);
            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation * spawnOctoRate, spawnRateVariation * spawnOctoRate);
            spawnOctoTimer = spawnOctoRate + (randomDelay * 2);
        }
        if (spawnTrapzTimer <= 0 && difficulty > 1.6f)
        {
            SpawnEnemy(trapzPrefab);
            float randomDelay = UnityEngine.Random.Range(-1 * spawnRateVariation * spawnOctoRate, spawnRateVariation * spawnOctoRate);
            spawnTrapzTimer = spawnTrapzRate + (randomDelay * 2);
        }

    }

    public GameObject SpawnEnemy(GameObject enemy, int i = 0, bool close = false)
    {

        if (i > 50)
        {
            Debug.Log("SpawnGroup Error: Could not find a valid spawn position");
            return null;
        }

        //Generate a random float between -2 and 5
        float spawnDistVariation = UnityEngine.Random.Range(-2f, 5f);

        // Get the player's position in 2d space
        Vector2 playerPos = player.GetComponent<Transform>().position;

        // Spawn an enemy at a random position appoximately SpawnRange units away from the player
        if (close) {spawnDistVariation = -2f; }
        Vector2 spawnPosition =  playerPos + UnityEngine.Random.insideUnitCircle * (spawnRange + spawnDistVariation);

        // Check if the spawn position intersects with any other colliders
        Collider2D hitCollider = Physics2D.OverlapPoint(spawnPosition);
        if (hitCollider != null && hitCollider.gameObject.tag != "Item")
        {
            return SpawnEnemy(enemy, i + 1, close);
        }

        // Check if the enemy is within the play area
        if (!playArea.rect.Contains(spawnPosition - (Vector2)playArea.position))
        {
            
            return SpawnEnemy(enemy, i + 1, close);
        }

        // Instantiate the enemy prefab at the spawn position
        Vector3 v3 = spawnPosition;
        GameObject spawnedEnemy = Spawn(v3, enemy);

        // Get the EnemyTracker component from the GameObject this script is attached to
        EnemyTracker enemyTracker = GetComponent<EnemyTracker>();
        if (enemyTracker != null)
        {
            enemyTracker.RegisterEnemy(spawnedEnemy);
        }
        else
        {
            Debug.LogWarning("EnemyTracker component not found on SpawnScript GameObject.");
        }

        return spawnedEnemy;
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
        
        if (hitCollider != null && hitCollider.gameObject.tag != "Item")
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

                if ((hitCollider == null ||  hitCollider.gameObject.tag == "Item") && playArea.rect.Contains(spawnPosition - (Vector2)playArea.position))
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
            GameObject spawnedEnemy = Spawn(v3, enemy);

            // Get the EnemyTracker component from the GameObject this script is attached to
            ;
            if (enemyTracker != null)
            {
                enemyTracker.RegisterEnemy(spawnedEnemy);
            }
            else
            {
                Debug.LogWarning("EnemyTracker component not found on SpawnScript GameObject.");
            }
        }
    }
    
    GameObject Spawn(Vector3 spawnPosition, GameObject enemy)
    {
        // First, check if we can spawn more enemies
        if (!enemyTracker.CanSpawnMore())
        {
            Debug.Log("Max enemies reached, cannot spawn more.");
            return null;
        }

        // Second, check if the spawn position is within 5 units of the player
        if (Vector3.Distance(spawnPosition, player.GetComponent<Transform>().position) < 5f)
        {
            Debug.Log("Spawn too close.");
            return null;
        }

        return Instantiate(enemy, spawnPosition, Quaternion.identity);
    }
}


