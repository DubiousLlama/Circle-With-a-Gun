using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Pathfinding;

public class ObstacleSetup : MonoBehaviour
{
    public GameObject obstaclePrefab;
    
    List<GameObject> obstacles;
    RectTransform playArea;

    void Awake()
    {
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();


        if (playArea == null)
        {
            Debug.LogError("PlayArea not found");
        }

        obstacles = GenerateLocations();
    }
    
    void Start()
    {
        AstarPath.active.Scan();
    }

    public void OnDestroy()
    {
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
    }

    // This function should return a list of RectTransforms that represent the locations of the obstacles
    // The obsacles should not overlap with each other
    // no point on any obstacle should be closer than buffer/2 units to any other obstacle
    // The obstacles should not overlap with the player
    // The obstacles should be completely within the play area
    // 3-4 obstacles should be long rectangles with length 10-20 units and width 1.25 units, at random Z rotations
    // 1-2 obstacles should be square obstacles with side length 5-10 units
    // There should be 4-6 obstacles in total

    List<GameObject> GenerateLocations()
    {
        List<GameObject> generatedObstacles = new List<GameObject>();

        int rectangleObstacles = Random.Range(3, 5);
        int squareObstacles = Random.Range(1, 3);

        int buffer = 2;

        int i = 0;

        List<Vector2> obstaclePositions = new List<Vector2> {
            new Vector2(-19.3f, -9.4f),
            new Vector2(-3.4f, 4.8f),
            new Vector2(1.2f, 18.4f),
            new Vector2(-21.6f, 18.5f),
            new Vector2(2.2f, -10.1f),
            new Vector2(-20.3f, 4f),
            new Vector2(-10f, 5f)
            };

        while (generatedObstacles.Count < rectangleObstacles)
        {
            i++;

            if (i > 300)
            {
                Debug.Log("Abort Generation");
                break;
            }

            // Get a random item from the list of obstacle positions
            Vector2 position = obstaclePositions[Random.Range(0, obstaclePositions.Count)];

            // Add a bit of randomness to the position
            position += Random.insideUnitCircle * 1.5f;

            // Get a random rotation
            float rotation = Random.Range(0, 360);

            // Get a random length
            float length = Random.Range(8, 20);

            // Check if the obstacle would overlap with any other obstacle. The obstacles, player, and walls all have colliders
            Collider2D overlap = Physics2D.OverlapBox(position, new Vector2(length + buffer, 1.25f + buffer), rotation);
            if (overlap != null)
            {
                continue;
            }

            // We're good to go. Instantiate the obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.Euler(0, 0, rotation));
            obstacle.transform.localScale = new Vector3(length, 1.25f, 1);

            // remove the position we used from the list
            obstaclePositions.Remove(position);
            
            generatedObstacles.Add(obstacle);
        }

        while (generatedObstacles.Count < rectangleObstacles + squareObstacles)
        {
            i++;

            if (i > 500)
            {
                Debug.Log("Abort generation");
                break;
            }

            // Get a random position inside the play area
            Vector2 position = obstaclePositions[Random.Range(0, obstaclePositions.Count)];

            // Get a random rotation
            float rotation = Random.Range(0f, 360f);

            // Get a random length
            float length = Random.Range(3f, 6f);

            // Check if the obstacle would overlap with any other obstacle. The obstacles, player, and walls all have colliders
            if (Physics2D.OverlapBox(position, new Vector2(length + buffer, length + buffer), rotation))
            {
                continue;
            }

            // We're good to go. Instantiate the obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.Euler(0, 0, rotation));
            obstacle.transform.localScale = new Vector2(length, length);

            generatedObstacles.Add(obstacle);
        }

        Debug.Log("Iterations required to generate arena: " + i);

        return generatedObstacles;

    }


}
