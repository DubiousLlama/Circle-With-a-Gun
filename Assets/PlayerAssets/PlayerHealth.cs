using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 0f;
    public float startingMaxHealth = 1000f;
    public float regenRate = 50f;

    public float regenDelay = 2f;
    public HealthBarController sliderController;
    public GameObject gameOverScreen;

    private float regenTimer = 0f;

    private ScoreTracker scoreTracker;
    private PlayerStats playerStats;
    private bool gameOver = false;

    private float maxHealth = 1000f;

    AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        health += startingMaxHealth;
        gameOverScreen.SetActive(false);

        scoreTracker = GameObject.Find("Score").GetComponent<ScoreTracker>();
        playerStats = GetComponent<PlayerStats>();

        audioManager = AudioManager.instance;
    }

    public void Damage(float damage)
    {
        string damageToUse = "damage" + 3.ToString();
        audioManager.PlaySfx(damageToUse);
        health -= damage;
        regenTimer = 0f;
    }

    public void Heal(float heal)
    {
        health += heal;
        regenTimer = regenDelay + 0.01f;
    }

    void Update()
    {
        maxHealth = startingMaxHealth * playerStats.MaxHealth();

        if (regenTimer < regenDelay)
        {
            regenTimer += Time.deltaTime;
        }

        if (health < maxHealth && regenTimer > regenDelay)
        {
            health += regenRate * playerStats.RegenRate() * Time.deltaTime;
        }
        if (health > maxHealth)
        {
            health = maxHealth;
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
                Invoke("EndGame", 1.4f);
                gameOverScreen.SetActive(true);
            }
        }
    }

    private void EndGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
