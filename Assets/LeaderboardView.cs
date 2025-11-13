using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingMessage;
    public GameObject content;
    public Roster roster;
    public GameObject scoreDisplayPrefab;

    [Header("Navigation Buttons")]
    public Button personalScoreButton;
    public Button friendsScoreButton;
    public Button globalScoreButton;

    [Header("Status UI")]
    public GameObject refreshButton;
    public GameObject noNewData;

    private SteamLeaderboardManager leaderboardManager;
    private ScoreLists currentScoreList = ScoreLists.Friends;

    #region Unity Lifecycle

    private void Start()
    {
        leaderboardManager = SteamLeaderboardManager.Instance;
        
        if (leaderboardManager == null)
        {
            Debug.LogError("SteamLeaderboardManager instance not found!");
            return;
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

    private void CreateScoreUI(List<ScoreData> highScores)
    {
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
