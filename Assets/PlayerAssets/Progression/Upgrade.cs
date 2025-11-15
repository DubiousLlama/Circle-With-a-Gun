using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UpgradeRarity
{
    Common,
    Uncommon,
    Rare,
    Glitch
}

[CreateAssetMenu(fileName = "New Upgrade", menuName = "ScriptableObjects/Upgrade")]
public class Upgrade : ScriptableObject
{
    [Header("Upgrade Info")]
    public string upgradeName;
    public string displayName;
    [TextArea]
    public string description;
    public UpgradeRarity rarity;

    [Header("Occurence Effects")]
    public string[] requires;
    public string[] prevents;
    public float weight = 1f;

    public Sprite Icon => Resources.Load<Sprite>($"UpgradeIcons/{name}");
    public GameObject UpgradeObject => Resources.Load<GameObject>($"UpgradePrefabs/{name}");
}
