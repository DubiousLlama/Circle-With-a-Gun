using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedButton : Weapon
{
    private string sfx = "teleport";
    RectTransform playArea;
    PlayerHealth playerHealth;
    private float teleportDelay = 0.3f;
    private float healAmount = 300;

    private Vector3 destination;

    public void Awake()
    {
        displayName = "Escape";
        cooldown = 15f;
        weaponType = WeaponType.Secondary;
    }

    public override void Equip()
    {
        base.Equip();

        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
    }

    protected override void Fire()
    {
        int i = 0;
        while (i < 200) {
            i++;

            if (i == 100)
            {
                Debug.LogWarning("Failed to find a valid teleport location");
                return;
            }

            // Get a random point inside the play area
            Vector3 randomPoint = new Vector3(Random.Range(playArea.rect.xMin, playArea.rect.xMax), Random.Range(playArea.rect.yMin, playArea.rect.yMax), 0);

            // Check if the point is too close to the player (decreasing as we get desperate)
            float distanceAway = 25f - (i * 0.05f);
            if (Vector3.Distance(randomPoint, transform.position) < distanceAway)
            {
                continue;
            }

            // Check if the point collides with anything
            Collider2D hit = Physics2D.OverlapPoint(randomPoint);
            if (hit != null)
            {
                continue;
            }

            // Check if there is a foe within 5 (decreasing as we get deseperate) units of the point
            float foeRadius = 5f - (i * 0.025f);
            Collider2D[] foes = Physics2D.OverlapCircleAll(randomPoint, foeRadius);
            if (foes.Length > 0)
            {
                continue;
            }

            // Play the teleport sound
            AudioManager.instance.PlaySfx(sfx);
            destination = randomPoint;
            Invoke("Teleport", teleportDelay);

            break;
        }
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
