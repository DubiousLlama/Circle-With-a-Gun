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
        if (!PlayerPrefs.HasKey("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", 0);
        }

        HighScore.GetComponent<TextMeshProUGUI>().text = "High Score: " + PlayerPrefs.GetInt("HighScore");

        // Subscribe to Steam leaderboard updates
        SteamLeaderboardManager.OnFriendLeaderboardsDownloaded += UpdateHighScoreFromSteam;
    }

    void Start()
    {
        doTutorial = PlayerPrefs.GetInt("doTutorial", 1);
        tutorialToggle.GetComponent<Toggle>().isOn = doTutorial == 1;
        Debug.Log("Tutorial selected:" + PlayerPrefs.GetInt("doTutorial"));

        if (GameManager.Instance.ShowCharacterSelectOnMenuLoad)
        {
            GameManager.Instance.ShowCharacterSelectOnMenuLoad = false;
            menuCanvas.SetActive(false);
            characterSelectCanvas.SetActive(true);
        }
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
            
            // Defer preload to next frame to avoid frame hitch
            StartCoroutine(DeferredPreloadScene());
        }
    }

    /// <summary>
    /// Defers the scene preload to the next frame to avoid frame hitch on button press
    /// </summary>
    private IEnumerator DeferredPreloadScene()
    {
        yield return null; // Wait one frame
        
        int gunTimeSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (ScenePreloader.Instance != null)
        {
            Debug.Log($"Starting preload of GunTime scene (index {gunTimeSceneIndex}) while character is being selected");
            ScenePreloader.Instance.PreloadScene(gunTimeSceneIndex);
        }
        else
        {
            Debug.LogWarning("ScenePreloader.Instance is null. Scene will load normally after character selection.");
        }
    }

    public void viewScores()
    {
        StartCoroutine(LoadSceneAsync(2));
    }

    public void CharacterSelected()
    {
      
        int gunTimeSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // Check if scene was preloaded, if so use it
        if (ScenePreloader.Instance != null && ScenePreloader.Instance.GetOperation(gunTimeSceneIndex) != null)
        {
            Debug.Log("Using preloaded GunTime scene");
            StartCoroutine(ActivatePreloadedScene(gunTimeSceneIndex));
        }
        else
        {
            // Fall back to normal loading if preload wasn't available
            Debug.Log("Scene not preloaded, loading normally");
            loadingScreen.SetActive(true);
            StartCoroutine(LoadSceneAsync(1));
        }
    }

    /// <summary>
    /// Activates a preloaded scene with loading bar animation.
    /// Waits for both the preload to be ready (progress >= 0.9) AND completes the visual animation.
    /// </summary>
    private IEnumerator ActivatePreloadedScene(int sceneIndex)
    {
        var asyncLoad = ScenePreloader.Instance.GetOperation(sceneIndex);
        
        if (asyncLoad == null)
        {
            Debug.LogWarning("Preloaded operation was null, falling back to normal load");
            yield return StartCoroutine(LoadSceneAsync(1));
            yield break;
        }

        // Wait for the actual async operation to reach ready state (progress >= 0.9)
        Debug.Log($"Waiting for preloaded scene to reach ready state (progress >= 0.9)...");
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        Debug.Log("Activating preloaded scene");
        ScenePreloader.Instance.ActivatePreloaded(sceneIndex);


        // Wait for activation to complete
        float activationTimeout = Time.realtimeSinceStartup + 5f;
        while (!asyncLoad.isDone && Time.realtimeSinceStartup < activationTimeout)
        {
            yield return null;
        }

        if (!asyncLoad.isDone)
        {
            Debug.LogError("Scene activation timed out!");
        }

        MenuMusic.instance.LeaveLevel();
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
        int flag = (PlayerPrefs.GetInt("doTutorial") == 1) ? 0 : 1;
        PlayerPrefs.SetInt("doTutorial", flag);
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

        // Update PlayerPref with the highest score from Steam
        PlayerPrefs.SetInt("HighScore", highestScore);
        PlayerPrefs.Save();
        
        // Update the UI display with the new high score
        HighScore.GetComponent<TextMeshProUGUI>().text = "High Score: " + highestScore;
        
        Debug.Log($"Updated high score from Steam leaderboards: {highestScore}");
    }
}
