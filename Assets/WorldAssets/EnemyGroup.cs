using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName="New Enemy Group", menuName = "ScriptableObjects/EnemyGroup")]
[System.Serializable]
public class EnemyGroup
{
    public GameObject enemy;
    public int count;
    public Vector2 spawnOffset;
    public float groupSpread = 1f;

    public EnemyGroup(GameObject enemy, int count, Vector2 spawnOffset, float groupSpread=1f)
    {
        this.enemy = enemy;
        this.count = count;
        this.spawnOffset = spawnOffset;
        this.groupSpread = groupSpread;
    }
}
