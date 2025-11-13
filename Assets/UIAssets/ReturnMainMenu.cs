using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMainMenu : MonoBehaviour
{
    public static event Action OnReturnToMainMenu;
    public Roster roster;

    public void Start()
    {
        SteamLeaderboardManager.Instance.GetAllLeaderboards();
    }

    public void invokeMenuReturn()
    {
        OnReturnToMainMenu?.Invoke();
    }

    public void returnMainMenu()
    {
        StartCoroutine(CheckAndSaveNewHighScoreCoroutine(() => 
        {
            OnReturnToMainMenu?.Invoke();
            if (GameManager.Instance != null && GameManager.Instance.NewHighScore == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            } else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            }

                Time.timeScale = 1f;
        }));
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
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        
        // Get the character prefName from the roster
        if (roster == null || selectedCharacterIndex >= roster.allCharacters.Count)
        {
            Debug.LogWarning("Could not find roster or character index out of range");
            onComplete?.Invoke();
            yield break;
        }

        string characterUsed = roster.allCharacters[selectedCharacterIndex].prefName;

        // Wait for leaderboard to be ready with 1-second timeout
        float timeoutTime = Time.time + 1f;
        while (!SteamLeaderboardManager.Instance.IsCharacterLeaderboardReady(characterUsed))
        {
            if (Time.time > timeoutTime)
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
