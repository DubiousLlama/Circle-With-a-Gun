using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferLevelUp : MonoBehaviour
{
    [Header("References")]
    public Transform UIOptionsGrid;

    [Header("Rarity Rates (expressed as count per total)")]
    public int commonRate;
    public int uncommonRate;
    public int rareRate;

    [Header("Settings")]
    [Tooltip("How much more likely prerequisite upgrades are to be offered")]
    float prerequisiteWeight = 2f;

    float commonWeight;
    float uncommonWeight;
    float rareWeight;

    bool isShotgun = false;
    bool isBoomerang = false;
    List<string> noBoomerang = new List<string> { "Multishot", "Wallbounce" };
    List<string> noShotgun = new List<string> {"Wallbounce" };
    List<Upgrade> allUpgrades = null;
    Dictionary<Upgrade, float> weightedUpgrades = new Dictionary<Upgrade, float>();
    List<string> currentUpgrades = new List<string>();

    Upgrade[] offeredUpgrades = new Upgrade[3];

    public static event Action LevelUpSelected;

    // Start is called before the first frame update
    void Awake()
    {
        commonWeight = commonRate / (float)(commonRate + uncommonRate + rareRate);
        uncommonWeight = uncommonRate / (float)(commonRate + uncommonRate + rareRate);
        rareWeight = rareRate / (float)(commonRate + uncommonRate + rareRate);
    }

    private void OnEnable()
    {
        if (allUpgrades == null)
        {
            allUpgrades = new List<Upgrade>(Resources.LoadAll<Upgrade>("Upgrades"));
        }

        SetupUpgradeList();
        for (int i = 0; i < 3; i++)
        {
            offeredUpgrades[i] = GetRandomUpgrade();
        }

        // Display the offered upgrades to the player
        foreach (Transform child in UIOptionsGrid)
        {
            SetupOption(child.gameObject);
        }

        Debug.Log("Level Up Menu: Requesting pause");
        GameManager.Instance.RequestPause(GameManager.PauseReason.LevelUpMenu);

        string weaponName = GameObject.Find("PC").GetComponent<WeaponsManager>().GetEquippedWeapon(WeaponType.Primary).getDisplayName();
        isBoomerang = weaponName == "Boomerang";
        isShotgun = weaponName == "Shotgun";
    }

    private void OnDisable()
    {
        Debug.Log("Level Up Menu: Removing pause");
        GameManager.Instance.RemovePause(GameManager.PauseReason.LevelUpMenu);
    }

    public void SelectLevelUp(int index)
    {
        Upgrade selectedUpgrade = offeredUpgrades[index];

        if (selectedUpgrade != null)
        {
            currentUpgrades.Add(selectedUpgrade.name);
            GameObject upObj = Instantiate(selectedUpgrade.UpgradeObject);
            upObj.transform.SetParent(GameObject.FindGameObjectWithTag("Player").transform);
        }

        LevelUpSelected?.Invoke();
        gameObject.transform.parent.gameObject.SetActive(false);
    }

    private void SetupOption(GameObject button)
    {
        // Set up the UI to display the offered upgrades
        int index = button.transform.GetSiblingIndex();
        Upgrade up = offeredUpgrades[index];

        if (up == null)
        {
            button.SetActive(false);
            GameObject nothingLeft = transform.parent.Find("NothingLeft").gameObject;
            nothingLeft.SetActive(true);
            // The game object has a rectTransform component
            Vector2 anchor = nothingLeft.GetComponent<RectTransform>().anchoredPosition;
            anchor.y = (index < 2 || anchor.y == 133) ? 133 : 90;
            nothingLeft.GetComponent<RectTransform>().anchoredPosition = anchor;

            if (index == 0)
            {
                GameObject acceptance = transform.parent.Find("Acceptance").gameObject;
                acceptance.SetActive(true);
            }
            return;
        }

        TextMeshProUGUI title = button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI description = button.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        Image icon = button.transform.GetChild(2).GetComponent<Image>();

        title.text = up.displayName;
        description.text = up.description;
        icon.sprite = up.Icon;

        button.GetComponent<Image>().color = up.rarity switch
        {
            UpgradeRarity.Common => RarityColors.Common,
            UpgradeRarity.Uncommon => RarityColors.Uncommon,
            UpgradeRarity.Rare => RarityColors.Rare,
            _ => Color.gray
        };

    }

    private Upgrade GetRandomUpgrade()
    {
        if (weightedUpgrades.Count == 0)
        {
            Debug.LogWarning("GetRandomUpgrade: No available upgrades to select from.");
            return null;
        }
        Upgrade up = WeightedRandomUpgrade(weightedUpgrades);
        weightedUpgrades.Remove(up);

        return up;
    }

    private void SetupUpgradeList()
    {
        weightedUpgrades.Clear();

        foreach (Upgrade up in allUpgrades)
        {

            if (currentUpgrades.Contains(up.name))
            {
                continue;
            }
            if (currentUpgrades.Any(x => up.prevents.Contains(x)))
            {
                continue;
            }
            if (up.requires.Any() && !currentUpgrades.Any(x => up.requires.Contains(x)))
            {
                continue;
            }
            if (noBoomerang.Contains(up.name) && isBoomerang)
            {
                continue;
            }
            if (noShotgun.Contains(up.name) && isShotgun)
            {
                continue;
            }

            float rarityWeight = up.rarity switch
            {
                UpgradeRarity.Common => commonWeight,
                UpgradeRarity.Uncommon => uncommonWeight,
                UpgradeRarity.Rare => rareWeight,
                _ => 0f
            };
            weightedUpgrades[up] = up.requires.Any() ? up.weight * rarityWeight * prerequisiteWeight : up.weight * rarityWeight;
        }
    }

    private Upgrade WeightedRandomUpgrade(Dictionary<Upgrade, float> weightedUpgrades)
    {
        float totalWeight = weightedUpgrades.Values.Sum();
        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        foreach (var kvp in weightedUpgrades)
        {
            cumulativeWeight += kvp.Value;
            if (randomValue <= cumulativeWeight)
            {
                return kvp.Key;
            }
        }
        
        Debug.LogError("WeightedRandomUpgrade: Should never reach here if weights are positive");
        return null; // Should never reach here if weights are positive
    }
}
