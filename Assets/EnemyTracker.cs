using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTracker : MonoBehaviour
{
    [Range(10, 250)]
    public int maxEnemiesDesktop = 200;

    [Range(10, 250)]
    public int maxEnemiesMobile = 150;

    private int maxEnemies = 0;


    HashSet<int> enemies = new HashSet<int>();

    public void Start()
    {
        // Set the max enemies based on if we are on a mobile platform
        maxEnemies = Application.isMobilePlatform ? maxEnemiesMobile : maxEnemiesDesktop;
    }

    public void RegisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;
        enemies.Add(enemy.GetInstanceID());
    }

    public void UnregisterEnemy(GameObject enemy)
    {
        enemies.Remove(enemy.GetInstanceID());
    }

    public int GetEnemyCount()
    {
        return enemies.Count;
    }

    public bool CanSpawnMore()
    {
        return enemies.Count < maxEnemies;
    }
}
