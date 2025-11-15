using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMainMenu : MonoBehaviour
{
    public static event Action OnReturnToMainMenu;
    public Roster roster;

    private bool highScoreCheckStarted = false;
    private bool highScoreCheckCompleted = false;
    private int targetSceneIndex = -1;

    public void Start()
    {
        SteamLeaderboardManager.Instance.GetAllLeaderboards();
    }

    public void invokeMenuReturn()
    {
        OnReturnToMainMenu?.Invoke();
    }

    /// <summary>
    /// Call this when the game over screen becomes active to begin checking high score
    /// and preloading the appropriate scene in the background.
    /// </summary>
    public void OnGameOverScreenShown()
    {
        SaveManager.instance.Save();

		if (!highScoreCheckStarted)
        {
            highScoreCheckStarted = true;
            Debug.Log("Game over screen shown, starting high score check and scene preload...");
            StartCoroutine(CheckAndSaveNewHighScoreCoroutine(OnHighScoreCheckComplete));
        }
    }

    private void OnHighScoreCheckComplete()
    {
        highScoreCheckCompleted = true;
        
        // Determine which scene to load based on whether there's a new high score
        if (GameManager.Instance != null && GameManager.Instance.NewHighScore == true)
        {
            targetSceneIndex = SceneManager.GetActiveScene().buildIndex + 1; // High scores scene
            Debug.Log($"New high score detected, preloading high scores scene (index {targetSceneIndex})");
        }
        else
        {
            targetSceneIndex = SceneManager.GetActiveScene().buildIndex - 1; // Main menu
            Debug.Log($"No new high score, preloading main menu (index {targetSceneIndex})");
        }

        // Start preloading the target scene
        if (ScenePreloader.Instance != null)
        {
            ScenePreloader.Instance.PreloadScene(targetSceneIndex);
        }
        else
        {
            Debug.LogWarning("ScenePreloader.Instance is null. Make sure ScenePreloader exists in the scene.");
        }
    }

    public void returnMainMenu()
    {

        // Clear all pause reasons when leaving the game scene
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearAllPauses();
        }

        Debug.Log("Returning to main menu...");
        
        // If high score check hasn't completed yet, wait for it
        if (!highScoreCheckCompleted)
        {
            Debug.Log("High score check not completed, waiting for it to finish...");
            StartCoroutine(WaitForHighScoreCheckThenActivate());
        }
        else
        {
            // High score check already done, just activate the preloaded scene
            Debug.Log("High score check already completed, activating target scene");
            ActivateTargetScene();
        }
    }

    /// <summary>
    /// Waits for high score check to complete, ensuring targetSceneIndex is set before proceeding
    /// </summary>
    private IEnumerator WaitForHighScoreCheckThenActivate()
    {
        // Make sure high score check has started
        if (!highScoreCheckStarted)
        {
            highScoreCheckStarted = true;
            Debug.Log("High score check not started, starting it now...");
            StartCoroutine(CheckAndSaveNewHighScoreCoroutine(OnHighScoreCheckComplete));
        }

        // Wait for the high score check to complete with a timeout
        float timeout = Time.realtimeSinceStartup + 5f; // 5 second timeout
        while (!highScoreCheckCompleted && Time.realtimeSinceStartup < timeout)
        {
            Debug.Log("Waiting for high score check to complete...");
            yield return new WaitForSeconds(0.1f);
        }

        if (!highScoreCheckCompleted)
        {
            Debug.LogError("High score check timed out! Using fallback main menu.");
            targetSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        }

        ActivateTargetScene();
    }

    private void ActivateTargetScene()
    {
        if (targetSceneIndex == -1)
        {
            Debug.LogError("Target scene index is -1! This should not happen. Defaulting to main menu.");
            targetSceneIndex = SceneManager.GetActiveScene().buildIndex - 1;
        }

        Debug.Log($"Activating target scene (index {targetSceneIndex})");
        OnReturnToMainMenu?.Invoke();

        // Use the preloader if available, otherwise load normally
        if (ScenePreloader.Instance != null && ScenePreloader.Instance.GetOperation(targetSceneIndex) != null)
        {
            Debug.Log("Using preloaded scene for instant transition");
            ScenePreloader.Instance.ActivatePreloaded(targetSceneIndex);
        }
        else
        {
            Debug.Log($"Scene not preloaded, loading normally (index {targetSceneIndex})");
            SceneManager.LoadScene(targetSceneIndex);
        }
    }

    private IEnumerator CheckAndSaveNewHighScoreCoroutine(Action onComplete = null)
    {
        // Get the final score from ScoreTracker
        ScoreTracker scoreTracker = FindObjectOfType<ScoreTracker>();
        if (scoreTracker == null)
        {
            Debug.LogWarning("ScoreTracker not found, cannot check for new high score");
            onComplete?.Invoke();
            yield break;
        }

        int finalScore = scoreTracker.GetScore();

        // Get the selected character from PlayerPrefs
        int selectedCharacterIndex = SaveManager.instance.GetInt("SelectedCharacter", 0);
        
        // Get the character prefName from the roster
        if (roster == null || selectedCharacterIndex >= roster.allCharacters.Count)
        {
            Debug.LogWarning("Could not find roster or character index out of range");
            onComplete?.Invoke();
            yield break;
        }

        string characterUsed = roster.allCharacters[selectedCharacterIndex].prefName;

        // Wait for leaderboard to be ready with 1-second timeout using realtimeSinceStartup (not affected by timeScale)
        float timeoutTime = Time.realtimeSinceStartup + 1f;
        while (!SteamLeaderboardManager.Instance.IsCharacterLeaderboardReady(characterUsed))
        {
            if (Time.realtimeSinceStartup > timeoutTime)
            {
                Debug.LogWarning($"Timeout waiting for leaderboard to be ready for character: {characterUsed}");
                break;
            }
            yield return null;
        }

        // Check if this is a new high score using the Steam leaderboard (only for the specific character)
        if (SteamLeaderboardManager.Instance != null && 
            SteamLeaderboardManager.Instance.IsNewHighScore(finalScore, characterUsed))
        {   
            // Save to GameManager to trigger animation in high scores scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NewHighScore = true;
                GameManager.Instance.NewHighScoreValue = finalScore;
                GameManager.Instance.NewHighScoreCharacter = characterUsed;
                Debug.Log($"New high score detected: {finalScore} with {characterUsed}");
            }
        }
        else
        {
            Debug.Log($"No new high score detected: {finalScore} with {characterUsed}");
            // Clear any previous high score state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NewHighScore = false;
            }
        }

        onComplete?.Invoke();
    }
}
