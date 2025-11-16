using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightningStorm : Weapon
{

    private string strike = "Lightning";
    private string explosion = "Explosion";
    public int damage = 125;
    public int aoeDamage = 75;

    List<Collider2D> foesToHit;
    GameObject strikePrefab;

    LayerMask enemyLayer;

    public void Awake()
    {
        weaponType = getFinalType();
        SetRarity(WeaponRarity.Rare);
    }

    public void Start()
    {
        string strikeToUse = "LightningStrikeEffect";
        strikePrefab = Resources.Load<GameObject>(strikeToUse);
    }

    public override void Equip()
    {
        base.Equip();
        PlayerStats.instance.secondaryWeaponType = SecondaryWeaponType.LightningStrike;
        enemyLayer = LayerMask.GetMask("Foes");
    }

    public override string getDisplayName()
    {
        return "Lightning Strike";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Secondary;
    }

    public override void Fire()
    {
        Collider2D[] foes = Physics2D.OverlapCircleAll(transform.position, 12f, enemyLayer);
        if (foes.Length == 0)
        {
            Debug.Log("No foes in range");
            return;
        }

        int numFoes = Random.Range(Mathf.Min(foes.Length, 3), Mathf.Min(foes.Length, 6));
        int numExtraStrikes = Random.Range(2, 4);

        // Get numFoes that are spread far apart from each other
        foesToHit = SelectSpacedOutFoes(foes, numFoes);

        for (int i = 0; i < foesToHit.Count; i++)
        {
            float delay = Random.Range(0.02f, 0.15f) * i + 0.2f;
            StartCoroutine(StrikeFoe(foesToHit[i].transform.position, delay));
        }

        for (int i = 0; i < numExtraStrikes; i++)
        {
            // Get a random point on the unit circle around the player
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector3 strikePoint = transform.position + (Vector3)(randomDirection * Random.Range(6f, 12f));
            float delay = Random.Range(0f, 0.2f);
            StartCoroutine(StrikeFoe(strikePoint, delay));
        }
    }

    /// <summary>
    /// Greedily selects N foes that are spread far apart from each other.
    /// Uses a greedy algorithm: start with the foe farthest from the player,
    /// then iteratively select the foe that is farthest from all selected foes.
    /// Time complexity: O(n * m) where n is foes.Length and m is numFoes.
    /// </summary>
    private List<Collider2D> SelectSpacedOutFoes(Collider2D[] foes, int numFoes)
    {
        List<Collider2D> selected = new List<Collider2D>(numFoes);
        HashSet<int> usedIndices = new HashSet<int>();

        // Step 1: Select the foe farthest from the player
        int farthestFromPlayer = 0;
        float maxDist = 0f;
        for (int i = 0; i < foes.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, foes[i].transform.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                farthestFromPlayer = i;
            }
        }

        selected.Add(foes[farthestFromPlayer]);
        usedIndices.Add(farthestFromPlayer);

        // Step 2: Greedily select remaining foes
        // Each new foe should be as far as possible from all previously selected foes
        while (selected.Count < numFoes && selected.Count < foes.Length)
        {
            int bestCandidate = -1;
            float bestMinDistance = -1f;

            // For each unused foe, find its minimum distance to any selected foe
            for (int i = 0; i < foes.Length; i++)
            {
                if (usedIndices.Contains(i))
                    continue;

                float minDistToSelected = float.MaxValue;
                for (int j = 0; j < selected.Count; j++)
                {
                    float dist = Vector3.Distance(foes[i].transform.position, selected[j].transform.position);
                    minDistToSelected = Mathf.Min(minDistToSelected, dist);
                }

                // Pick the foe with the largest minimum distance
                if (minDistToSelected > bestMinDistance)
                {
                    bestMinDistance = minDistToSelected;
                    bestCandidate = i;
                }
            }

            if (bestCandidate != -1)
            {
                selected.Add(foes[bestCandidate]);
                usedIndices.Add(bestCandidate);
            }
            else
            {
                break; // No more valid candidates
            }
        }

        return selected;
    }

    private IEnumerator StrikeFoe(Vector3 destination, float delay)
    {
        yield return new WaitForSeconds(delay);

        // The lighting strikeSFX object has a child called 'StrikePoint' which is the point where the lightning strikes
        // Adjust the position of the lightning strikeSFX so that the StrikePoint is at the destination
        Vector3 strikeOffset = new Vector3(0.27f * 0.4f, 10.8f * 0.4f, 0);

        // Create the lightning strikeSFX 
        GameObject lightningStrike = Instantiate(strikePrefab, destination + strikeOffset, Quaternion.identity);
        Destroy(lightningStrike, 2f);
        AudioManager.instance.PlaySfx(strike, Random.Range(0.3f, 0.6f));
        yield return new WaitForSeconds(0.5f);
        LightningExplosion(destination);
    }

    private void LightningExplosion(Vector3 destination)
    {
        AudioManager.instance.PlaySfx(explosion, Random.Range(0.2f, 0.4f));


        // Get all foes directly contacted
        Collider2D[] foes = Physics2D.OverlapCircleAll(destination, 0.5f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(damage);
            }
        }

        StartCoroutine(LightningAoE(destination));
    }

    private IEnumerator LightningAoE(Vector3 destination)
    {
        yield return new WaitForSeconds(0.1f);

        // Get all foes in the AoE
        Collider2D[] foes = Physics2D.OverlapCircleAll(destination, 1.5f, enemyLayer);
        foreach (Collider2D foe in foes)
        {
            if (foe.tag == "Foe")
            {
                foe.GetComponentInParent<EnemyHealth>().TakeDamage(aoeDamage);
            }
        }
    }
}
