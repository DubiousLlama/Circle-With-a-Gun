using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    private RectTransform playArea;

    public GameObject weaponItem;
    public GameObject[] items;

    [Range(0.01f, 20)]
    public float spawnRate = 10f;

    private float spawnTimer = 0f;
    private Vector2 spawnLocation;

    private float[] itemRarityWeights;

    public float powerUpWeight;
    public float weaponWeight;
    public float legendaryWeight;

    public float commonOccurance;
    public float uncommonOccurance;
    public float rareOccurance;
// Note

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
        player = GameObject.Find("PC");

        if (playArea == null)
        {
            Debug.LogError("PlayArea not found");
        }
        if (player == null)
        {
            Debug.LogError("Player not found");
        }

        itemRarityWeights = new float[items.Length];
        for (int i = 0; i < items.Length; i++) {
            Weapon weapon = items[i].GetComponent<Weapon>();
            if (weapon != null) {
                if (weapon.weaponType == WeaponType.Legendary)
                {
                    itemRarityWeights[i] = legendaryWeight;
                } else
                {
                    itemRarityWeights[i] = weaponWeight;
                }
            } else {
                itemRarityWeights[i] = powerUpWeight;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnRate)
        {
            spawnTimer = 0f;
            int index = WeightedRandom(itemRarityWeights);
            GameObject itemPrefab = items[index];
            SpawnItemAtRandomLocation(itemPrefab);
        }
    }

    static public int WeightedRandom(float[] weights)
    {
        float weightSum = 0f;
        foreach (float weight in weights)
        {
            weightSum += weight;
        }
        int index = 0;
        int lastIndex = weights.Length - 1;
        while (index < lastIndex)
        {
            if (Random.Range(0, weightSum) < weights[index])
            {
                return index;
            }
            weightSum -= weights[index++];
        }
        return index;
    }

    private void SpawnItemAtRandomLocation(GameObject itemPrefab, int j = 0)
    {
        if (j > 10)
        {
            Debug.Log("Too many attempts to spawn item");
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

        Collider2D collider = Physics2D.OverlapCircle(spawnLocation, 1f,LayerMask.NameToLayer("Item"));
        
        // Check if the spawn loaction overlaps with another item
        collider = Physics2D.OverlapCircle(spawnLocation, 1f,LayerMask.NameToLayer("Item"));
        if (collider != null)
        {
            Debug.Log("Item overlaps with another item");
            SpawnItemAtRandomLocation(itemPrefab, j+=1);
            return;
        }

        // check if the spawn location overlaps with an Obstacle
        collider = Physics2D.OverlapCircle(spawnLocation, 1f, LayerMask.NameToLayer("Obstacle"));
        if (collider != null)
        {
            Debug.Log("Item overlaps with an obstacle");
            SpawnItemAtRandomLocation(itemPrefab, j+=1);
            return;
        }

        // Check if the location is within 4 units of the player
        if (Vector2.Distance(spawnLocation, player.transform.position) < 4)
        {
            Debug.Log("Item is too close to the player");
            SpawnItemAtRandomLocation(itemPrefab, j+=1);
            return;
        }

        Vector3 v3 = new Vector3(spawnLocation.x, spawnLocation.y, 5);

        if (itemPrefab.GetComponent<Weapon>() == null)
        {
            SpawnItem(itemPrefab, v3, WeaponRarity.Common);
            return;
        }

        WeaponRarity rarity;
        if (itemPrefab.GetComponent<Weapon>().getFinalType() == WeaponType.Legendary)
        {
            rarity = WeaponRarity.Legendary;
        } else
        {
            rarity = (WeaponRarity)WeightedRandom(new float[] { commonOccurance, uncommonOccurance, rareOccurance });
        }

        // Check if the player has an item of the same type or better equipped
        string weaponKind = itemPrefab.GetComponent<Weapon>().getDisplayName();
        Weapon playerWeapon = player.GetComponent<WeaponsManager>().GetEquippedWeapon(itemPrefab.GetComponent<Weapon>().weaponType);

        if (playerWeapon.getDisplayName() == weaponKind && playerWeapon.rarity >= rarity)
        {
            Debug.Log("Player already has a better or equal weapon equipped");
            return;
        }

        SpawnItem(itemPrefab, v3, rarity);
    }

    public void SpawnItem(GameObject itemPrefab, Vector3 v3, WeaponRarity rarity)
    {
        bool isWeapon = itemPrefab.GetComponent<Weapon>() != null;
        if (isWeapon) {
            GameObject weaponItemInstance = Instantiate(weaponItem, v3, Quaternion.identity);
            WeaponItem weaponItemComponent = weaponItemInstance.GetComponent<WeaponItem>();
            GameObject weaponInstance = Instantiate(itemPrefab, weaponItemInstance.transform);
            Weapon weaponComponent = weaponInstance.GetComponent<Weapon>();
            weaponComponent.SetRarity(rarity);
            weaponItemComponent.weapon = weaponInstance;
        } else {
            Instantiate(itemPrefab, v3, Quaternion.identity);
        }
    }

}
