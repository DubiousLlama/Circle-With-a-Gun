using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class LeaderboardView : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingMessage;
    public GameObject content;
    public Roster roster;
    public GameObject scoreDisplayPrefab;
    public ScrollRect scrollRect;

    [Header("Navigation Buttons")]
    public Button personalScoreButton;
    public Button friendsScoreButton;
    public Button globalScoreButton;

    [Header("Status UI")]
    public GameObject refreshButton;
    public GameObject noNewData;

    [Header("Gamepad Scrolling")]
    [Tooltip("Speed of scrolling when using gamepad (pixels per second)")]
    public float scrollSpeed = 300f;
    [Tooltip("Threshold for analog stick input (0-1)")]
    public float stickDeadzone = 0.3f;
    [Tooltip("Exponent for scroll speed curve (higher = faster falloff at low inputs, more responsive at full stick)")]
    public float scrollCurveExponent = 2f;

    private SteamLeaderboardManager leaderboardManager;
    private ScoreLists currentScoreList = ScoreLists.Friends;
    private bool isScrolling = false;
    private const float ScrollThreshold = 0.1f;
    private bool previousFrameHadInput = false;
    private RectTransform scrollRectTransform;

    #region Unity Lifecycle

    private void Start()
    {
        leaderboardManager = SteamLeaderboardManager.Instance;
        
        if (leaderboardManager == null)
        {
            Debug.LogError("SteamLeaderboardManager instance not found!");
            return;
        }

        if (scrollRect != null)
        {
            scrollRectTransform = scrollRect.GetComponent<RectTransform>();
        }

        InitializeUI();
        
        // Subscribe to manager events
        SteamLeaderboardManager.OnFriendLeaderboardsDownloaded += OnLeaderboardsReady;
        SteamLeaderboardManager.OnGlobalLeaderboardsDownloaded += OnLeaderboardsReady;
        
        // Request leaderboard data
        leaderboardManager.GetAllLeaderboards();
    }

    private void OnDestroy()
    {
        if (leaderboardManager != null)
        {
            SteamLeaderboardManager.OnFriendLeaderboardsDownloaded -= OnLeaderboardsReady;
            SteamLeaderboardManager.OnGlobalLeaderboardsDownloaded -= OnLeaderboardsReady;
        }
    }

    private void Update()
    {
        HandleGamepadNavigation();
    }

    #endregion

    #region Gamepad Navigation

    private void HandleGamepadNavigation()
    {
        if (scrollRect == null) return;

        var gamepadInput = Gamepad.current;
        if (gamepadInput == null) return;

        // Get D-pad input
        var dpadValue = gamepadInput.dpad.ReadValue();
        bool dpadDownPressed = dpadValue.y < -ScrollThreshold;
        bool dpadUpPressed = dpadValue.y > ScrollThreshold;

        // Get left stick input
        var leftStickValue = gamepadInput.leftStick.ReadValue();
        bool stickDownPressed = leftStickValue.y < -stickDeadzone;
        bool stickUpPressed = leftStickValue.y > stickDeadzone;

        // Combine D-pad and stick input for detection
        bool downPressed = dpadDownPressed || stickDownPressed;
        bool upPressed = dpadUpPressed || stickUpPressed;
        bool currentFrameHasInput = downPressed || upPressed;

        // Calculate scroll intensity (0 to 1)
        float scrollIntensity = 0f;
        if (downPressed)
        {
            if (dpadDownPressed)
            {
                scrollIntensity = 0.6f; // D-pad equivalent to 0.6 stick input
            }
            else if (stickDownPressed)
            {
                scrollIntensity = Mathf.Abs(leftStickValue.y); // Use actual stick magnitude
            }
        }
        else if (upPressed)
        {
            if (dpadUpPressed)
            {
                scrollIntensity = 0.6f; // D-pad equivalent to 0.6 stick input
            }
            else if (stickUpPressed)
            {
                scrollIntensity = Mathf.Abs(leftStickValue.y); // Use actual stick magnitude
            }
        }

        // Check if a nav button is currently selected
        Button selectedButton = EventSystem.current.currentSelectedGameObject?.GetComponent<Button>();
        bool isNavButtonSelected = selectedButton != null && 
            (selectedButton == personalScoreButton || selectedButton == friendsScoreButton || selectedButton == globalScoreButton);

        bool stickLeftRightPressed = Mathf.Abs(leftStickValue.x) > stickDeadzone * 1.8f;

        // Enter scroll mode only on transition: no input last frame, but input this frame, and nav button selected
        if (!previousFrameHadInput && downPressed && isNavButtonSelected && !isScrolling && !stickLeftRightPressed)
        {
            // Check if the scroll rect even has enough content to scroll
            if (HasScrollableContent())
            {
                // Start scrolling when input is detected on a nav button
                isScrolling = true;
                DisableUINavigation();
            }
        }

        if (isScrolling)
        {
            // Handle scrolling
            if (downPressed)
            {
                ScrollDown(scrollIntensity);
            }
            else if (upPressed)
            {
                // Continue scrolling up
                ScrollUp(scrollIntensity);
            }
            else
            {
                // No input this frame - check if we should exit scroll mode
                // Only exit if we're at the top, otherwise stay in scroll mode
                if (IsScrollRectAtTop())
                {
                    isScrolling = false;
                    EnableUINavigation();
                }
            }
        }

        // Store current input state for next frame
        previousFrameHadInput = currentFrameHasInput;
    }

    private void ScrollDown(float intensity)
    {
        if (scrollRect == null) return;
        
        float curvedIntensity = ApplyScrollCurve(intensity);
        float scrollDelta = scrollSpeed * curvedIntensity * Time.deltaTime;
        
        // Use ScrollRect's normalized vertical position (0 = top, 1 = bottom)
        scrollRect.verticalNormalizedPosition -= scrollDelta / (scrollRect.content.rect.height - scrollRectTransform.rect.height);
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }

    private void ScrollUp(float intensity)
    {
        if (scrollRect == null) return;
        
        float curvedIntensity = ApplyScrollCurve(intensity);
        float scrollDelta = scrollSpeed * curvedIntensity * Time.deltaTime;
        
        // Use ScrollRect's normalized vertical position (0 = top, 1 = bottom)
        scrollRect.verticalNormalizedPosition += scrollDelta / (scrollRect.content.rect.height - scrollRectTransform.rect.height);
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
    }

    private bool HasScrollableContent()
    {
        if (scrollRect == null) return false;
        
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRectTransform.rect.height;
        
        return contentHeight > viewportHeight;
    }

    private bool IsScrollRectAtTop()
    {
        if (scrollRect == null) return true;
        
        return scrollRect.verticalNormalizedPosition >= 0.99f;
    }

    private float ApplyScrollCurve(float intensity)
    {
        // Apply exponential curve to intensity
        // At intensity 0: result is 0
        // At intensity 0.5: result is 0.5^exponent (e.g., 0.25 with exponent 2)
        // At intensity 1.0: result is 1.0
        // Higher exponent = faster falloff at low values, more dramatic acceleration
        return Mathf.Pow(intensity, scrollCurveExponent);
    }

    private void DisableUINavigation()
    {
        // Disable the EventSystem's ability to send navigation events to buttons
        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = false;
        }
    }

    private void EnableUINavigation()
    {
        // Re-enable the EventSystem's navigation events
        if (EventSystem.current != null)
        {
            EventSystem.current.sendNavigationEvents = true;
        }
    }

    #endregion

    #region Initialization

    private void InitializeUI()
    {
        if (content != null && loadingMessage != null)
        {
            bool isShowingCachedData = !leaderboardManager.IsFriendsDataReady();
            content.SetActive(true);
            loadingMessage.SetActive(!isShowingCachedData);
        }
        
        if (friendsScoreButton != null) 
        {
            friendsScoreButton.Select();
            friendsScoreButton.onClick.AddListener(() => SwitchScoreList(ScoreLists.Friends));
        }
        
        if (personalScoreButton != null) 
            personalScoreButton.onClick.AddListener(() => SwitchScoreList(ScoreLists.Personal));
            
        if (globalScoreButton != null) 
            globalScoreButton.onClick.AddListener(() => SwitchScoreList(ScoreLists.Global));
            
        if (refreshButton != null) 
            refreshButton.GetComponent<Button>().onClick.AddListener(OnRefreshButtonPressed);

        UpdateRefreshButtonState();
        SwitchScoreList(currentScoreList);
    }

    #endregion

    #region Event Callbacks

    private void OnLeaderboardsReady()
    {
        if (NewHighScoreAnimation.SuppressLeaderboardRefresh)
            return;
        CheckAndDisplayScores();
    }

    #endregion

    #region UI Management

    public void SwitchScoreList(ScoreLists scoreList)
    {
        currentScoreList = scoreList;
        List<ScoreData> scores = leaderboardManager.GetLeaderboard(scoreList);
        
        if (scores != null)
        {
            CreateScoreUI(scores);
        }
        else
        {
            Debug.LogWarning($"No scores available for {scoreList}");
        }
    }

    /// <summary>Returns a list with at most one entry per (playerName, characterUsed), keeping the highest score. Prevents the previous high from reappearing after the new-score animation.</summary>
    private static List<ScoreData> DeduplicateToBestPerPlayerCharacter(List<ScoreData> highScores)
    {
        if (highScores == null || highScores.Count == 0) return highScores;
        Dictionary<string, ScoreData> best = new Dictionary<string, ScoreData>();
        foreach (ScoreData s in highScores)
        {
            string key = s.playerName + "\n" + s.characterUsed;
            if (!best.TryGetValue(key, out ScoreData existing) || s.score > existing.score)
                best[key] = s;
        }
        List<ScoreData> list = new List<ScoreData>(best.Values);
        list.Sort((a, b) => b.score.CompareTo(a.score));
        return list;
    }

    private void CreateScoreUI(List<ScoreData> highScores)
    {
        // One row per (player, character): keep only their best score so we never show a previous high after the new-score animation.
        highScores = DeduplicateToBestPerPlayerCharacter(highScores);

        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject scoreObj = Instantiate(scoreDisplayPrefab, content.transform);
            UpdateScoreDisplay(scoreObj, highScores[i]);
            UpdateRank(scoreObj, i + 1);
        }

        Canvas.ForceUpdateCanvases();
    }

    public void UpdateScoreDisplay(GameObject scoreObject, ScoreData score)
    {
        Transform hL = scoreObject.transform.Find("HorizLayout");
        hL.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        scoreObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.score.ToString("N0");
        hL.GetChild(2).GetComponent<TextMeshProUGUI>().text = score.playerName;

        Character ch = roster.allCharacters.Find(c => c.prefName == score.characterUsed);
        if (ch != null) 
            hL.GetChild(1).GetComponent<Image>().sprite = ch.sprite;
    }

    public void UpdateRank(GameObject scoreObject, int rank)
    {
        scoreObject.transform.Find("HorizLayout").GetChild(0).GetComponent<TextMeshProUGUI>().text = $"#{rank}";
    }

    private void UpdateRefreshButtonState()
    {
        // This would require exposing more state from the manager
        // For now, we'll keep the button visible
        if (refreshButton != null) refreshButton.SetActive(true);
    }

    private void ReselectCurrentButton()
    {
        switch (currentScoreList)
        {
            case ScoreLists.Personal: 
                if (personalScoreButton != null) personalScoreButton.Select(); 
                break;
            case ScoreLists.Friends: 
                if (friendsScoreButton != null) friendsScoreButton.Select(); 
                break;
            case ScoreLists.Global: 
                if (globalScoreButton != null) globalScoreButton.Select(); 
                break;
        }
    }

    private void CheckAndDisplayScores()
    {
        if (leaderboardManager.IsFriendsDataReady())
        {
            if (content != null) content.SetActive(true);
            if (loadingMessage != null) loadingMessage.SetActive(false);
            
            UpdateRefreshButtonState();
            SwitchScoreList(currentScoreList);
        }
    }

    #endregion

    #region Button Callbacks

    public void OnRefreshButtonPressed()
    {
        Debug.Log("Manual refresh requested");
        
        if (content != null) content.SetActive(false);
        if (loadingMessage != null) loadingMessage.SetActive(true);

        ReselectCurrentButton();
        UpdateRefreshButtonState();
        leaderboardManager.ManualRefresh();
    }

    #endregion
}
