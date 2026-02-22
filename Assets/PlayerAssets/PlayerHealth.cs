using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PlayerHealth : MonoBehaviour
{
    public float health = 0f;
    public float startingMaxHealth = 1000f;
    public float regenRate = 50f;

    public float regenDelay = 2f;
    public HealthBarController sliderController;
    public GameObject gameOverScreen;
    public ScoreTracker scoreTracker;

    private float regenTimer = 0f;

    private bool gameOver = false;

    private float maxHealth = 1000f;

    AudioManager audioManager;
    GameMusic gameMusic;
    PostProcessVolume post;
    Vignette vignette;

    PlayerStats stats;

    // Start is called before the first frame update
    void Start()
    {
        health += startingMaxHealth;
        gameOverScreen.SetActive(false);

        audioManager = AudioManager.instance;
        gameMusic = GameMusic.instance;
        stats = PlayerStats.instance;

        post = GameObject.Find("PostEffects").GetComponent<PostProcessVolume>();
        vignette = post.profile.GetSetting<Vignette>();
    }

    public void Damage(float damage)
    {
        string damageToUse = "damage" + 3.ToString();
        audioManager.PlaySfx(damageToUse);
        health -= damage;
        regenTimer = 0f;

        if (health < maxHealth * 0.3f && !gameOver)
        {
            gameMusic.PlayEventTrack("lowHealth");
        }
    }

    public void Heal(float heal, bool triggerRegen = true)
    {
        health += heal;
        if (triggerRegen)
        {
            regenTimer = regenDelay + 0.01f;
        }
    }

    void Update()
    {
        maxHealth = startingMaxHealth * stats.GetStatMod(StatTypes.MaxHealth);

        if (regenTimer < regenDelay)
        {
            regenTimer += Time.deltaTime * stats.GetStatMod(StatTypes.HealthRegenDelay);
        }

        if (health < maxHealth && regenTimer > regenDelay)
        {
            health += regenRate * Time.deltaTime * stats.GetStatMod(StatTypes.HealthRegenRate);
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        } 
        
        
        if (health < maxHealth * 0.33f)
        {
            gameMusic.HealthLow(true);
        } else
        {
            gameMusic.HealthLow(false);
        }

            sliderController.SetHealth((int)health);
        sliderController.SetMaxHealth((int)maxHealth);

        if (health <= 0)
        {
            if (gameOver == false)
            {
                gameOver = true;
                scoreTracker.GameOver();
                if (scoreTracker.GetScore() > PlayerPrefs.GetInt("HighScore"))
                {
                    PlayerPrefs.SetInt("HighScore", scoreTracker.GetScore());
                }
                gameMusic.PlayEventTrack("death");
                GameManager.Instance.RequestPause(GameManager.PauseReason.GameOver);
                gameOverScreen.SetActive(true);
                gameOverScreen.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = "Score: " + scoreTracker.GetScore().ToString();
            }
        }

        Pulse();

    }

    private void Pulse()
    {
        float pulseSpeed = 5f;

        if (health < maxHealth * 0.35f && !gameOver)
        {
            // 
            float amplitude = (1 - (health / (maxHealth * 0.35f))) * 0.05f + 0.12f;

            // Breathing effect using sine wave
            vignette.intensity.value =  Mathf.Sin(Time.time * pulseSpeed) * amplitude * 0.3f + amplitude * 3;
        }
        else
        {
            // Smoothly fade back to 0 when not low health
            vignette.intensity.value = Mathf.MoveTowards(
                vignette.intensity.value,
                0f,
                Time.deltaTime
            );
        }
    }

    public float GetHealthPercentage()
    {
        return health / maxHealth;
    }
}