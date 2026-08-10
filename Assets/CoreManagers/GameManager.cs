using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool ShowCharacterSelectOnMenuLoad { get; set; } = false;
    public bool NewHighScore { get; set; } = false;

    public int NewHighScoreValue { get; set; } = 0;
    public string NewHighScoreCharacter { get; set; } = "";

    // Pause management
    private HashSet<PauseReason> pauseReasons = new HashSet<PauseReason>();

    protected Callback<GameOverlayActivated_t> m_gameOverlayActivated;

    public enum PauseReason
    {
        PauseMenu,
        LevelUpMenu,
        GameOver,
        SteamOverlay,
        WindowFocusLost
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
        m_gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
    }

    private void OnEnable()
    {
        Application.focusChanged += OnApplicationFocus;
    }
    private void OnDisable()
    {
        Application.focusChanged -= OnApplicationFocus;
    }

    void Update()
    {
        MenuInputMode.Tick();
        if (CrosshairController.IsReplacingCursor)
            SetCursorState(false);
        else if (MenuInputMode.MouseActiveThisFrame)
            SetCursorState(true);
        else if (MenuInputMode.GamepadActiveThisFrame)
            SetCursorState(false);
    }

    void SetCursorState(bool isVisible)
    {
        Cursor.visible = isVisible;
    }

    private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
    {
        if (pCallback.m_bActive != 0)
        {
            Debug.Log("Steam Overlay is now active.");
            RequestPause(PauseReason.SteamOverlay);
        }
        else
        {
            Debug.Log("Steam Overlay has been closed.");
            RemovePause(PauseReason.SteamOverlay);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Debug.Log("Application lost focus.");
            RequestPause(PauseReason.WindowFocusLost);
        }
        else
        {
            Debug.Log("Application gained focus.");
            RemovePause(PauseReason.WindowFocusLost);
        }
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