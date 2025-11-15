using System.Collections;
using System;
using System.Collections.Generic;
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
    public Guid guid;
    public string tag;
    public float decayRate;
    public float ogModifer;

    public StatModifier(StatTypes stat, float modifier, bool isEternal, float duration = 0f, string tag = "", float decayRate = 0f)
    {
        this.stat = stat;
        this.isEternal = isEternal;
        this.modifier = modifier;
        this.duration = duration;
        this.tag = tag;
        this.decayRate = decayRate;
        ogModifer = modifier;
        guid = Guid.NewGuid();
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
            StatModifier sm = multStatModifiers[i];
            if (!multStatModifiers[i].isEternal)
            {
                sm.duration -= Time.deltaTime;
                if (sm.duration <= 0)
                {
                    multStatModifiers.RemoveAt(i);
                    continue;
                }
                else
                {
                    multStatModifiers[i] = sm;
                }
            }

            if (multStatModifiers[i].decayRate > 0)
            {
                sm.modifier = (1 - Mathf.Pow(sm.decayRate, -1*sm.duration))*sm.ogModifer;
                if (sm.modifier <= 0)
                {
                    multStatModifiers.RemoveAt(i);
                    continue;
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
        float totalMod = 0f;
        foreach (StatModifier sm in multStatModifiers)
        {
            if (sm.stat == stat)
            {
                totalMod += sm.modifier;
            }
        }
        totalMod = Mathf.Clamp(totalMod, -0.9f, 12f);
        return 1 + totalMod;
    }

    public Guid ModifyMultStat(StatTypes stat, float mult, bool isEternal, float duration = 0f, string tag = "", float decayRate = 0f)
    {
        multStatModifiers.Add(new StatModifier(stat, mult, isEternal, duration, tag, decayRate));
        return multStatModifiers[multStatModifiers.Count - 1].guid;
    }

    public void ResetMultStat(StatTypes stat)
    {
        multStatModifiers.RemoveAll(sm => sm.stat == stat);
    }

    public void RemoveMultStatModifier(Guid guid)
    {
        multStatModifiers.RemoveAll(sm => sm.guid == guid);
    }

    public void RemoveAllModifiersWithTag(string tag)
    {
        multStatModifiers.RemoveAll(sm => sm.tag == tag);
    }

    public bool DoesGuidExist(Guid guid)
    {
        if (guid == Guid.Empty) return false;
        for (int i = 0; i < multStatModifiers.Count; i++)
        {
            if (multStatModifiers[i].guid == guid)
            {
                return true;
            }
        }
        return false;
    }

    public void SetModifierDuration(Guid guid, float newDuration)
    {
        for (int i = 0; i < multStatModifiers.Count; i++)
        {
            if (multStatModifiers[i].guid == guid)
            {
                StatModifier sm = multStatModifiers[i];
                sm.duration = newDuration;
                multStatModifiers[i] = sm;
                return;
            }
        }
    }

    public bool DoesTagExist(string tag)
    {
        foreach (StatModifier sm in multStatModifiers)
        {
            if (sm.tag == tag)
            {
                return true;
            }
        }

        return false;
    }

}
