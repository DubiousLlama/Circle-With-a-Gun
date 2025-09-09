using UnityEngine;
using System.Collections.Generic;

class ObstacleSetup : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public RectTransform arenaBounds;

    [Header("Obstacle Settings")]
    public float jitterAmount = 0.5f;
    public float safetyMargin = 0.5f; // Distance from arena edges

    [Header("Obstacle Type Distribution")]
    [Range(0, 7)] public int numberOfSquares = 4;

    [Header("Square Obstacles")]
    public float minSquareSize = 1f;
    public float maxSquareSize = 2f;

    [Header("Rectangle Obstacles")]
    public float minRectangleShortSide = 0.8f;
    public float maxRectangleShortSide = 1.5f;
    public float minRectangleLongSide = 2.5f;
    public float maxRectangleLongSide = 4f;

    private List<Bounds> placedObstacles = new List<Bounds>();

    /*
     * This script should place six obstacles around the arena at the start of the game.
     * The obstacles should be evenly spaced around the arena: one each in the upper left, upper right, lower left, lower right, left center, and right center.
     * Some of the obstacles should be long rectangles, and others should be squares.
     * The obstacles should have random rotations and some random jitter on their placements, so that they are different each time the game is played.
     * The obstacles should not extend beyond the bounds of the arena or overlap with each other.
     * 
     * Efficiency is paramount. The script should include logging of how long it takes to run.
     */

    private void Awake()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        SetupObstacles();
        stopwatch.Stop();
        Debug.Log($"ObstacleSetup completed in {stopwatch.ElapsedMilliseconds}ms ({stopwatch.ElapsedTicks} ticks)");
    }

    private void SetupObstacles()
    {
        if (obstaclePrefab == null || arenaBounds == null)
        {
            Debug.LogError("ObstacleSetup: Missing required references (obstaclePrefab or arenaBounds)");
            return;
        }

        // Get arena bounds
        Rect arenaRect = arenaBounds.rect;
        Vector3 arenaCenter = arenaBounds.position;

        // Define the seven positions relative to arena bounds
        Vector2[] positions = new Vector2[]
        {
            new Vector2(-arenaRect.width * 0.25f, arenaRect.height * 0.25f),   // Upper left
            new Vector2(arenaRect.width * 0.25f, arenaRect.height * 0.25f),    // Upper right
            new Vector2(-arenaRect.width * 0.25f, -arenaRect.height * 0.25f),  // Lower left
            new Vector2(arenaRect.width * 0.25f, -arenaRect.height * 0.25f),   // Lower right
            new Vector2(-arenaRect.width * 0.4f, 0f),                          // Left center
            new Vector2(arenaRect.width * 0.4f, 0f),                           // Right center
            new Vector2(0f, 0f)                                                // Center
        };

        // Generate random obstacle types based on configuration
        List<bool> isRectangle = GenerateRandomObstacleTypes();

        for (int i = 0; i < positions.Length; i++)
        {
            PlaceObstacle(positions[i], arenaCenter, arenaRect, isRectangle[i]);
        }
    }

    private List<bool> GenerateRandomObstacleTypes()
    {
        List<bool> types = new List<bool>();

        // Add the specified number of squares (false)
        for (int i = 0; i < numberOfSquares; i++)
        {
            types.Add(false);
        }

        // Add the remaining rectangles (true)
        for (int i = 0; i < 7 - numberOfSquares; i++)
        {
            types.Add(true);
        }

        // Shuffle the list
        for (int i = 0; i < types.Count; i++)
        {
            bool temp = types[i];
            int randomIndex = Random.Range(i, types.Count);
            types[i] = types[randomIndex];
            types[randomIndex] = temp;
        }

        return types;
    }

    private void PlaceObstacle(Vector2 basePosition, Vector3 arenaCenter, Rect arenaRect, bool isRectangle)
    {
        const int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Apply random jitter
            Vector2 jitteredPosition = basePosition + new Vector2(
                Random.Range(-jitterAmount, jitterAmount),
                Random.Range(-jitterAmount, jitterAmount)
            );

            // Calculate world position
            Vector3 worldPosition = arenaCenter + new Vector3(jitteredPosition.x, jitteredPosition.y, 0f);

            // Generate obstacle dimensions
            Vector2 size = GenerateObstacleSize(isRectangle);
            float rotation = Random.Range(0f, 360f);

            // Check if placement is valid
            Bounds obstacleBounds = new Bounds(worldPosition, new Vector3(size.x, size.y, 1f));

            if (IsValidPlacement(obstacleBounds, arenaCenter, arenaRect))
            {
                CreateObstacle(worldPosition, size, rotation);
                placedObstacles.Add(obstacleBounds);
                return;
            }
        }

        Debug.LogWarning($"Failed to place obstacle after {maxAttempts} attempts at position {basePosition}");
    }

    private Vector2 GenerateObstacleSize(bool isRectangle)
    {
        if (isRectangle)
        {
            // Long rectangle: one dimension is significantly larger
            float shortSide = Random.Range(minRectangleShortSide, maxRectangleShortSide);
            float longSide = Random.Range(minRectangleLongSide, maxRectangleLongSide);

            // Randomly choose orientation
            return Random.value > 0.5f ?
                new Vector2(longSide, shortSide) :
                new Vector2(shortSide, longSide);
        }
        else
        {
            // Square: both dimensions similar
            float size = Random.Range(minSquareSize, maxSquareSize);
            return new Vector2(size, size);
        }
    }

    private bool IsValidPlacement(Bounds obstacleBounds, Vector3 arenaCenter, Rect arenaRect)
    {
        // Check arena bounds
        Vector3 arenaMin = arenaCenter + new Vector3(-arenaRect.width * 0.5f + safetyMargin, -arenaRect.height * 0.5f + safetyMargin, 0f);
        Vector3 arenaMax = arenaCenter + new Vector3(arenaRect.width * 0.5f - safetyMargin, arenaRect.height * 0.5f - safetyMargin, 0f);

        if (obstacleBounds.min.x < arenaMin.x || obstacleBounds.max.x > arenaMax.x ||
            obstacleBounds.min.y < arenaMin.y || obstacleBounds.max.y > arenaMax.y)
        {
            return false;
        }

        // Check overlap with existing obstacles
        foreach (var existingBounds in placedObstacles)
        {
            if (obstacleBounds.Intersects(existingBounds))
            {
                return false;
            }
        }

        return true;
    }

    private void CreateObstacle(Vector3 position, Vector2 size, float rotation)
    {
        obstaclePrefab.transform.localScale = new Vector3(size.x, size.y, 1f);
        GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.Euler(0f, 0f, rotation));

        // Set parent for organization
        obstacle.transform.SetParent(transform);
    }
}