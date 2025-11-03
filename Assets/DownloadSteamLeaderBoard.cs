using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class DownloadSteamLeaderBoard : MonoBehaviour
{
    // Class to store leaderboard entry data
    public class LeaderboardEntry
    {
        public int rank;
        public string playerName;
        public int score;
        public CSteamID steamID;

        public LeaderboardEntry(int rank, string playerName, int score, CSteamID steamID = default)
        {
            this.rank = rank;
            this.playerName = playerName;
            this.score = score;
            this.steamID = steamID;
        }
    }

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


    // Store leaderboard handle
    private SteamLeaderboard_t currentLeaderboard;
    
    // Callbacks for Steam API
    private CallResult<LeaderboardFindResult_t> findLeaderboardCallResult;
    private CallResult<LeaderboardScoresDownloaded_t> downloadLeaderboardCallResult;
    private CallResult<LeaderboardScoreUploaded_t> uploadLeaderboardCallResult;

    // Store downloaded entries
    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();

    void Awake()
    {
        if (!SteamManager.Initialized) return;
        bool x = SteamAPI.Init();
        
        // Initialize callbacks
        findLeaderboardCallResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
        downloadLeaderboardCallResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
        uploadLeaderboardCallResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardScoreUploaded);
    }

    private void Start()
    {
        GetTop20Entries();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UploadTestScore();
            GetTop20Entries();
        }
    }

    /// <summary>
    /// Gets the top 20 entries from the "kv_hs" global leaderboard
    /// </summary>
    public void GetTop20Entries()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        // Find the leaderboard first
        SteamAPICall_t handle = SteamUserStats.FindLeaderboard("High Score: Kevin");
        findLeaderboardCallResult.Set(handle);
    }

    /// <summary>
    /// Callback when the leaderboard is found
    /// </summary>
    private void OnLeaderboardFound(LeaderboardFindResult_t result, bool bIOFailure)
    {
        if (bIOFailure || result.m_bLeaderboardFound == 0)
        {
            Debug.LogError("Failed to find leaderboard 'High Score: Kevin'");
            failure.SetActive(true);
            failure.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = SteamUtils.GetAppID().ToString();
            return;
        }

        Debug.Log("Leaderboard found!");
        currentLeaderboard = result.m_hSteamLeaderboard;

        // Download the top 20 global entries
        SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(
            currentLeaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
            1,  // Start rank (1-based)
            20  // End rank
        );
        
        downloadLeaderboardCallResult.Set(handle);
    }

    /// <summary>
    /// Callback when leaderboard scores are downloaded
    /// </summary>
    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t result, bool bIOFailure)
    {
        if (bIOFailure)
        {
            Debug.LogError("Failed to download leaderboard scores");
            return;
        }

        Debug.Log($"Downloaded {result.m_cEntryCount} leaderboard entries");

        leaderboardEntries.Clear();

        // Process each entry
        for (int i = 0; i < result.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[0];
            
            if (SteamUserStats.GetDownloadedLeaderboardEntry(result.m_hSteamLeaderboardEntries, i, out entry, details, 0))
            {
                string playerName = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                LeaderboardEntry leaderboardEntry = new LeaderboardEntry(
                    entry.m_nGlobalRank,
                    playerName,
                    entry.m_nScore,
                    entry.m_steamIDUser
                );
                
                leaderboardEntries.Add(leaderboardEntry);
                Debug.Log($"Rank {leaderboardEntry.rank}: {leaderboardEntry.playerName} - Score: {leaderboardEntry.score}");
            }
        }
    }

    /// <summary>
    /// Uploads a test score to the "kv_hs" leaderboard
    /// </summary>
    /// <param name="testScore">The score value to upload (default is 12345)</param>
    public void UploadTestScore(int testScore = 678910)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam is not initialized!");
            return;
        }

        // Upload the test score
        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, testScore, null, 0);
        uploadLeaderboardCallResult.Set(handle);
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

        Debug.Log($"Successfully uploaded score!");
        success.SetActive(true);
    }
}
