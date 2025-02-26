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
        while (i < 100) {
            i++;

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
