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
    public void Awake()
    {
        // Subscribe to Steam leaderboard updates
        SteamLeaderboardManager.OnFriendLeaderboardsDownloaded += UpdateHighScoreFromSteam;
    }

    void Start()
    {
        doTutorial = SaveManager.instance.GetInt("doTutorial", 1);
        tutorialToggle.GetComponent<Toggle>().isOn = doTutorial == 1;
        Debug.Log("Tutorial selected:" + SaveManager.instance.GetInt("doTutorial"));

        if (GameManager.Instance.ShowCharacterSelectOnMenuLoad)
        {
            GameManager.Instance.ShowCharacterSelectOnMenuLoad = false;
            menuCanvas.SetActive(false);
            characterSelectCanvas.SetActive(true);
        }

        if (!PlayerPrefs.HasKey("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", 0);
        }

        HighScore.GetComponent<TextMeshProUGUI>().text = "High Score: " + PlayerPrefs.GetInt("HighScore");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlayGame()
    {
        if (doTutorial == 1)
        {
            // Tutorial mode: set character to Kevin (index 0) and load directly
            PlayerPrefs.SetInt("SelectedCharacter", 0);
            menuCanvas.SetActive(false);
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneAsync(1));
            return;
        }
        else
        {
            menuCanvas.SetActive(false);
            characterSelectCanvas.SetActive(true);
        }
    }

    public void viewScores()
    {
        StartCoroutine(LoadSceneAsync(2));
    }

    public void CharacterSelected()
    {
        loadingScreen.SetActive(true);
        StartCoroutine(LoadSceneAsync(1));
    }

    private IEnumerator LoadSceneAsync(int indexAdd)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + indexAdd);
        
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

        MenuMusic.instance.LeaveLevel();

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
        int flag = (SaveManager.instance.GetInt("doTutorial") == 1) ? 0 : 1;
        SaveManager.instance.SetInt("doTutorial", flag);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        SteamLeaderboardManager.OnFriendLeaderboardsDownloaded -= UpdateHighScoreFromSteam;
    }

    /// <summary>
    /// Updates the high score from Steam leaderboards when they are downloaded.
    /// This serves as the single source of truth for the player's best score.
    /// </summary>
    private void UpdateHighScoreFromSteam()
    {
        if (SteamLeaderboardManager.Instance == null)
        {
            Debug.LogWarning("SteamLeaderboardManager instance not found");
            return;
        }

        // Get personal scores from the leaderboards (already sorted descending by score)
        List<ScoreData> personalScores = SteamLeaderboardManager.Instance.GetLeaderboard(ScoreLists.Personal);
        
        if (personalScores == null || personalScores.Count == 0)
        {
            Debug.Log("No personal scores found in Steam leaderboards");
            return;
        }

        // The list is already sorted descending, so the first entry is the highest score
        int highestScore = personalScores[0].score;
        if (highestScore <= 0)
        {
            Debug.Log("No valid high score found in Steam leaderboards");
            return;
        }

        // Update PlayerPref with the highest score from Steam
        PlayerPrefs.SetInt("HighScore", highestScore);
        PlayerPrefs.Save();
        
        // Update the UI display with the new high score
        HighScore.GetComponent<TextMeshProUGUI>().text = "High Score: " + highestScore;
        
        Debug.Log($"Updated high score from Steam leaderboards: {highestScore}");
    }
}
