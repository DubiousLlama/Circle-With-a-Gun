using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SteamLeaderboardManager : MonoBehaviour
{
    public static SteamLeaderboardManager Instance { get; private set; }

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

#pragma warning disable CS0414 // Unity's static analysis incorrectly flags some of these as unused
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
    private int lastUploadedScore = 0;
    private string lastUploadedCharacter = "";
    private bool lastUploadSucceeded = false;

#pragma warning restore CS0414

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

    /// <summary>
    /// Represents a pending score awaiting upload with integrity validation.
    /// </summary>
    [System.Serializable]
    private class PendingScoreEntry
    {
        public int score;
        public string characterUsed;
        public string timestamp;
        public string validationHash;
        public int uploadAttempts;
        
        public PendingScoreEntry(int score, string characterUsed)
        {
            this.score = score;
            this.characterUsed = characterUsed;
            this.timestamp = DateTime.UtcNow.ToString("o");
            this.uploadAttempts = 0;
            this.validationHash = GenerateValidationHash(score, characterUsed, this.timestamp);
        }
        
        /// <summary>
        /// Validates the integrity of this pending score entry.
        /// Returns true if the score hasn't been tampered with.
        /// </summary>
        public bool ValidateIntegrity()
        {
            string recalculatedHash = GenerateValidationHash(score, characterUsed, timestamp);
            return validationHash == recalculatedHash;
        }
        
        private static string GenerateValidationHash(int score, string characterUsed, string timestamp)
        {
            // Combine the score data with a device identifier to create a hash
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string input = $"{score}:{characterUsed}:{timestamp}:{deviceId}";
            
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }

    [System.Serializable]
    private class PendingScoresCache
    {
        public List<PendingScoreEntry> pendingScores = new List<PendingScoreEntry>();
        public string lastValidated;
    }

    #endregion

    private string pendingScoresFilePath;
    private List<PendingScoreEntry> pendingScores = new List<PendingScoreEntry>();
    private const string PENDING_SCORES_FILENAME_SUFFIX = "_pending_scores.json";

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
        GetAllLeaderboards();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            UploadTestScore("Commando", 250);
        }
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
            pendingScoresFilePath = Path.Combine(Application.persistentDataPath, $"leaderboard_cache_{userId}{PENDING_SCORES_FILENAME_SUFFIX}");
        }
        else
        {
            cacheFilePath = Path.Combine(Application.persistentDataPath, "leaderboard_cache.json");
            pendingScoresFilePath = Path.Combine(Application.persistentDataPath, $"leaderboard_cache{PENDING_SCORES_FILENAME_SUFFIX}");
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
    /// If Steam is unavailable, queues the score for later upload with tamper protection.
    /// </summary>
    public void UploadScore(int score, string characterUsed, Action onCompleted = null)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning($"Steam not initialized. Queueing score {score} for {characterUsed} for later upload.");
            QueuePendingScore(score, characterUsed);
            onScoreUploadedCallback = onCompleted;
            onScoreUploadedCallback?.Invoke();
            return;
        }

        string leaderboardName = GetLeaderboardNameForCharacter(characterUsed);
        if (leaderboardName == null || !leaderboardHandles.ContainsKey(leaderboardName))
        {
            Debug.LogError($"Leaderboard handle not found for character '{characterUsed}'. Make sure leaderboards have been fetched first.");
            return;
        }

        onScoreUploadedCallback = onCompleted;
        lastUploadedScore = score;
        lastUploadedCharacter = characterUsed;
        lastUploadSucceeded = false;
        
        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(
            leaderboardHandles[leaderboardName], 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, 
            score, 
            null, 
            0
        );
        uploadLeaderboardCallResult.Set(handle);
        Debug.Log($"LM: Uploading score {score} to {characterUsed} leaderboard");
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

    public void ManualRefresh()
    {
        Debug.Log("Manual refresh requested");
        steamFetchFailed = false;
        isShowingCachedData = false;
        
        GetAllLeaderboards();
    }

    #endregion

    #region Pending Score Management

    /// <summary>
    /// Adds a score to the pending upload queue with integrity validation.
    /// </summary>
    private void QueuePendingScore(int score, string characterUsed)
    {
        if (score < 0)
        {
            Debug.LogError("Cannot queue negative score");
            return;
        }

        if (!LeaderboardNames.ContainsValue(characterUsed))
        {
            Debug.LogError($"Invalid character: {characterUsed}");
            return;
        }

        var pendingEntry = new PendingScoreEntry(score, characterUsed);
        pendingScores.Add(pendingEntry);
        SavePendingScores();
        
        Debug.Log($"Queued score {score} for {characterUsed}. Pending uploads: {pendingScores.Count}");
    }

    /// <summary>
    /// Attempts to upload all pending scores. Called when Steam connection is restored.
    /// </summary>
    private void ProcessPendingScores()
    {
        if (pendingScores.Count == 0) return;
        if (!SteamManager.Initialized) return;

        Debug.Log($"Processing {pendingScores.Count} pending score(s)...");

        // Validate integrity before uploading
        var validScores = pendingScores.Where(p => p.ValidateIntegrity()).ToList();
        var invalidScores = pendingScores.Except(validScores).ToList();

        if (invalidScores.Count > 0)
        {
            Debug.LogError($"Detected {invalidScores.Count} tampered score entry(ies). Removing them.");
            foreach (var invalid in invalidScores)
            {
                pendingScores.Remove(invalid);
            }
            SavePendingScores();
        }

        foreach (var pending in validScores)
        {
            pending.uploadAttempts++;
            UploadScore(pending.score, pending.characterUsed, () => OnPendingScoreUploaded(pending));
        }

        SavePendingScores();
    }

    private void OnPendingScoreUploaded(PendingScoreEntry pending)
    {
        pendingScores.Remove(pending);
        SavePendingScores();
        Debug.Log($"Successfully uploaded pending score {pending.score} for {pending.characterUsed}");
    }

    private void LoadPendingScores()
    {
        if (!File.Exists(pendingScoresFilePath)) return;

        try
        {
            string json = File.ReadAllText(pendingScoresFilePath);
            PendingScoresCache cache = JsonUtility.FromJson<PendingScoresCache>(json);
            if (cache?.pendingScores != null)
            {
                pendingScores = cache.pendingScores;
                Debug.Log($"Loaded {pendingScores.Count} pending score(s)");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load pending scores: {e.Message}");
        }
    }

    private void SavePendingScores()
    {
        try
        {
            PendingScoresCache cache = new PendingScoresCache
            {
                pendingScores = pendingScores,
                lastValidated = DateTime.UtcNow.ToString("o")
            };

            string json = JsonUtility.ToJson(cache, true);
            File.WriteAllText(pendingScoresFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save pending scores: {e.Message}");
        }
    }

    /// <summary>
    /// Returns the count of pending scores awaiting upload.
    /// </summary>
    public int GetPendingScoreCount()
    {
        return pendingScores.Count;
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

        // Attempt to process any pending scores when connection is restored
        ProcessPendingScores();

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
        // Check for IO failure first
        if (bIOFailure)
        {
            Debug.LogWarning($"WARNING: Score upload IO failure for {lastUploadedCharacter}. Score {lastUploadedScore} may not have been uploaded due to network or server error.");
            QueuePendingScore(lastUploadedScore, lastUploadedCharacter);
            isShowingCachedData = false;
            onScoreUploadedCallback?.Invoke();
            onScoreUploadedCallback = null;
            return;
        }

        if (result.m_bSuccess == 0)
        {
            Debug.LogWarning($"WARNING: Score upload failed for {lastUploadedCharacter}. Score {lastUploadedScore} was not accepted by the leaderboard.");
            QueuePendingScore(lastUploadedScore, lastUploadedCharacter);
            isShowingCachedData = false;
            onScoreUploadedCallback?.Invoke();
            onScoreUploadedCallback = null;
            return;
        }

        // Check if the score was actually changed/accepted by the leaderboard
        // result.m_bScoreChanged indicates if the score replaced a previous best
        if (result.m_bScoreChanged == 0)
        {
            Debug.LogWarning($"WARNING: Score upload completed but score was NOT changed/accepted for {lastUploadedCharacter}. " +
                $"Uploaded score {lastUploadedScore} may be lower than or equal to existing best score.");
        }
        else
        {
            Debug.Log($"Successfully uploaded score {lastUploadedScore} for {lastUploadedCharacter}. New rank: {result.m_nGlobalRankNew}");
            lastUploadSucceeded = true;
        }

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
                
                Debug.Log($"Loaded cached leaderboard data ({friendsScoreList.Count} friends, {globalScoreList.Count} global)");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load cache: {e.Message}");
        }

        LoadPendingScores();
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
        
        if (friendsScoreList.Count > 0 || globalScoreList.Count > 0)
        {
            isShowingCachedData = true;
            friendsDataReady = true;
            globalDataReady = true;
        }
        else
        {
            Debug.LogError("No cached data available and Steam fetch failed!");
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
