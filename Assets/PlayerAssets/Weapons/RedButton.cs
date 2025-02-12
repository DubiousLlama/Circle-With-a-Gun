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

    public new void Awake()
    {
        base.Awake();

        // Modify base class variables as needed
        cooldown = 15f;
        isAutomatic = false;
        isTemporary = false;
        weaponType = WeaponType.Secondary;

        playArea = GameObject.Find("PlayArea").GetComponent<RectTransform>();
        playerHealth = transform.parent.GetComponent<PlayerHealth>();
    }

    protected override void Fire()
    {
        int i = 0;
        while (playArea != null) {
            // Get a random point inside the play area
            Vector3 randomPoint = new Vector3(Random.Range(playArea.rect.xMin, playArea.rect.xMax), Random.Range(playArea.rect.yMin, playArea.rect.yMax), 0);

            // Check if the point is too close to the player
            if (Vector3.Distance(randomPoint, transform.position) < 20f)
            {
                continue;
            }

            // Check if the point collides with anything
            Collider2D hit = Physics2D.OverlapPoint(randomPoint);
            if (hit != null)
            {
                continue;
            }

            // Check if there is a foe within 5 units of the point
            Collider2D[] foes = Physics2D.OverlapCircleAll(randomPoint, 5f);
            if (foes.Length > 0)
            {
                continue;
            }

            // Debug only code
            #if UNITY_EDITOR
                i++;
                if (i > 1000)
                {
                    Debug.LogError("Red Button failed to find a valid point after 1000 attempts");
                    break;
                }
            #endif


            // If all checks pass, queue the telport (it should happen after 0.1 seconds)

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
