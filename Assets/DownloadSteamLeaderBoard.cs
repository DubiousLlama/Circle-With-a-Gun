using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class DownloadSteamLeaderBoard : MonoBehaviour
{
    public GameObject success;
    public GameObject failure;

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

    // Store leaderboard handles per character
    private Dictionary<string, SteamLeaderboard_t> leaderboardHandles = new Dictionary<string, SteamLeaderboard_t>();
    
    // Callbacks for Steam API - one set per leaderboard
    private Dictionary<string, CallResult<LeaderboardFindResult_t>> findLeaderboardCallResults = new Dictionary<string, CallResult<LeaderboardFindResult_t>>();
    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> downloadFriendsCallResults = new Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>>();
    private Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>> downloadGlobalCallResults = new Dictionary<string, CallResult<LeaderboardScoresDownloaded_t>>();

    private CallResult<LeaderboardScoreUploaded_t> uploadLeaderboardCallResult;

    // Store downloaded user+friends entries per leaderboard
    private Dictionary<string, List<LeaderboardEntry_t>> friendleaderboardEntries = new Dictionary<string, List<LeaderboardEntry_t>>();

    // Store downloaded user+global top 50 entries per leaderboard
    private Dictionary<string, List<LeaderboardEntry_t>> globalleaderboardEntries = new Dictionary<string, List<LeaderboardEntry_t>>();

    void Awake()
    {
        if (!SteamManager.Initialized) return;
        bool x = SteamAPI.Init();
        
        // Initialize callbacks for upload
        uploadLeaderboardCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);

        // Initialize callback dictionaries for each leaderboard
        foreach (var leaderboardName in LeaderboardNames.Keys)
        {
            findLeaderboardCallResults[leaderboardName] = CallResult<LeaderboardFindResult_t>.Create(
                (result, bIOFailure) => OnLeaderboardFound(result, bIOFailure, leaderboardName));
            
            downloadFriendsCallResults[leaderboardName] = CallResult<LeaderboardScoresDownloaded_t>.Create(
                (result, bIOFailure) => OnFriendsLeaderboardScoresDownloaded(result, bIOFailure, leaderboardName));

            downloadGlobalCallResults[leaderboardName] = CallResult<LeaderboardScoresDownloaded_t>.Create(
                (result, bIOFailure) => OnGlobalLeaderboardScoresDownloaded(result, bIOFailure, leaderboardName));

            friendleaderboardEntries[leaderboardName] = new List<LeaderboardEntry_t>();
            globalleaderboardEntries[leaderboardName] = new List<LeaderboardEntry_t>();
        }
    }

    public List<ScoreData> GetLeaderboard(ScoreLists sl)
    {
        return null;
    }

    private void Start()
    {
        GetTop20EntriesAllLeaderboards();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UploadTestScore();
            GetTop20EntriesAllLeaderboards();
        }
    }

    /// <summary>
    /// Gets the top 20 entries from all nine leaderboards in parallel
    /// </summary>
    public void GetTop20EntriesAllLeaderboards()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        // Request all leaderboards in parallel
        foreach (var leaderboardName in LeaderboardNames.Keys)
        {
            SteamAPICall_t handle = SteamUserStats.FindLeaderboard(leaderboardName);
            findLeaderboardCallResults[leaderboardName].Set(handle);
            Debug.Log($"Requested leaderboard: {leaderboardName}");
        }
    }

    /// <summary>
    /// Callback when a leaderboard is found
    /// </summary>
    private void OnLeaderboardFound(LeaderboardFindResult_t result, bool bIOFailure, string leaderboardName)
    {
        if (bIOFailure || result.m_bLeaderboardFound == 0)
        {
            Debug.LogError($"Failed to find leaderboard '{leaderboardName}'");
            return;
        }

        Debug.Log($"Leaderboard found: {leaderboardName}");
        leaderboardHandles[leaderboardName] = result.m_hSteamLeaderboard;

        // Download the friends leaderboard (user + friends)
        SteamAPICall_t friendsHandle = SteamUserStats.DownloadLeaderboardEntries(
            result.m_hSteamLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends,
            0,  // Start index (ignored for friends)
            0   // End index (ignored for friends)
        );
        downloadFriendsCallResults[leaderboardName].Set(friendsHandle);

        // Download the global top 50 entries (including user)
        SteamAPICall_t globalHandle = SteamUserStats.DownloadLeaderboardEntries(
            result.m_hSteamLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser,
            -25, // Start: 25 entries before user
            25   // End: 25 entries after user (total 51 including user)
        );
        downloadGlobalCallResults[leaderboardName].Set(globalHandle);
    }

    /// <summary>
    /// Callback when friends leaderboard scores are downloaded
    /// </summary>
    private void OnFriendsLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t result, bool bIOFailure, string leaderboardName)
    {
        if (bIOFailure)
        {
            Debug.LogError($"Failed to download friends leaderboard scores for '{leaderboardName}'");
            return;
        }

        Debug.Log($"Downloaded {result.m_cEntryCount} friends leaderboard entries for '{leaderboardName}'");

        friendleaderboardEntries[leaderboardName].Clear();

        // Process each entry
        for (int i = 0; i < result.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[0];
            
            if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out entry, details, 0))
            {
                friendleaderboardEntries[leaderboardName].Add(entry);
            }
        }
    }

    /// <summary>
    /// Callback when global leaderboard scores are downloaded
    /// </summary>
    private void OnGlobalLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t result, bool bIOFailure, string leaderboardName)
    {
        if (bIOFailure)
        {
            Debug.LogError($"Failed to download global leaderboard scores for '{leaderboardName}'");
            return;
        }

        Debug.Log($"Downloaded {result.m_cEntryCount} global leaderboard entries for '{leaderboardName}'");

        globalleaderboardEntries[leaderboardName].Clear();

        // Process each entry
        for (int i = 0; i < result.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[0];
            
            if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out entry, details, 0))
            {
                globalleaderboardEntries[leaderboardName].Add(entry);
            }
        }
    }

    /// <summary>
    /// Uploads a test score to the leaderboard for a specific character
    /// </summary>
    /// <param name="characterName">The character name key (e.g., "Kevin")</param>
    /// <param name="testScore">The score value to upload (default is 678910)</param>
    public void UploadTestScore(string characterName = "Kevin", int testScore = 678910)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        // Find the leaderboard name for this character
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

        // Upload the test score
        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(
            leaderboardHandles[leaderboardName], 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, 
            testScore, 
            null, 
            0);
        uploadLeaderboardCallResult.Set(handle);
        Debug.Log($"Uploading score {testScore} to {characterName} leaderboard");
    }

    /// <summary>
    /// Callback when leaderboard score is uploaded
    /// </summary>
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
        success.SetActive(true);
    }
}
