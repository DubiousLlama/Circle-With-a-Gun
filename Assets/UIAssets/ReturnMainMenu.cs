using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnMainMenu : MonoBehaviour
{
    public static event Action OnReturnToMainMenu;
    public Roster roster;

    public void invokeMenuReturn()
    {
        CheckAndSaveNewHighScore();
        OnReturnToMainMenu?.Invoke();
    }

    public void returnMainMenu()
    {
        CheckAndSaveNewHighScore();
        OnReturnToMainMenu?.Invoke();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        Time.timeScale = 1f;
    }

    private void CheckAndSaveNewHighScore()
    {
        // Get the final score from ScoreTracker
        ScoreTracker scoreTracker = FindObjectOfType<ScoreTracker>();
        if (scoreTracker == null)
        {
            Debug.LogWarning("ScoreTracker not found, cannot check for new high score");
            return;
        }

        int finalScore = scoreTracker.GetScore();

        // Get the selected character from PlayerPrefs
        int selectedCharacterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        
        // Get the character prefName from the roster
        if (roster == null || selectedCharacterIndex >= roster.allCharacters.Count)
        {
            Debug.LogWarning("Could not find roster or character index out of range");
            return;
        }

        string characterUsed = roster.allCharacters[selectedCharacterIndex].prefName;

        // Check if this is a new high score using the Steam leaderboard (only for the specific character)
        if (DownloadSteamLeaderBoard.Instance != null && 
            DownloadSteamLeaderBoard.Instance.IsCharacterLeaderboardReady(characterUsed) &&
            DownloadSteamLeaderBoard.Instance.IsNewHighScore(finalScore, characterUsed))
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
            // Clear any previous high score state
            if (GameManager.Instance != null)
            {
                GameManager.Instance.NewHighScore = false;
            }
        }
    }
}
