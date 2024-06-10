using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
    // PowerUp Indicies
    // 0: Regen
    // 1: Speed
    // 2: Laser
    // 3: x2
    // 4: Bomb

    public float[] powerUpDurations = new float[4];
    public bool[] powerUpActive = new bool[4];

    private GameObject player;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private Shooting playerShooting;
    private BeamAttack playerNoAimAttacks;
    private GameObject healthBar;
    private IndicatorController indController;
    private ScoreTracker scoreTracker;

    private Color powerUpGreen = new Color(0.054901960784313725f, 0.7686274509803922f, 0);

    private int x2Multiplier = 1;

    private static float baseRegenRate;
    private static float baseRegenDelay;
    private static float baseSpeed;
    private static int baseShootingDamage;
    private static float baseShootingSpeed;
    public static Color baseBeamColor;

    [Range(1, 20)]
    public float regenDuration = 10f;
    [Range(1, 20)]
    public float speedDuration = 10f;
    [Range(1, 20)]
    public float laserDuration = 10f;
    [Range(1, 20)]
    public float x2Duration = 10f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PC");
        playerHealth = player.GetComponent<PlayerHealth>();
        playerMovement = player.GetComponent<PlayerMovement>();
        playerShooting = player.GetComponent<Shooting>();

        healthBar = GameObject.Find("HealthBar").transform.GetChild(0).gameObject;
        baseBeamColor = new Color(0.561111f, 0, 1f, 1f);
        indController = GameObject.Find("PowerUpIndicator").GetComponent<IndicatorController>();
        scoreTracker = GameObject.Find("Score").GetComponent<ScoreTracker>();


        for (int i = 0; i < powerUpActive.Length; i++)
        {
            powerUpActive[i] = false;
        }

        for (int i = 0; i < powerUpDurations.Length; i++)
        {
            powerUpDurations[i] = 0;
        }

        baseRegenRate = playerHealth.regenRate;
        baseRegenDelay = playerHealth.regenDelay;
        baseSpeed = playerMovement.moveSpeed;
       
        baseShootingSpeed = playerShooting.fireRate;
        baseShootingDamage = playerShooting.damage;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < powerUpDurations.Length; i++)
        {
            if (powerUpDurations[i] > 0)
            {
                powerUpDurations[i] -= Time.deltaTime;
            } else
            {
                powerUpActive[i] = false;
            }
            
        }

        ModifyPlayerState();
    }

    private void ModifyPlayerState()
    {
        // Regen
        if (powerUpActive[0])
        {
            // Get the Fill component of the health bar
            healthBar.GetComponent<Image>().color = powerUpGreen;
            
            playerHealth.regenRate = baseRegenRate * 5;
            playerHealth.regenDelay = baseRegenDelay / 2f;
            indController.SetDuration(0, powerUpDurations[0]);


        }
        else
        {
            // Regen Inactive
            healthBar.GetComponent<Image>().color = Color.red;

            playerHealth.regenRate = baseRegenRate;
            playerHealth.regenDelay = baseRegenDelay;
            indController.SetDuration(0, 0);
        }

        // Speed
        if (powerUpActive[1])
        {
            playerMovement.moveSpeed = baseSpeed * 1.66f;
            indController.SetDuration(1, powerUpDurations[1]);
        }
        else
        {
            playerMovement.moveSpeed = baseSpeed;
            indController.SetDuration(1, 0);
        }

        // Laser
        if (powerUpActive[2])
        {
           playerShooting.bulletColor = powerUpGreen;
           playerShooting.damage = 60;
           playerShooting.fireRate = 0.08f;
           indController.SetDuration(2, powerUpDurations[2]);
        }
        else
        {
            // Laser Inactive
            playerShooting.damage = baseShootingDamage;
            playerShooting.bulletColor = baseBeamColor;
            playerShooting.fireRate = baseShootingSpeed;
            indController.SetDuration(2, 0);

        }

        // x2
        if (powerUpActive[3])
        {
            //playerShooting.bulletColor = powerUpGreen;
            //playerShooting.damage = (int)(baseShootingDamage * x2Multiplier);
            //scoreTracker.multiplier = x2Multiplier;
            //indController.SetMultiplier(x2Multiplier);
            //indController.SetDuration(3, powerUpDurations[3]);
            
        }
        else
        {
            // x2 Inactive
            //playerShooting.bulletColor = baseBulletColor;
            //indController.SetDuration(3, 0);
            //indController.SetMultiplier(2);
            //x2Multiplier = 1;
            //scoreTracker.multiplier = x2Multiplier;
        }
    }

    public void ActivatePowerUp(string powerUp)
    {
        switch (powerUp)
        {
            case "Regen":
                ActivatePowerUp(0, regenDuration);
                break;
            case "Speed":
                ActivatePowerUp(1, speedDuration);
                break;
            case "Laser":
                ActivatePowerUp(2, laserDuration);
                break;
            case "x2":
                ActivatePowerUp(3, x2Duration + powerUpDurations[3]);
                break;
            default:
                break;
        }
    }
       
    private void ActivatePowerUp(int powerUpIndex, float duration)
    {
        powerUpActive[powerUpIndex] = true;
        powerUpDurations[powerUpIndex] = duration;

        if (powerUpIndex == 3)
        {
            x2Multiplier = x2Multiplier * 2;
        }
    }
}
