using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Text.RegularExpressions;

public class WaveSpawner : MonoBehaviour
{
    [Header("Settings")]
    public float difficulty = 1f;

    [Range(0f, 0.1f)]
    public float difficultyIncrease = 0.008f;

    [Range(5f, 20f)]
    public float timeBetweenWaves = 10f;

    [Range(0f, 5f)]
    public float timeVariance = 2f;

    [Range(0f, 5f)]
    public float distanceVariance = 1f;
    

    [Header("Wave Pools")]
    public List<Wave> pool1 = new List<Wave>();
    public float pool1time;

    public List<Wave> pool2 = new List<Wave>();
    public float pool2time;

    public List<Wave> pool3 = new List<Wave>();
    public float pool3time;

    public List<Wave> pool4 = new List<Wave>();
    public float pool4time;


    private Dictionary<float, List<Wave>> pools;
    private float time;
    private float timeSinceLastWave;

    GameObject player;
    RectTransform playArea;

    // Start is called before the first frame update
    void Start()
    {
        time = 0;
        timeSinceLastWave = 0;

        pools = new Dictionary<float, List<Wave>>
        {
            { pool1time, pool1 },
            { pool2time, pool2 },
            { pool3time, pool3 },
            { pool4time, pool4 }
        };

        player = GameObject.Find("PC");
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        difficulty += Time.deltaTime * difficultyIncrease;
        time += Time.deltaTime;
        timeSinceLastWave += Time.deltaTime * difficulty;

        List<Wave> curpool = pools[pool1time];

        foreach (float pooltime in pools.Keys)
        {
            if (time > pooltime)
            {
                // Most of the time, get the most recent pool, but sometimes get the previous pool. Should have 1/4^n chance of getting the pool n previous.
                if (Random.Range(0f, 1f) > 0.75f) {
                    curpool = pools[pooltime];
                }
            }
        }

        

        if (curpool.Count == 0)
        {
            return;
        }

        if (timeSinceLastWave > timeBetweenWaves)
        {
            timeSinceLastWave = 0 + Random.Range(-timeVariance, timeVariance);
            Wave wave = curpool[Random.Range(0, curpool.Count)];
            Debug.Log("Spawning wave: " + wave.name);

            if (wave.ongoing)
            {
                StartCoroutine(SpawnWaveOngoing(wave));
            }
            else
            {
                SpawnWave(wave);
            }
        }
    }

    private IEnumerator SpawnWaveOngoing(Wave wave) {

        for (int i = 0; i < wave.groups.Count; i += wave.groupsAtATime)
        {
            for (int j = 0; j < wave.groupsAtATime; j++)
            {
                EnemyGroup group = wave.groups[i + j];
                Vector3 spawnPosition = new Vector3(
                                               player.transform.position.x + group.spawnOffset.x + Random.Range(-distanceVariance, distanceVariance),
                                               player.transform.position.y + group.spawnOffset.y + Random.Range(-distanceVariance, distanceVariance),
                                               0);

                for (int k = 0; k < group.count; k++)
                {
                    for (int tries = 0; tries < 10; tries++)
                    {
                        spawnPosition = new Vector3(
                                               spawnPosition.x + Random.Range(-group.groupSpread + tries, group.groupSpread + tries),
                                               spawnPosition.y + Random.Range(-group.groupSpread + tries, group.groupSpread + tries),
                                               0);

                        if (Spawn(group.enemy, spawnPosition))
                        {
                            break;
                        }
                    }
                }
            }
            Debug.Log("Waiting for " + wave.rate + " seconds before spawning next group.");
            yield return new WaitForSeconds(wave.rate);
        }
    }

    private void SpawnWave(Wave wave)
    {
        for (int i = 0; i < wave.groups.Count; i++)
        {
            EnemyGroup group = wave.groups[i];

            if (group.spawnOffset.x == 0 && group.spawnOffset.y == 0)
            {
                group.spawnOffset = Random.insideUnitCircle * (16f + Random.Range(-distanceVariance, distanceVariance));
            }

            Vector3 spawnPosition = new Vector3(
                                            player.transform.position.x + (group.spawnOffset.x + Random.Range(-distanceVariance, distanceVariance)),
                                            player.transform.position.y + group.spawnOffset.y + Random.Range(-distanceVariance, distanceVariance),
                                            0);

            for (int k = 0; k < group.count; k++)
            {
                for (int tries = 0; tries < 4; tries++)
                {
                    Vector3 offset = Random.insideUnitCircle*(group.groupSpread+tries);
                    spawnPosition += offset;

                    if (Spawn(group.enemy, spawnPosition))
                    {
                        break;
                    }
                }

            }
        }
    }

    bool Spawn(GameObject enemy, Vector3 spawnPosition)
    {
        // Check if the spawn position intersects with any other colliders
        Collider2D hitCollider = Physics2D.OverlapCircle(spawnPosition, 0.3f);
        if (hitCollider != null && hitCollider.gameObject.tag != "PowerUp")
        {
            return false;
        }

        // Check if the enemy is within the play area
        if (!playArea.rect.Contains(new Vector2(spawnPosition.x, spawnPosition.y) - (Vector2)playArea.position))
        {
            return false;
        }

        // Check if the enemy is within 4 units of the player
        if (Vector2.Distance(spawnPosition, player.transform.position) < 4)
        {
            return false;
        }

        // Instantiate the enemy prefab at the spawn position
        Instantiate(enemy, spawnPosition, Quaternion.identity);
        return true;
    }
}
