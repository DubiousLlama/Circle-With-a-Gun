using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 0f;
    public float maxHealth = 1000f;
    public float regenRate = 50f;

    public float regenDelay = 2f;
    public HealthBarController sliderController;
    public GameObject gameOverScreen;

    private float regenTimer = 0f;

    private ScoreTracker scoreTracker;

    private bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {
        health += maxHealth;
        gameOverScreen.SetActive(false);

        scoreTracker = GameObject.Find("Score").GetComponent<ScoreTracker>();
    }

    
    public void Damage(float damage)
    {
        health -= damage;
        regenTimer = 0f;
    }

    void Update()
    {
        if (regenTimer < regenDelay)
        {
            regenTimer += Time.deltaTime;
        }

        if (health < maxHealth && regenTimer > regenDelay)
        {
            health += regenRate * Time.deltaTime;
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        sliderController.SetHealth((int)health);

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
