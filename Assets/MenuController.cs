using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    public GameObject HighScore;
    public GameObject tutorialToggle;
    public GameObject menuCanvas;
    public GameObject loadingScreen;
    public GameObject characterSelectCanvas;
    public Slider loadingBar;
    public Image selectedChar;

    public List<Image> charSprites;

    int doTutorial;

    void Start()
    {
        doTutorial = PlayerPrefs.GetInt("doTutorial", 1);
        tutorialToggle.GetComponent<Toggle>().isOn = doTutorial == 1;
        Debug.Log(PlayerPrefs.GetInt("doTutorial"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        if (doTutorial == 1)
        {
            menuCanvas.SetActive(false);
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneAsync());
            return;
        }
        else
        {
            menuCanvas.SetActive(false);
            characterSelectCanvas.SetActive(true);
        }
    }

    public void CharacterSelected()
    {
        characterSelectCanvas.SetActive(false);
        loadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        
        float targetProgress = 0f;
        float currentProgress = 0f;
        
        while (!asyncLoad.isDone)
        {
            targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.5f);
            
            if (targetProgress > currentProgress)
            {
                currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, Time.deltaTime * 2f);
            } else if (currentProgress < 1f)
            {
                currentProgress += Time.deltaTime * (1 - currentProgress);
            }

            loadingBar.value = Mathf.Clamp01(currentProgress);

            yield return null;
        }
        
        while (currentProgress < 1f)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, 1f, Time.deltaTime * 2.5f);
            loadingBar.value = currentProgress;
            yield return null;
        }
        
        loadingScreen.SetActive(false);
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
    }
}
