using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool ShowCharacterSelectOnMenuLoad { get; set; } = false;
    public bool NewHighScore { get; set; } = false;

    public int NewHighScoreValue { get; set; } = 0;
    public string NewHighScoreCharacter { get; set; } = "";

    // Pause management
    private HashSet<PauseReason> pauseReasons = new HashSet<PauseReason>();

    public enum PauseReason
    {
        PauseMenu,
        LevelUpMenu,
        GameOver
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ClearAllPauses();
    }

    /// <summary>
    /// Request the game to be paused for a specific reason
    /// </summary>
    public void RequestPause(PauseReason reason)
    {
        if (pauseReasons.Add(reason))
        {
            Debug.Log($"Pause requested: {reason}. Active pause reasons: {pauseReasons.Count}");
            UpdatePauseState();
        }
    }

    /// <summary>
    /// Remove a pause reason and unpause if no other reasons remain
    /// </summary>
    public void RemovePause(PauseReason reason)
    {
        if (pauseReasons.Remove(reason))
        {
            Debug.Log($"Pause removed: {reason}. Remaining pause reasons: {pauseReasons.Count}");
            UpdatePauseState();
        }
    }

    /// <summary>
    /// Check if the game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return pauseReasons.Count > 0;
    }

    /// <summary>
    /// Check if a specific reason is currently causing a pause
    /// </summary>
    public bool IsPausedFor(PauseReason reason)
    {
        return pauseReasons.Contains(reason);
    }

    /// <summary>
    /// Clear all pause reasons and force unpause (use with caution)
    /// </summary>
    public void ClearAllPauses()
    {
        pauseReasons.Clear();
        UpdatePauseState();
        Debug.Log("All pause reasons cleared");
    }

    private void UpdatePauseState()
    {
        if (pauseReasons.Count > 0)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}