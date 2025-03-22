using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    public GameObject HighScore;
    public MenuMusic menuMusic;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Awake()
    {
        if (!PlayerPrefs.HasKey("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", 0);
        }

        HighScore.GetComponent<TextMeshProUGUI>().text = "High Score: " + PlayerPrefs.GetInt("HighScore");
        menuMusic = MenuMusic.instance;
    }
}
