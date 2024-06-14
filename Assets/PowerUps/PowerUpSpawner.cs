using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    private RectTransform playArea;
    

    public GameObject regenPrefab;
    public GameObject speedPrefab;
    public GameObject laserPrefab;
    public GameObject bombPrefab;

    [Range(0.01f, 20)]
    public float spawnRate = 10f;

    private float spawnTimer = 0f;
    private int type;
    private Vector2 spawnLocation;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
        player = GameObject.Find("PC");

        if (playArea == null)
        {
            Debug.LogError("PlayArea not found");
        }
        if (player == null)
        {
            Debug.LogError("Player not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate)
        {
            spawnTimer = 0f;
            type = Random.Range(0, 4);
            SpawnPowerUp(type);
        }
    }

    public void SpawnPowerUp(int type, int j = 0)
    {
        if (j > 10)
        {
            Debug.Log("Too many attempts to spawn powerup");
            return;
        }

        if (type < 0 || type > 3)
        {
            Debug.LogError("Invalid PowerUp Type");
            return;
        }

        // Select a location within the play area
        spawnLocation = new Vector3(Random.Range(playArea.rect.xMin + 1f, playArea.rect.xMax - 1f), Random.Range(playArea.rect.yMin + 1f, playArea.rect.yMax  - 1f), 5) + playArea.position;

        //only run this code when in the editor
        #if UNITY_EDITOR
        if (!playArea.rect.Contains(spawnLocation - (Vector2)playArea.position))
        {
            Debug.Log(spawnLocation);
            Debug.Log("spawn location is not within the play area");
            return;
        }
        #endif

        // Check if the spawn loaction overlaps with another powerup
        Collider2D collider = Physics2D.OverlapCircle(spawnLocation, 1f,LayerMask.NameToLayer("PowerUp"));
        if (collider != null)
        {
            Debug.Log("PowerUp overlaps with another powerup");
            SpawnPowerUp(type, j+=1);
            return;
        }

        // check if the spawn location overlaps with an Obstacle
        collider = Physics2D.OverlapCircle(spawnLocation, 1f, LayerMask.NameToLayer("Obstacle"));
        if (collider != null)
        {
            Debug.Log("PowerUp overlaps with an obstacle");
            SpawnPowerUp(type, j+=1);
            return;
        }

        // Check if the location is within 4 units of the player
        if (Vector2.Distance(spawnLocation, player.transform.position) < 4)
        {
            Debug.Log("PowerUp is too close to the player");
            SpawnPowerUp(type, j+=1);
            return;
        }

        Vector3 v3 = new Vector3(spawnLocation.x, spawnLocation.y, 5);

        switch (type)
        {
            case 0:
                Instantiate(regenPrefab, v3, Quaternion.identity);
                break;
            case 1:
                Instantiate(speedPrefab, v3, Quaternion.identity);
                break;
            case 2:
                Instantiate(laserPrefab, v3, Quaternion.identity);
                break;
            case 3:
                Instantiate(bombPrefab, v3, Quaternion.identity);
                break;
            default:
                Debug.LogError("Invalid PowerUp Type");
                break;

        }
    }
}
