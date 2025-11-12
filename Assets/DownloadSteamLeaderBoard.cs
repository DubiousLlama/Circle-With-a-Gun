using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms.Impl;

public class DownloadSteamLeaderBoard : MonoBehaviour
{
    public GameObject loadingMessage;
    public GameObject content;
    public Roster roster;
    public GameObject scoreDisplayPrefab;

    public Button personalScoreButton;
    public Button friendsScoreButton;
    public Button globalScoreButton;

    public GameObject refreshButton;
    public GameObject noNewData;

    Dictionary<string, string> LeaderboardNames = new Dictionary<string, string>()
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

    // Leaderboard handles and Steam callbacks
    private Dictionary<string, SteamLeaderboard_t> leaderboardHandles = new Dictionary<string, SteamLeaderboard_t>();
    private Dictionary<string, CallResult<LeaderboardFindResult_t>> findLeaderboardCallResults = new Dictionary<string, CallResult<LeaderboardFindResult_t>>();
    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> downloadFriendsCallResults = new Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>>();
    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> downloadGlobalCallResults = new Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>>();
    private CallResult<LeaderboardScoreUploaded_t> uploadLeaderboardCallResult;

    // Downloaded entries
    private Dictionary<string, List<LeaderboardEntry_t>> friendleaderboardEntries = new Dictionary<string, List<LeaderboardEntry_t>>();
    public static event Action OnFriendLeaderboardsDownloaded;

    private Dictionary<string, List<LeaderboardEntry_t>> globalleaderboardEntries = new Dictionary<string, List<LeaderboardEntry_t>>();
    public static event Action OnGlobalLeaderboardsDownloaded;

    // Track which leaderboards have completed downloading
    private HashSet<string> friendsLeaderboardsCompleted = new HashSet<string>();
    private HashSet<string> globalLeaderboardsCompleted = new HashSet<string>();

    // Synthesized ordered lists
    private List<ScoreData> friendsScoreList = new List<ScoreData>();
    private List<ScoreData> globalScoreList = new List<ScoreData>();
    
    // Loading state
    private bool friendsDataReady = false;
    private bool globalDataReady = false;
    private ScoreLists currentScoreList = ScoreLists.Friends;

    // Cache management
    private string cacheFilePath;
    private const float CACHE_EXPIRY_HOURS = 3f;

    private bool isShowingCachedData = false;
    private bool steamFetchFailed = false;
    private bool isFetchingFromSteam = false;
    private DateTime cacheTimestamp;
    private int failedLeaderboardCount = 0;
    private int totalLeaderboardsToFetch = 0;

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

    void Awake()
    {
        if (!SteamManager.Initialized) return;
        // SteamAPI.Init returns whether the Steam API initialized; result intentionally unused here.
        bool x = SteamAPI.Init();
        
        // Initialize cache file path per Steam user
        if (SteamManager.Initialized)
        {
            string userId = SteamUser.GetSteamID().ToString();
            cacheFilePath = Path.Combine(
                Application.persistentDataPath, 
                $"leaderboard_cache_{userId}.json"
            );
        }
        else
        {
            cacheFilePath = Path.Combine(
                Application.persistentDataPath, 
                "leaderboard_cache.json"
            );
        }

        // Load cached data immediately for instant display
        LoadCachedData();
        
        uploadLeaderboardCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);

        foreach (var leaderboardName in LeaderboardNames.Keys)
        {
            // Capture leaderboardName in the callback; with current C# version this is safe per-iteration.
            findLeaderboardCallResults[leaderboardName] = CallResult<LeaderboardFindResult_t>.Create(
                (result, bIOFailure) => OnLeaderboardFound(result, bIOFailure, leaderboardName));
            
            downloadFriendsCallResults[leaderboardName] = CallResult<LeaderboardScoresDownloaded_t>.Create(
                (result, bIOFailure) => OnLeaderboardScoresDownloaded(result, bIOFailure, leaderboardName, ScoreLists.Friends));

            downloadGlobalCallResults[leaderboardName] = CallResult<LeaderboardScoresDownloaded_t>.Create(
                (result, bIOFailure) => OnLeaderboardScoresDownloaded(result, bIOFailure, leaderboardName, ScoreLists.Global));

            friendleaderboardEntries[leaderboardName] = new List<LeaderboardEntry_t>();
            globalleaderboardEntries[leaderboardName] = new List<LeaderboardEntry_t>();
        }

        OnFriendLeaderboardsDownloaded += HandleLeaderboardsReady;
        OnGlobalLeaderboardsDownloaded += HandleLeaderboardsReady;
    }

    private void OnDestroy()
    {
        OnFriendLeaderboardsDownloaded -= HandleLeaderboardsReady;
        OnGlobalLeaderboardsDownloaded -= HandleLeaderboardsReady;
    }

    private void LoadCachedData()
    {
        if (!File.Exists(cacheFilePath)) return;

        try
        {
            string json = File.ReadAllText(cacheFilePath);
            LeaderboardCache cache = JsonUtility.FromJson<LeaderboardCache>(json);
            if (cache == null || cache.friendsScores == null || cache.globalScores == null) return;

            // Check if cache is still valid
            bool isCacheValid = false;
            if (DateTime.TryParse(cache.timestamp, out DateTime cacheTime))
            {
                cacheTimestamp = cacheTime;
                double hoursSinceCached = (DateTime.Now - cacheTime).TotalHours;
                isCacheValid = hoursSinceCached < CACHE_EXPIRY_HOURS;
            }
            else cacheTimestamp = DateTime.Now;
            
            // Load cache data regardless of validity (will be shown if Steam fails)
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
                timestamp = DateTime.Now.ToString("o") // ISO 8601 format
            };
            
            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(cacheFilePath, json);
            cacheTimestamp = DateTime.Now;
            // Debug.Log("Saved leaderboard cache");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save cache: {e.Message}");
        }
    }

    private void UpdateRefreshButtonState()
    {
        if (refreshButton != null) refreshButton.SetActive(!isFetchingFromSteam);
        if (noNewData != null) noNewData.SetActive(steamFetchFailed);
    }

    public void OnRefreshButtonPressed()
    {
        Debug.Log("Manual refresh requested");
        steamFetchFailed = false;
        isShowingCachedData = false;
        
        // Hide content and show loading message during manual refresh
        if (content != null) content.SetActive(false);
        if (loadingMessage != null) loadingMessage.SetActive(true);

        // Re-select the button corresponding to current display state
        switch (currentScoreList)
        {
            case ScoreLists.Personal: if (personalScoreButton != null) personalScoreButton.Select(); break;
            case ScoreLists.Friends: if (friendsScoreButton != null) friendsScoreButton.Select(); break;
            case ScoreLists.Global: if (globalScoreButton != null) globalScoreButton.Select(); break;
        }
        
        UpdateRefreshButtonState();
        GetAllLeaderboards();
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

    private List<ScoreData> GetPersonalScores()
    {
        if (!SteamManager.Initialized) return new List<ScoreData>();
        
        CSteamID userId = SteamUser.GetSteamID();
        List<ScoreData> personalScores = new List<ScoreData>();

        foreach (var kvp in friendleaderboardEntries)
        {
            string characterName = LeaderboardNames[kvp.Key];
            // Find returns default(struct) if not found; checking m_steamIDUser against userId is safe in that case.
            var userEntry = kvp.Value.Find(e => e.m_steamIDUser == userId);
            
            if (userEntry.m_steamIDUser == userId)
            {
                // SteamFriends also returns the local user's persona name via the same API.
                string playerName = SteamFriends.GetFriendPersonaName(userId);
                personalScores.Add(new ScoreData(userEntry.m_nScore, playerName, characterName));
            }
        }

        return personalScores.OrderByDescending(s => s.score).ToList();
    }

    private void Start()
    {
        // Always enable content - we'll show loading overlay only if no cached data
        if (content != null) content.SetActive(true);
            
        // Only show loading overlay if we don't have cached data
        if (loadingMessage != null) loadingMessage.SetActive(!isShowingCachedData);
        
        friendsScoreButton.Select();
        
        if (personalScoreButton != null) personalScoreButton.onClick.AddListener(() => SwitchScoreList(ScoreLists.Personal));
        if (friendsScoreButton != null) friendsScoreButton.onClick.AddListener(() => SwitchScoreList(ScoreLists.Friends));
        if (globalScoreButton != null) globalScoreButton.onClick.AddListener(() => SwitchScoreList(ScoreLists.Global));
        if (refreshButton != null) refreshButton.GetComponent<Button>().onClick.AddListener(OnRefreshButtonPressed);

        // Always fetch fresh data in the background
        GetAllLeaderboards();
    }

    public void GetAllLeaderboards()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            HandleSteamFetchFailure();
            return;
        }

        // Don't reset data ready flags if we're just refreshing cached data
        // Only reset them if this is a manual refresh (not showing cached data)
        if (!isShowingCachedData)
        {
            friendsDataReady = false;
            globalDataReady = false;
        }
        
        // Clear completion tracking for new fetch
        friendsLeaderboardsCompleted.Clear();
        globalLeaderboardsCompleted.Clear();
        
        steamFetchFailed = false;
        isFetchingFromSteam = true;
        failedLeaderboardCount = 0;
        totalLeaderboardsToFetch = LeaderboardNames.Count * 2; // Friends + Global for each
        
        // Update button state to hide refresh during fetch
        UpdateRefreshButtonState();

        foreach (var leaderboardName in LeaderboardNames.Keys)
        {
            SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardName);
            findLeaderboardCallResults[leaderboardName].Set(handle);
            // Debug.Log($"Requested leaderboard: {leaderboardName}");
        }
    }

    private void OnLeaderboardFound(LeaderboardFindResult_t result, bool bIOFailure, string leaderboardName)
    {
        if (bIOFailure || result.m_bLeaderboardFound == 0)
        {
            Debug.LogError($"Failed to find leaderboard '{leaderboardName}'");
            failedLeaderboardCount += 2; // Count as 2 failures (friends + global)
            CheckForCompleteFetchFailure();
            return;
        }

        // Debug.Log($"Leaderboard found: {leaderboardName}");
        leaderboardHandles[leaderboardName] = result.m_hSteamLeaderboard;

        // For friend-based requests, Steam ignores the start/end indices.
        SteamAPICall_t friendsHandle = SteamUserStats.DownloadLeaderboardEntries(
            result.m_hSteamLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends,
            0,
            0
        );
        downloadFriendsCallResults[leaderboardName].Set(friendsHandle);

        SteamAPICall_t globalHandle = SteamUserStats.DownloadLeaderboardEntries(
            result.m_hSteamLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser,
            -25,
            25
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

        // Get the appropriate entries dictionary and completion tracker based on score type
        Dictionary<string, List<LeaderboardEntry_t>> entriesDict = GetEntriesDictionary(scoreType);
        HashSet<string> completedSet = scoreType == ScoreLists.Friends ? friendsLeaderboardsCompleted : globalLeaderboardsCompleted;
        
        entriesDict[leaderboardName].Clear();

        for (int i = 0; i < result.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[0];
            
            if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out entry, details, 0))
            {
                entriesDict[leaderboardName].Add(entry);
            }
        }

        // Mark this leaderboard as completed
        completedSet.Add(leaderboardName);
        
        // Check if ALL leaderboards for this type have been downloaded
        if (completedSet.Count >= LeaderboardNames.Count)
        {
            RaiseLeaderboardDownloadedEvent(scoreType);
        }
    }

    private Dictionary<string, List<LeaderboardEntry_t>> GetEntriesDictionary(ScoreLists scoreType) => scoreType == ScoreLists.Friends ? friendleaderboardEntries : globalleaderboardEntries;

    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> GetCallResultsDictionary(ScoreLists scoreType) => scoreType == ScoreLists.Friends ? downloadFriendsCallResults : downloadGlobalCallResults;

    private void RaiseLeaderboardDownloadedEvent(ScoreLists scoreType)
    {
        if (scoreType == ScoreLists.Friends) OnFriendLeaderboardsDownloaded?.Invoke();
        else OnGlobalLeaderboardsDownloaded?.Invoke();
    }

    private void HandleLeaderboardsReady()
    {
        // Check if fresh data is available for friends or global
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
            // Fresh data received, no longer showing cached data
            isShowingCachedData = false;
            steamFetchFailed = false;
            isFetchingFromSteam = false;
            
            CheckAndDisplayScores();
        }
    }

    private bool AllLeaderboardsDownloaded(Dictionary<string, List<LeaderboardEntry_t>> entriesDict, 
                                          Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> callResultsDict)
    {
        // Determine which type we're checking based on the dictionary reference
        HashSet<string> completedSet = (entriesDict == friendleaderboardEntries) ? 
            friendsLeaderboardsCompleted : globalLeaderboardsCompleted;
        
        return completedSet.Count >= LeaderboardNames.Count;
    }

    private void CheckForCompleteFetchFailure()
    {
        // If all fetches failed, handle the failure
        if (failedLeaderboardCount >= totalLeaderboardsToFetch)
        {
            HandleSteamFetchFailure();
        }
    }

    private void HandleSteamFetchFailure()
    {
        Debug.LogWarning("Steam fetch failed. Displaying cached data if available.");
        steamFetchFailed = true;
        isFetchingFromSteam = false;
        
        // Show content and hide loading message
        if (content != null) content.SetActive(true);
        if (loadingMessage != null) loadingMessage.SetActive(false);
        
        // If we have cached data, show it
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
            // No cached data available - show error message
            Debug.LogError("No cached data available and Steam fetch failed!");

            if (noNewData != null)
            {
                noNewData.SetActive(true);
                TextMeshProUGUI textComponent = noNewData.GetComponent<TextMeshProUGUI>();
                if (textComponent != null) textComponent.text = "Unable to contact Steam. Check your internet connection.";
            }
        }
    }

    private void CheckAndDisplayScores()
    {
        if (friendsDataReady && globalDataReady)
        {
            // Show content and hide loading message when fresh data is ready
            if (content != null) content.SetActive(true);
            if (loadingMessage != null) loadingMessage.SetActive(false);
            
            UpdateRefreshButtonState();
            SwitchScoreList(currentScoreList);
            
            // Save fresh data to cache only when both are ready
            SaveCacheData();
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
                
                // For global leaderboards, request user information if persona name is not available
                if (requestUserInfo && (string.IsNullOrEmpty(playerName) || playerName == entry.m_steamIDUser.ToString()))
                {
                    bool needsUpdate = SteamFriends.RequestUserInformation(entry.m_steamIDUser, false);
                    playerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                    
                    if (string.IsNullOrEmpty(playerName))
                        // Fallback using the last digits of the SteamID when persona is not available.
                        playerName = "Player " + entry.m_steamIDUser.ToString().Substring(entry.m_steamIDUser.ToString().Length - 4);
                }
                
                scoreList.Add(new ScoreData(entry.m_nScore, playerName, characterName));
            }
        }

        return scoreList.OrderByDescending(s => s.score).ToList();
    }

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

    void CreateScoreUI(List<ScoreData> highScores)
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
        if (ch != null) hL.GetChild(1).GetComponent<Image>().sprite = ch.sprite;
    }

    public void UpdateRank(GameObject scoreObject, int rank) => scoreObject.transform.Find("HorizLayout").GetChild(0).GetComponent<TextMeshProUGUI>().text = "#" + rank.ToString();

    public void UploadTestScore(string characterName = "Kevin", int testScore = 678910)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        string leaderboardName = null;
        foreach (var kvp in LeaderboardNames)
        {
            if (kvp.Value == characterName)
            {
                leaderboardName = kvp.Key;
                break;
            }
        }

        if (leaderboardName == null || !leaderboardHandles.ContainsKey(leaderboardName))
        {
            Debug.LogError($"Leaderboard handle not found for character '{characterName}'. Make sure leaderboards have been fetched first.");
            return;
        }

        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(leaderboardHandles[leaderboardName], ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, testScore, null, 0);
        uploadLeaderboardCallResult.Set(handle);
        Debug.Log($"Uploading score {testScore} to {characterName} leaderboard");
    }

    private void OnLeaderboardScoreUploaded(LeaderboardScoreUploaded_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to upload leaderboard score");
            return;
        }

        if (result.m_bSuccess == 0)
        {
            Debug.LogError("Leaderboard score upload was rejected");
            return;
        }

        Debug.Log("Successfully uploaded score!");
        
        // Invalidate cache after upload so we fetch fresh data
        isShowingCachedData = false;
    }

    // Track completion of leaderboard downloads
    private void TrackLeaderboardDownloadCompletion(String leaderboardName, ScoreLists scoreType)
    {
        if (scoreType == ScoreLists.Friends) friendsLeaderboardsCompleted.Add(leaderboardName);
        else globalLeaderboardsCompleted.Add(leaderboardName);
    }
}
