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

    // Start is called before the first frame update
    void Start()
    {
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();

        if (playArea == null)
        {
            Debug.LogError("PlayArea not found");
        }

        Debug.Log(playArea.rect.position);
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate)
        {
            spawnTimer = 0f;
            type = Random.Range(0, 5);
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

        if (type < 0 || type > 4)
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

       // Check if the spawn location is within view of the camera
       //Vector3 screenPoint = Camera.main.WorldToViewportPoint(spawnLocation);
       // if (!(screenPoint.z < 0 || screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1))
       // {
       //     Debug.Log("PowerUp is within view of the camera");
       //     SpawnPowerUp(type);
       //     return;
       // }

        // Check if the spawn loaction overlaps with another powerup
        Collider2D collider = Physics2D.OverlapCircle(spawnLocation, 1f,LayerMask.NameToLayer("PowerUp"));
        if (collider != null)
        {
            Debug.Log("PowerUp overlaps with another powerup");
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
                // Instantiate(x2Prefab, v3, Quaternion.identity);
                break;
            case 4:
                Instantiate(bombPrefab, v3, Quaternion.identity);
                break;
            default:
                Debug.LogError("Invalid PowerUp Type");
                break;

        }
    }
}
