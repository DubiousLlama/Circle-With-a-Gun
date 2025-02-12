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
    // 3: Bomb

    public float[] powerUpDurations = new float[4];
    public bool[] powerUpActive = new bool[4];

    private GameObject player;
    private PlayerHealth playerHealth;
    private Shooting playerShooting;
    private BeamAttack playerNoAimAttacks;
    private GameObject healthBar;

    private Color powerUpGreen = new Color(0.054901960784313725f, 0.7686274509803922f, 0);

    private static float baseRegenDelay;
    private static float baseShootingSpeed;
    public static Color baseBeamColor;

    PlayerStats playerStats;

    [Range(1, 20)]
    public float regenDuration = 10f;
    [Range(1, 20)]
    public float speedDuration = 10f;
    [Range(1, 20)]
    public float laserDuration = 10f;
    [Range(1, 20)]
    public float bombDuration = 10f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PC");
        playerHealth = player.GetComponent<PlayerHealth>();
        playerShooting = player.GetComponent<Shooting>();

        healthBar = GameObject.Find("HealthBar").transform.GetChild(0).gameObject;
        baseBeamColor = new Color(0.561111f, 0, 1f, 1f);
        playerNoAimAttacks = player.GetComponent<BeamAttack>();

        playerStats = player.GetComponent<PlayerStats>();


        for (int i = 0; i < powerUpActive.Length; i++)
        {
            powerUpActive[i] = false;
        }

        for (int i = 0; i < powerUpDurations.Length; i++)
        {
            powerUpDurations[i] = 0;
        }

        baseRegenDelay = playerHealth.regenDelay;

        baseShootingSpeed = playerShooting.fireRate;
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

            playerStats.regenMod = 5f;
            playerHealth.regenDelay = baseRegenDelay / 2f;


        }
        else
        {
            // Regen Inactive
            healthBar.GetComponent<Image>().color = Color.red;

            playerStats.regenMod = 1f;
            playerHealth.regenDelay = baseRegenDelay;
        }

        // Speed
        if (powerUpActive[1])
        {
            playerStats.speedMod = 1.66f;
        }
        else
        {
            playerStats.speedMod = 1f;
        }

        // Laser
        if (powerUpActive[2])
        {
           playerShooting.bulletColor = powerUpGreen;
           playerStats.damageMod = 2f;
           playerShooting.fireRate = 0.08f;
        }
        else
        {
            // Laser Inactive
            playerStats.damageMod = 1f;
            playerShooting.bulletColor = baseBeamColor;
            playerShooting.fireRate = baseShootingSpeed;
        }

        // Explosions
        if (powerUpActive[3])
        {

            playerNoAimAttacks.rechargeBar.transform.GetChild(0).GetComponent<Image>().color = powerUpGreen;
            playerNoAimAttacks.beamColor = powerUpGreen;
            playerStats.explosionDamageMod = 2f;
            playerStats.explosionRadiusMod = 1.25f;

        }
        else
        {
            playerNoAimAttacks.beamColor = baseBeamColor;
            playerNoAimAttacks.rechargeBar.transform.GetChild(0).GetComponent<Image>().color = baseBeamColor;
            playerStats.explosionDamageMod = 1f;
            playerStats.explosionRadiusMod = 1f;
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
            case "Bomb":
                ActivatePowerUp(3, bombDuration);
                break;
            default:
                break;
        }
    }
       
    private void ActivatePowerUp(int powerUpIndex, float duration)
    {
        powerUpActive[powerUpIndex] = true;
        powerUpDurations[powerUpIndex] = duration;
    }
}
