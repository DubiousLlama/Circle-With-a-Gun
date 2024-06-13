using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    public List<EnemyGroup> groups;

    // If not ongoing, the wave will all spawn at once.
    // If ongoing, the wave will groupsAtATime groups at a time, with rate seconds between each group.
    public bool ongoing = false;
    public int groupsAtATime = 1;
    public float rate = 5f;

    public Wave(string name, List<EnemyGroup> groups, bool ongoing, int groupsAtATime, float rate)
    {
        this.name = name;
        this.groups = groups;
        this.ongoing = ongoing;
        this.groupsAtATime = groupsAtATime;
        this.rate = rate;
    }
}
