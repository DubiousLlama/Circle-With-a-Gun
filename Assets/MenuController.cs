using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    public GameObject HighScore;
    public MenuMusic menuMusic;
    public GameObject tutorialToggle;

    private void Start()
    {
        tutorialToggle.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("doTutorial", 1) == 1;
        Debug.Log(PlayerPrefs.GetInt("doTutorial"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void toggleTutorial()
    {
        int flag = (PlayerPrefs.GetInt("doTutorial") == 1) ? 0 : 1;
        PlayerPrefs.SetInt("doTutorial", flag);
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
