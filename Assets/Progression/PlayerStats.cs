using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public enum StatTypes
{
    AttackSpeed,
    SecondaryCooldown,
    MaxHealth,
    MoveSpeed,
    HealthRegenRate,
    HealthRegenDelay,
    LegendaryDuration,
    RevivesInt,
    InvulnerablityBool,
    SecoundaryCountInt
}

struct StatModifier
{
    public StatTypes stat;
    public float modifier;
    public bool isEternal;
    public float duration;
    public GUID guid;

    public StatModifier(StatTypes stat, float modifier, bool isEternal, float duration)
    {
        this.stat = stat;
        this.isEternal = isEternal;
        this.modifier = modifier;
        this.duration = duration;
        guid = GUID.Generate();
    }
}

public class PlayerStats : MonoBehaviour
{

    public static PlayerStats instance;

    private List<StatModifier> multStatModifiers = new List<StatModifier>();

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("Multiple PlayerStats in scene!");
        }
    }

    void Update()
    {
        for (int i = multStatModifiers.Count - 1; i >= 0; i--)
        {
            if (!multStatModifiers[i].isEternal)
            {
                StatModifier sm = multStatModifiers[i];
                sm.duration -= Time.deltaTime;
                if (sm.duration <= 0)
                {
                    multStatModifiers.RemoveAt(i);
                }
                else
                {
                    multStatModifiers[i] = sm;
                }
            }
        }
    }

    public float GetStatMod(StatTypes stat)
    {
        float totalMod = 1f;
        foreach (StatModifier sm in multStatModifiers)
        {
            if (sm.stat == stat)
            {
                totalMod += sm.modifier;
            }
        }
        return 1 + totalMod;
    }

    public GUID ModifyMultStat(StatTypes stat, float mult, bool isEternal, float duration = 0f)
    {
        multStatModifiers.Add(new StatModifier(stat, mult, isEternal, duration));
        return multStatModifiers[multStatModifiers.Count - 1].guid;
    }

    public void ResetMultStat(StatTypes stat)
    {
        multStatModifiers.RemoveAll(sm => sm.stat == stat);
    }

    public void RemoveMultStatModifier(GUID guid)
    {
        multStatModifiers.RemoveAll(sm => sm.guid == guid);
    }

}
