using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DownloadSteamLeaderBoard : MonoBehaviour
{
    public static DownloadSteamLeaderBoard Instance { get; private set; }

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

    private readonly Dictionary<string, string> LeaderboardNames = new Dictionary<string, string>()
    {
        { "High Score: Kevin", "Kevin" },
        { "High Score: Bombs McGee", "BombsMcGee" },
        { "High Score: Commando", "Commando" },
        { "High Score: Electric Jeff", "ElectricJeff" },
        { "High Score: Demo Man", "DemoMan" },
        { "High Score: Specialist", "Specialist" },
        { "High Score: Shock Trooper", "ShockTrooper" },
        { "High Score: Wacky Steve", "WackySteve" },
        { "High Score: Miss Microtransaction", "MissMicrotransaction" },
    };

    private Dictionary<string, SteamLeaderboard_t> leaderboardHandles = new Dictionary<string, SteamLeaderboard_t>();
    private Dictionary<string, CallResult<LeaderboardFindResult_t>> findLeaderboardCallResults = new Dictionary<string, CallResult<LeaderboardFindResult_t>>();
    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> downloadFriendsCallResults = new Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>>();
    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> downloadGlobalCallResults = new Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>>();
    private CallResult<LeaderboardScoreUploaded_t> uploadLeaderboardCallResult;

    private Dictionary<string, List<LeaderboardEntry_t>> friendleaderboardEntries = new Dictionary<string, List<LeaderboardEntry_t>>();
    private Dictionary<string, List<LeaderboardEntry_t>> globalleaderboardEntries = new Dictionary<string, List<LeaderboardEntry_t>>();
    
    public static event Action OnFriendLeaderboardsDownloaded;
    public static event Action OnGlobalLeaderboardsDownloaded;

    private HashSet<string> friendsLeaderboardsCompleted = new HashSet<string>();
    private HashSet<string> globalLeaderboardsCompleted = new HashSet<string>();

    private List<ScoreData> friendsScoreList = new List<ScoreData>();
    private List<ScoreData> globalScoreList = new List<ScoreData>();
    
    private bool friendsDataReady = false;
    private bool globalDataReady = false;
    private ScoreLists currentScoreList = ScoreLists.Friends;

    private string cacheFilePath;
    private const float CACHE_EXPIRY_HOURS = 3f;

    private bool isShowingCachedData = false;
    private bool steamFetchFailed = false;
    private bool isFetchingFromSteam = false;
    private DateTime cacheTimestamp;
    private int failedLeaderboardCount = 0;
    private int totalLeaderboardsToFetch = 0;

    private Action onScoreUploadedCallback = null;

    #region Serializable Classes

    [System.Serializable]
    private class LeaderboardCache
    {
        public List<ScoreDataSerializable> friendsScores = new List<ScoreDataSerializable>();
        public List<ScoreDataSerializable> globalScores = new List<ScoreDataSerializable>();
        public string timestamp;
    }

    [System.Serializable]
    private class ScoreDataSerializable
    {
        public int score;
        public string playerName;
        public string characterUsed;
        
        public ScoreDataSerializable(ScoreData data)
        {
            score = data.score;
            playerName = data.playerName;
            characterUsed = data.characterUsed;
        }
        
        public ScoreData ToScoreData() => new ScoreData(score, playerName, characterUsed);
    }

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!SteamManager.Initialized) return;
        
        SteamAPI.Init();
        
        InitializeCacheFilePath();
        LoadCachedData();
        InitializeSteamCallbacks();

        OnFriendLeaderboardsDownloaded += HandleLeaderboardsReady;
        OnGlobalLeaderboardsDownloaded += HandleLeaderboardsReady;
    }

    private void Start()
    {
        InitializeUI();
        GetAllLeaderboards();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            OnFriendLeaderboardsDownloaded -= HandleLeaderboardsReady;
            OnGlobalLeaderboardsDownloaded -= HandleLeaderboardsReady;
        }
    }

    #endregion

    #region Initialization

    private void InitializeCacheFilePath()
    {
        if (SteamManager.Initialized)
        {
            string userId = SteamUser.GetSteamID().ToString();
            cacheFilePath = Path.Combine(Application.persistentDataPath, $"leaderboard_cache_{userId}.json");
        }
        else
        {
            cacheFilePath = Path.Combine(Application.persistentDataPath, "leaderboard_cache.json");
        }
    }

    private void InitializeSteamCallbacks()
    {
        uploadLeaderboardCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);

        foreach (var leaderboardName in LeaderboardNames.Keys)
        {
            findLeaderboardCallResults[leaderboardName] = CallResult<LeaderboardFindResult_t>.Create(
                (result, bIOFailure) => OnLeaderboardFound(result, bIOFailure, leaderboardName));
            
            downloadFriendsCallResults[leaderboardName] = CallResult<LeaderboardScoresDownloaded_t>.Create(
                (result, bIOFailure) => OnLeaderboardScoresDownloaded(result, bIOFailure, leaderboardName, ScoreLists.Friends));

            downloadGlobalCallResults[leaderboardName] = CallResult<LeaderboardScoresDownloaded_t>.Create(
                (result, bIOFailure) => OnLeaderboardScoresDownloaded(result, bIOFailure, leaderboardName, ScoreLists.Global));

            friendleaderboardEntries[leaderboardName] = new List<LeaderboardEntry_t>();
            globalleaderboardEntries[leaderboardName] = new List<LeaderboardEntry_t>();
        }
    }

    private void InitializeUI()
    {
        if (content != null && loadingMessage != null)
        {
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
    }

    #endregion

    #region Public API

    /// <summary>
    /// Checks if a score is a new high score for the player+character combination using fresh Steam data (never cache).
    /// </summary>
    public bool IsNewHighScore(int score, string characterUsed)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Cannot check for new high score: Steam not initialized");
            return false;
        }

        string leaderboardName = GetLeaderboardNameForCharacter(characterUsed);
        if (leaderboardName == null)
        {
            Debug.LogWarning($"No leaderboard found for character: {characterUsed}");
            return false;
        }

        if (!friendleaderboardEntries.ContainsKey(leaderboardName))
        {
            Debug.LogWarning($"No leaderboard entries found for: {leaderboardName}");
            return false;
        }

        CSteamID userId = SteamUser.GetSteamID();
        var userEntry = friendleaderboardEntries[leaderboardName].Find(e => e.m_steamIDUser == userId);
        
        if (userEntry.m_steamIDUser != userId)
        {
            return true;
        }
        
        return score > userEntry.m_nScore;
    }

    /// <summary>
    /// Uploads a score to the Steam leaderboard for the given character.
    /// </summary>
    public void UploadScore(int score, string characterUsed, Action onCompleted = null)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Cannot upload score: Steam is not initialized!");
            return;
        }

        string leaderboardName = GetLeaderboardNameForCharacter(characterUsed);
        if (leaderboardName == null || !leaderboardHandles.ContainsKey(leaderboardName))
        {
            Debug.LogError($"Leaderboard handle not found for character '{characterUsed}'. Make sure leaderboards have been fetched first.");
            return;
        }

        onScoreUploadedCallback = onCompleted;
        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(
            leaderboardHandles[leaderboardName], 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, 
            score, 
            null, 
            0
        );
        uploadLeaderboardCallResult.Set(handle);
        Debug.Log($"Uploading score {score} to {characterUsed} leaderboard");
    }

    public bool IsFriendsDataReady() => friendsDataReady && !isShowingCachedData;

    public bool IsCharacterLeaderboardReady(string characterUsed)
    {
        if (!friendsDataReady || isShowingCachedData)
            return false;

        string leaderboardName = GetLeaderboardNameForCharacter(characterUsed);
        return leaderboardName != null && friendsLeaderboardsCompleted.Contains(leaderboardName);
    }

    public List<ScoreData> GetLeaderboard(ScoreLists sl)
    {
        return sl switch
        {
            ScoreLists.Friends => friendsScoreList,
            ScoreLists.Global => globalScoreList,
            ScoreLists.Personal => GetPersonalScores(),
            _ => null,
        };
    }

    public void UploadTestScore(string characterName = "Kevin", int testScore = 678910)
    {
        UploadScore(testScore, characterName);
    }

    public void OnRefreshButtonPressed()
    {
        Debug.Log("Manual refresh requested");
        steamFetchFailed = false;
        isShowingCachedData = false;
        
        if (content != null) content.SetActive(false);
        if (loadingMessage != null) loadingMessage.SetActive(true);

        ReselectCurrentButton();
        UpdateRefreshButtonState();
        GetAllLeaderboards();
    }

    #endregion

    #region Steam Leaderboard Fetching

    public void GetAllLeaderboards()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            HandleSteamFetchFailure();
            return;
        }

        if (!isShowingCachedData)
        {
            friendsDataReady = false;
            globalDataReady = false;
        }
        
        friendsLeaderboardsCompleted.Clear();
        globalLeaderboardsCompleted.Clear();
        
        steamFetchFailed = false;
        isFetchingFromSteam = true;
        failedLeaderboardCount = 0;
        totalLeaderboardsToFetch = LeaderboardNames.Count * 2;
        
        UpdateRefreshButtonState();

        foreach (var leaderboardName in LeaderboardNames.Keys)
        {
            SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardName);
            findLeaderboardCallResults[leaderboardName].Set(handle);
        }
    }

    private void OnLeaderboardFound(LeaderboardFindResult_t result, bool bIOFailure, string leaderboardName)
    {
        if (bIOFailure || result.m_bLeaderboardFound == 0)
        {
            Debug.LogError($"Failed to find leaderboard '{leaderboardName}'");
            failedLeaderboardCount += 2;
            CheckForCompleteFetchFailure();
            return;
        }

        leaderboardHandles[leaderboardName] = result.m_hSteamLeaderboard;

        SteamAPICall_t friendsHandle = SteamUserStats.DownloadLeaderboardEntries(
            result.m_hSteamLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends,
            0, 0
        );
        downloadFriendsCallResults[leaderboardName].Set(friendsHandle);

        SteamAPICall_t globalHandle = SteamUserStats.DownloadLeaderboardEntries(
            result.m_hSteamLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser,
            -25, 25
        );
        downloadGlobalCallResults[leaderboardName].Set(globalHandle);
    }

    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t result, bool bIOFailure, string leaderboardName, ScoreLists scoreType)
    {
        if (bIOFailure)
        {
            Debug.LogError($"Failed to download {scoreType} leaderboard scores for '{leaderboardName}'");
            failedLeaderboardCount++;
            CheckForCompleteFetchFailure();
            return;
        }

        Dictionary<string, List<LeaderboardEntry_t>> entriesDict = GetEntriesDictionary(scoreType);
        HashSet<string> completedSet = scoreType == ScoreLists.Friends ? friendsLeaderboardsCompleted : globalLeaderboardsCompleted;
        
        entriesDict[leaderboardName].Clear();

        for (int i = 0; i < result.m_cEntryCount; i++)
        {
            if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out LeaderboardEntry_t entry, new int[0], 0))
            {
                entriesDict[leaderboardName].Add(entry);
            }
        }

        completedSet.Add(leaderboardName);
        
        if (completedSet.Count >= LeaderboardNames.Count)
        {
            RaiseLeaderboardDownloadedEvent(scoreType);
        }
    }

    private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to upload leaderboard score");
            onScoreUploadedCallback = null;
            return;
        }

        if (result.m_bSuccess == 0)
        {
            Debug.LogError("Leaderboard score upload was rejected");
            onScoreUploadedCallback = null;
            return;
        }

        Debug.Log("Successfully uploaded score!");
        isShowingCachedData = false;

        onScoreUploadedCallback?.Invoke();
        onScoreUploadedCallback = null;
    }

    #endregion

    #region Leaderboard Data Processing

    private void HandleLeaderboardsReady()
    {
        bool friendsFreshDataAvailable = AllLeaderboardsDownloaded(friendleaderboardEntries, downloadFriendsCallResults);
        bool globalFreshDataAvailable = AllLeaderboardsDownloaded(globalleaderboardEntries, downloadGlobalCallResults);

        bool friendsNeedsUpdate = friendsFreshDataAvailable && (!friendsDataReady || isShowingCachedData);
        bool globalNeedsUpdate = globalFreshDataAvailable && (!globalDataReady || isShowingCachedData);

        if (friendsNeedsUpdate)
        {
            friendsScoreList = SynthesizeScoreData(friendleaderboardEntries, false);
            friendsDataReady = true;
        }

        if (globalNeedsUpdate)
        {
            globalScoreList = SynthesizeScoreData(globalleaderboardEntries, true);
            globalDataReady = true;
        }

        if (friendsNeedsUpdate || globalNeedsUpdate)
        {
            isShowingCachedData = false;
            steamFetchFailed = false;
            isFetchingFromSteam = false;
            
            CheckAndDisplayScores();
        }
    }

    private List<ScoreData> SynthesizeScoreData(Dictionary<string, List<LeaderboardEntry_t>> leaderboardEntries, bool requestUserInfo)
    {
        List<ScoreData> scoreList = new List<ScoreData>();

        foreach (var kvp in leaderboardEntries)
        {
            string leaderboardName = kvp.Key;
            string characterName = LeaderboardNames[leaderboardName];
            
            foreach (var entry in kvp.Value)
            {
                string playerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                
                if (requestUserInfo && (string.IsNullOrEmpty(playerName) || playerName == entry.m_steamIDUser.ToString()))
                {
                    SteamFriends.RequestUserInformation(entry.m_steamIDUser, false);
                    playerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                    
                    if (string.IsNullOrEmpty(playerName))
                    {
                        string steamIdStr = entry.m_steamIDUser.ToString();
                        playerName = "Player " + steamIdStr.Substring(steamIdStr.Length - 4);
                    }
                }
                
                scoreList.Add(new ScoreData(entry.m_nScore, playerName, characterName));
            }
        }

        return scoreList.OrderByDescending(s => s.score).ToList();
    }

    private List<ScoreData> GetPersonalScores()
    {
        if (!SteamManager.Initialized) return new List<ScoreData>();
        
        CSteamID userId = SteamUser.GetSteamID();
        List<ScoreData> personalScores = new List<ScoreData>();

        foreach (var kvp in friendleaderboardEntries)
        {
            string characterName = LeaderboardNames[kvp.Key];
            var userEntry = kvp.Value.Find(e => e.m_steamIDUser == userId);
            
            if (userEntry.m_steamIDUser == userId)
            {
                string playerName = SteamFriends.GetFriendPersonaName(userId);
                personalScores.Add(new ScoreData(userEntry.m_nScore, playerName, characterName));
            }
        }

        return personalScores.OrderByDescending(s => s.score).ToList();
    }

    #endregion

    #region UI Management

    public void SwitchScoreList(ScoreLists scoreList)
    {
        currentScoreList = scoreList;
        List<ScoreData> scores = GetLeaderboard(scoreList);
        
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
            highScores[i].scoreObject = scoreObj;
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
        if (refreshButton != null) refreshButton.SetActive(!isFetchingFromSteam);
        if (noNewData != null) noNewData.SetActive(steamFetchFailed);
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
        if (friendsDataReady && globalDataReady)
        {
            if (content != null) content.SetActive(true);
            if (loadingMessage != null) loadingMessage.SetActive(false);
            
            UpdateRefreshButtonState();
            SwitchScoreList(currentScoreList);
            SaveCacheData();
        }
    }

    #endregion

    #region Cache Management

    private void LoadCachedData()
    {
        if (!File.Exists(cacheFilePath)) return;

        try
        {
            string json = File.ReadAllText(cacheFilePath);
            LeaderboardCache cache = JsonUtility.FromJson<LeaderboardCache>(json);
            if (cache == null || cache.friendsScores == null || cache.globalScores == null) return;

            if (DateTime.TryParse(cache.timestamp, out DateTime cacheTime))
            {
                cacheTimestamp = cacheTime;
            }
            else 
            {
                cacheTimestamp = DateTime.Now;
            }
            
            friendsScoreList = cache.friendsScores.ConvertAll(s => s.ToScoreData());
            globalScoreList = cache.globalScores.ConvertAll(s => s.ToScoreData());
            
            if (friendsScoreList.Count > 0 || globalScoreList.Count > 0)
            {
                friendsDataReady = true;
                globalDataReady = true;
                isShowingCachedData = true;
                
                UpdateRefreshButtonState();
                SwitchScoreList(currentScoreList);
                
                Debug.Log($"Loaded cached leaderboard data ({friendsScoreList.Count} friends, {globalScoreList.Count} global)");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load cache: {e.Message}");
        }
    }

    private void SaveCacheData()
    {
        try
        {
            LeaderboardCache cache = new LeaderboardCache
            {
                friendsScores = friendsScoreList.ConvertAll(s => new ScoreDataSerializable(s)),
                globalScores = globalScoreList.ConvertAll(s => new ScoreDataSerializable(s)),
                timestamp = DateTime.Now.ToString("o")
            };
            
            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(cacheFilePath, json);
            cacheTimestamp = DateTime.Now;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save cache: {e.Message}");
        }
    }

    #endregion

    #region Error Handling

    private void HandleSteamFetchFailure()
    {
        Debug.LogWarning("Steam fetch failed. Displaying cached data if available.");
        steamFetchFailed = true;
        isFetchingFromSteam = false;
        
        if (content != null) content.SetActive(true);
        if (loadingMessage != null) loadingMessage.SetActive(false);
        
        if (friendsScoreList.Count > 0 || globalScoreList.Count > 0)
        {
            isShowingCachedData = true;
            friendsDataReady = true;
            globalDataReady = true;
            UpdateRefreshButtonState();
            SwitchScoreList(currentScoreList);
        }
        else
        {
            Debug.LogError("No cached data available and Steam fetch failed!");

            if (noNewData != null)
            {
                noNewData.SetActive(true);
                TextMeshProUGUI textComponent = noNewData.GetComponent<TextMeshProUGUI>();
                if (textComponent != null) 
                    textComponent.text = "Unable to contact Steam. Check your internet connection.";
            }
        }
    }

    private void CheckForCompleteFetchFailure()
    {
        if (failedLeaderboardCount >= totalLeaderboardsToFetch)
        {
            HandleSteamFetchFailure();
        }
    }

    #endregion

    #region Helper Methods

    private string GetLeaderboardNameForCharacter(string characterUsed)
    {
        foreach (var kvp in LeaderboardNames)
        {
            if (kvp.Value == characterUsed)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private List<LeaderboardEntry_t> GetCharacterLeaderboardEntries(string characterUsed)
    {
        string leaderboardName = GetLeaderboardNameForCharacter(characterUsed);
        if (leaderboardName != null && friendleaderboardEntries.ContainsKey(leaderboardName))
        {
            return friendleaderboardEntries[leaderboardName];
        }
        return new List<LeaderboardEntry_t>();
    }

    private Dictionary<string, List<LeaderboardEntry_t>> GetEntriesDictionary(ScoreLists scoreType)
    {
        return scoreType == ScoreLists.Friends ? friendleaderboardEntries : globalleaderboardEntries;
    }

    private void RaiseLeaderboardDownloadedEvent(ScoreLists scoreType)
    {
        if (scoreType == ScoreLists.Friends) 
            OnFriendLeaderboardsDownloaded?.Invoke();
        else 
            OnGlobalLeaderboardsDownloaded?.Invoke();
    }

    private bool AllLeaderboardsDownloaded(Dictionary<string, List<LeaderboardEntry_t>> entriesDict, 
                                          Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> callResultsDict)
    {
        HashSet<string> completedSet = (entriesDict == friendleaderboardEntries) ? 
            friendsLeaderboardsCompleted : globalLeaderboardsCompleted;
        
        return completedSet.Count >= LeaderboardNames.Count;
    }

    #endregion
}
