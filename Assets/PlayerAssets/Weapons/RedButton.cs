using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedButton : Weapon
{
    private string sfx = "teleport";
    RectTransform playArea;
    PlayerHealth playerHealth;

    private Vector3 destination;

    public void Awake()
    {
        weaponType = WeaponType.Secondary;
    }

    float teleportDelay = 0.1f;

    float healAmount = 400;

    public override void Equip()
    {
        base.Equip();

        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
    }

    public override string getDisplayName()
    {
        return "Escape";
    }

    public override WeaponType getFinalType()
    {
        return WeaponType.Secondary;
    }

    public override void Fire()
    {
        int maxAttempts = 200;
        float minDistance = 25f;
        float minFoeDistance = 5f;
        
        for (int i = 0; i < maxAttempts; i++)
        {
            // Gradually relax constraints as we get more desperate
            float currentMinDistance = Mathf.Max(10f, minDistance - (i * 0.05f));
            float currentFoeDistance = Mathf.Max(2f, minFoeDistance - (i * 0.025f));

            // Generate random point in a ring around the player (more efficient than pure random)
            // This ensures we're always at least trying to get away from current position
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(currentMinDistance, Mathf.Min(30f, Mathf.Max(playArea.rect.width, playArea.rect.height) / 2f));
            
            Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0);
            Vector3 randomPoint = transform.position + offset;

            // Clamp to play area bounds
            randomPoint.x = Mathf.Clamp(randomPoint.x, playArea.rect.xMin + 1f, playArea.rect.xMax - 1f);
            randomPoint.y = Mathf.Clamp(randomPoint.y, playArea.rect.yMin + 1f, playArea.rect.yMax - 1f);

            // Check if the point collides with anything
            Collider2D hit = Physics2D.OverlapPoint(randomPoint);
            if (hit != null)
            {
                continue;
            }

            // Check if there is a foe within the safe radius
            Collider2D[] foes = Physics2D.OverlapCircleAll(randomPoint, currentFoeDistance);
            if (foes.Length > 0)
            {
                continue;
            }

            // Found a valid spot!
            AudioManager.instance.PlaySfx(sfx);
            destination = randomPoint;
            Invoke("Teleport", teleportDelay);
            return;
        }

        // If we still haven't found a spot after all attempts, just teleport to a random spot in play area
        // (emergency fallback - better than doing nothing)
        Debug.LogWarning("Using emergency teleport fallback");
        destination = new Vector3(
            Random.Range(playArea.rect.xMin + 5f, playArea.rect.xMax - 5f),
            Random.Range(playArea.rect.yMin + 5f, playArea.rect.yMax - 5f),
            0
        );
        AudioManager.instance.PlaySfx(sfx);
        Invoke("Teleport", teleportDelay);
    }

    private void Teleport()
    {
        // Teleport the player
        transform.parent.position = destination;
        GameObject teleportEffect = Instantiate(Resources.Load<GameObject>("TeleportEffect"), transform.position, Quaternion.identity);

        // Heal the player
        playerHealth.Heal(healAmount);
        audioManager.PlaySfx("heal");
    }
}
