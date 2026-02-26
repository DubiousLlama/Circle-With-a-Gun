using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewHighScoreAnimation : MonoBehaviour
{
    private const string DebugLogPath = @"C:\Users\Seamus\Circle With a Gun\debug-f49438.log";
    /// <summary>True while the new-high-score animation is running or about to run. LeaderboardView skips refreshing content when this is set to avoid wiping the new score row.</summary>
    public static bool SuppressLeaderboardRefresh { get; private set; }

    public bool playing = false;

    public GameObject content;
    public Roster roster;
    public GameObject scoreDisplayPrefab;
    public ScrollRect scrollRect;

    public SteamLeaderboardManager leaderboardManager;

    public float scrollSpeed = 8f; // How fast the scroll follows the moving score

    public float fadeInDuration = 0.5f;
    public float smashInterval = 0.15f; // Time between each smash

    [Range(0.85f, 1f)]
    public float accelIncrease = 0.95f;

    private float accelFactor = 1f;
    
    private ScoreData pendingScore = null;
    /// <summary>True when pendingScore came from GameManager (we uploaded before loading this scene). We still play the animation without re-checking the friends list, which may already contain the uploaded score.</summary>
    private bool pendingScoreFromGameManager = false;
    private bool friendsDataReady = false;

    private void OnEnable()
    {
        SteamLeaderboardManager.OnFriendLeaderboardsDownloaded += OnFriendsDataReady;
    }

    private void OnDisable()
    {
        SteamLeaderboardManager.OnFriendLeaderboardsDownloaded -= OnFriendsDataReady;
    }

    private void Start()
    {
        leaderboardManager = SteamLeaderboardManager.Instance;

        // Check if GameManager has a new high score to display
        if (GameManager.Instance != null && GameManager.Instance.NewHighScore)
        {
            int score = GameManager.Instance.NewHighScoreValue;
            string characterUsed = GameManager.Instance.NewHighScoreCharacter;
            
            // Get player name from Steam
            string playerName = "Player";
            if (SteamManager.Initialized)
            {
                playerName = Steamworks.SteamFriends.GetFriendPersonaName(Steamworks.SteamUser.GetSteamID());
            }
            
            ScoreData newScore = new ScoreData(score, playerName, characterUsed);
            NewHighScore(newScore, fromGameManager: true);
            
            // Clear the flag so it doesn't trigger again
            GameManager.Instance.NewHighScore = false;
        }
    }

    private void OnFriendsDataReady()
    {
        friendsDataReady = true;

        // If we have a pending score, play the animation if it's a new high score OR it came from GameManager (score was already uploaded; friends list may already contain it).
        if (pendingScore != null && !playing)
        {
            bool shouldPlay = pendingScoreFromGameManager || IsNewHighScore(pendingScore);
            if (shouldPlay)
            {
                SuppressLeaderboardRefresh = true; // Prevent LeaderboardView from wiping content when it receives the same event
                StartCoroutine(PlayNewHighScoreAnimation(pendingScore));
            }
            pendingScore = null;
            pendingScoreFromGameManager = false;
        }
    }

    /// <param name="fromGameManager">True when this score was set by ReturnMainMenu (score already uploaded). We play the animation without re-checking the friends list, which may already include the uploaded score.</param>
    public void NewHighScore(ScoreData newScore, bool fromGameManager = false)
    {
        if (playing) return;

        // If friends data is ready, play immediately if it's a new high score OR from GameManager; otherwise queue it
        if (friendsDataReady)
        {
            bool shouldPlay = fromGameManager || IsNewHighScore(newScore);
            if (shouldPlay)
            {
                SuppressLeaderboardRefresh = true;
                StartCoroutine(PlayNewHighScoreAnimation(newScore));
            }
        }
        else
        {
            pendingScore = newScore;
            pendingScoreFromGameManager = fromGameManager;
        }
    }

    private bool IsNewHighScore(ScoreData newScore)
    {
        // Get the friends leaderboard
        List<ScoreData> highScores = leaderboardManager.GetLeaderboard(ScoreLists.Friends);
        
        // Find the previous best score by this player with this character
        ScoreData previousBest = highScores.FindLast(s => 
            s.playerName == newScore.playerName && s.characterUsed == newScore.characterUsed);
        
        // If no previous score exists, it's a new high score
        if (previousBest == null)
        {
            return true;
        }
        
        // Otherwise, check if the new score is higher than the previous best
        return newScore.score > previousBest.score;
    }

    IEnumerator PlayNewHighScoreAnimation(ScoreData newScore)
    {
        playing = true;
        accelFactor = 1f; // Reset acceleration factor
        try
        {
        yield return null; // Wait one frame for UI to update
        
        // Get the friends score list
        List<ScoreData> highScores = leaderboardManager.GetLeaderboard(ScoreLists.Friends);
        if (highScores == null) highScores = new List<ScoreData>();
        
        // Find old personal best row directly from the UI content, not from the data list.
        // The data list may have already been refreshed (overwriting the old score on Steam),
        // but the UI content still shows the pre-upload rows.
        Sprite newCharSprite = null;
        Character newCharacter = roster.allCharacters.Find(c => c.prefName == newScore.characterUsed);
        if (newCharacter != null) newCharSprite = newCharacter.sprite;

        GameObject oldPersonalBestObject = null;
        int oldPersonalBestScore = 0;

        for (int i = 0; i < content.transform.childCount; i++)
        {
            Transform scoreTransform = content.transform.GetChild(i);
            if (scoreTransform == null) continue;

            Transform hL = scoreTransform.Find("HorizLayout");
            Transform scoreT = scoreTransform.Find("Score");
            if (hL == null || hL.childCount <= 2 || scoreT == null) continue;

            TextMeshProUGUI nameText = hL.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = scoreT.GetComponent<TextMeshProUGUI>();
            if (nameText == null || scoreText == null) continue;
            if (nameText.text != newScore.playerName) continue;

            if (newCharSprite != null)
            {
                Image portrait = hL.GetChild(1).GetComponent<Image>();
                if (portrait == null || portrait.sprite != newCharSprite) continue;
            }

            int displayedScore;
            if (!int.TryParse(scoreText.text.Replace(",", ""), out displayedScore)) continue;
            if (displayedScore >= newScore.score) continue;

            if (oldPersonalBestObject == null || displayedScore > oldPersonalBestScore)
            {
                oldPersonalBestObject = scoreTransform.gameObject;
                oldPersonalBestScore = displayedScore;
            }
        }

        #region agent log
        DebugLog(
            "post-fix",
            "H1",
            "NewHighScoreAnimation.PlayNewHighScoreAnimation",
            "Resolved old personal best from UI content",
            "{\"newPlayer\":\"" + EscapeJson(newScore.playerName) + "\",\"newCharacter\":\"" + EscapeJson(newScore.characterUsed) + "\",\"newScore\":" + newScore.score + ",\"highScoresCount\":" + highScores.Count + ",\"contentChildCount\":" + content.transform.childCount + ",\"oldBestRowFound\":" + (oldPersonalBestObject != null ? "true" : "false") + ",\"oldBestScore\":" + (oldPersonalBestObject != null ? oldPersonalBestScore.ToString() : "null") + ",\"oldBestRowSibling\":" + (oldPersonalBestObject != null ? oldPersonalBestObject.transform.GetSiblingIndex().ToString() : "null") + "}"
        );
        #endregion
        
        // Exclude our new score from the list if it's already in the leaderboard (e.g. after upload/refresh),
        // so we don't compare against ourselves and get an off-by-one target position.
        List<ScoreData> scoresWithoutNew = highScores.FindAll(s =>
            !(s.playerName == newScore.playerName && s.characterUsed == newScore.characterUsed && s.score == newScore.score));
        // Calculate where the new score should go
        int targetPosition = CalculateTargetPosition(newScore.score, scoresWithoutNew);
        
        // Create new score GameObject
        GameObject newScoreObject = Instantiate(scoreDisplayPrefab, content.transform);
        UpdateScoreDisplay(newScoreObject, newScore);

        // Add CanvasGroup for visual effects
        CanvasGroup newScoreCG = newScoreObject.GetComponent<CanvasGroup>();
        if (newScoreCG == null)
        {
            newScoreCG = newScoreObject.AddComponent<CanvasGroup>();
        }
        newScoreCG.alpha = 1f;
        
        // Always start the new score at the bottom of the leaderboard for the dramatic rise-up animation.
        // oldPersonalBestObject is only used later to destroy that row so it doesn't fade back in.
        int startIndex = content.transform.childCount - 1;
        if (startIndex < 0) startIndex = 0;
        newScoreObject.transform.SetSiblingIndex(startIndex);
        
        // Force layout update
        Canvas.ForceUpdateCanvases();
        yield return null;
        
        // Scroll to center on the new score
        yield return StartCoroutine(ScrollToChild(newScoreObject.GetComponent<RectTransform>(), 0.5f));
        
        // Identify scores to smash through (by sibling index, not by storing references)
        List<GameObject> scoresToSmash = new List<GameObject>();
        for (int i = startIndex - 1; i >= targetPosition; i--)
        {
            if (i >= 0 && i < content.transform.childCount)
            {
                Transform scoreTransform = content.transform.GetChild(i);
                if (scoreTransform != null && scoreTransform.gameObject != newScoreObject)
                {
                    scoresToSmash.Add(scoreTransform.gameObject);
                }
            }
        }

        string firstSmash = scoresToSmash.Count > 0 ? GetScoreRowSummary(scoresToSmash[0]) : "";
        string lastSmash = scoresToSmash.Count > 0 ? GetScoreRowSummary(scoresToSmash[scoresToSmash.Count - 1]) : "";
        #region agent log
        DebugLog(
            "pre-fix",
            "H3",
            "NewHighScoreAnimation.PlayNewHighScoreAnimation",
            "Calculated movement range and smash list",
            "{\"targetPosition\":" + targetPosition + ",\"startIndex\":" + startIndex + ",\"scoresToSmashCount\":" + scoresToSmash.Count + ",\"firstSmash\":\"" + firstSmash + "\",\"lastSmash\":\"" + lastSmash + "\"}"
        );
        #endregion
        
        // Smash through each score with animation
        List<GameObject> smashedScores = new List<GameObject>();
        foreach (GameObject scoreToSmash in scoresToSmash)
        {
            // Smash effect
            OnScorePassedBy(scoreToSmash, accelFactor);
            
            // Make transparent
            CanvasGroup cg = scoreToSmash.GetComponent<CanvasGroup>();
            if (cg == null) cg = scoreToSmash.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            smashedScores.Add(scoreToSmash);
            
            // Wait for smash effect to be visible
            yield return new WaitForSeconds(smashInterval * accelFactor * 0.67f);

            // Move new score up one position in hierarchy
            int currentIndex = newScoreObject.transform.GetSiblingIndex();
            newScoreObject.transform.SetSiblingIndex(currentIndex - 1);
            
            // Force layout update
            Canvas.ForceUpdateCanvases();
            StartCoroutine(ScrollToChild(newScoreObject.GetComponent<RectTransform>(), smashInterval * accelFactor * 0.5f));
            yield return new WaitForSeconds(smashInterval * accelFactor * 0.33f);

            accelFactor *= accelIncrease; // Slightly speed up subsequent smashes
        }
        
        // Update all ranks for visible scores
        for (int i = 0; i < content.transform.childCount; i++)
        {
            Transform scoreTransform = content.transform.GetChild(i);
            if (scoreTransform != null)
            {
                UpdateRank(scoreTransform.gameObject, i + 1);
            }
        }
        
        // Separate smashed scores into those to fade in and those to destroy.
        // Destroy any row that displays our old personal best (by reference or by displayed data)
        // so it never fades back in.
        List<GameObject> scoresToFadeIn = new List<GameObject>();

        #region agent log
        DebugLog(
            "post-fix",
            "H4",
            "NewHighScoreAnimation.PlayNewHighScoreAnimation",
            "Smashed rows matching old personal best",
            "{\"smashedCount\":" + smashedScores.Count + ",\"oldBestInSmashedByReference\":" + ((oldPersonalBestObject != null && smashedScores.Contains(oldPersonalBestObject)) ? "true" : "false") + "}"
        );
        #endregion

        foreach (GameObject smashedScore in smashedScores)
        {
            bool isOldPersonalBest = oldPersonalBestObject != null && smashedScore == oldPersonalBestObject;

            #region agent log
            DebugLog(
                "post-fix",
                "H2",
                "NewHighScoreAnimation.PlayNewHighScoreAnimation",
                "Row destroy-or-fade decision",
                "{\"row\":\"" + GetScoreRowSummary(smashedScore) + "\",\"matchesByReference\":" + (isOldPersonalBest ? "true" : "false") + "}"
            );
            #endregion

            if (isOldPersonalBest)
            {
                Destroy(smashedScore);
            }
            else
            {
                scoresToFadeIn.Add(smashedScore);
            }
        }
        
        // Fade in remaining smashed scores
        yield return StartCoroutine(FadeInScores(scoresToFadeIn));
        
        AudioManager.instance.PlaySfx("Victory");

        // Score was already uploaded when the new high score was detected (ReturnMainMenu); no need to upload again.
        }
        finally
        {
            playing = false;
            SuppressLeaderboardRefresh = false;
        }
    }

    IEnumerator ScrollToChild(RectTransform target, float duration)
    {
        if (scrollRect == null || target == null) yield break;
        
        float startTime = Time.time;
        float startScroll = scrollRect.verticalNormalizedPosition;
        
        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            
            // Calculate target scroll position to center the child
            float targetScroll = CalculateScrollToChild(target);
            
            // Smooth interpolation
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startScroll, targetScroll, t);
            
            yield return null;
        }
    }

    float CalculateScrollToChild(RectTransform target)
    {
        if (scrollRect == null || scrollRect.viewport == null || target == null) return 0.5f;
        
        Canvas.ForceUpdateCanvases();
        
        RectTransform content = this.content.GetComponent<RectTransform>();
        RectTransform viewport = scrollRect.viewport;
        
        // Get the target's position relative to the content
        Vector2 targetLocalPos = content.InverseTransformPoint(target.position);
        
        // Content height and viewport height
        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;
        
        if (contentHeight <= viewportHeight) return 1f; // Content fits, no scroll needed
        
        // Calculate how far down the target is from the top of content (as positive value)
        float targetDistanceFromTop = -targetLocalPos.y;
        
        // We want to center the target in the viewport
        float desiredScrollPosition = targetDistanceFromTop - (viewportHeight / 2f);
        
        // Normalize (0 = top, 1 = bottom for scroll offset)
        float maxScroll = contentHeight - viewportHeight;
        float normalizedScroll = desiredScrollPosition / maxScroll;
        
        // Invert for Unity's coordinate system (1 = top, 0 = bottom)
        return Mathf.Clamp01(1f - normalizedScroll);
    }

    /// <summary>Returns the 0-based sibling index where the new score should end up. Uses >= so we place above ties (first among equal scores) and smash all scores less than or equal to us.</summary>
    int CalculateTargetPosition(int score, List<ScoreData> highScores)
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            if (score >= highScores[i].score)
            {
                return i;
            }
        }
        return highScores.Count;
    }

    IEnumerator FadeInScores(List<GameObject> scoreObjects)
    {
        List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
        
        foreach (GameObject scoreObject in scoreObjects)
        {
            if (scoreObject != null)
            {
                CanvasGroup cg = scoreObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    canvasGroups.Add(cg);
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = elapsed / fadeInDuration;
            
            foreach (CanvasGroup cg in canvasGroups)
            {
                if (cg != null)
                {
                    cg.alpha = alpha;
                }
            }
            
            yield return null;
        }

        foreach (CanvasGroup cg in canvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = 1f;
            }
        }
    }

    /// <summary>Returns true if this score row displays the given player name and score (used to identify old personal best for destroy vs fade-in).</summary>
    bool ScoreRowDisplaysScore(GameObject scoreRow, string playerName, int score, string characterUsed = null)
    {
        if (scoreRow == null) return false;
        Transform scoreT = scoreRow.transform.Find("Score");
        Transform hL = scoreRow.transform.Find("HorizLayout");
        if (scoreT == null || hL == null || hL.childCount <= 2) return false;
        TextMeshProUGUI scoreText = scoreT.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = hL.GetChild(2).GetComponent<TextMeshProUGUI>();
        if (scoreText == null || nameText == null) return false;
        bool basicMatch = nameText.text == playerName && scoreText.text == score.ToString("N0");
        if (!basicMatch) return false;
        if (string.IsNullOrEmpty(characterUsed)) return true;

        Character ch = roster != null ? roster.allCharacters.Find(c => c.prefName == characterUsed) : null;
        if (ch == null) return true; // Fallback when roster lookup is unavailable.
        Image portrait = hL.GetChild(1).GetComponent<Image>();
        return portrait != null && portrait.sprite == ch.sprite;
    }

    string GetScoreRowSummary(GameObject scoreRow)
    {
        if (scoreRow == null) return "null";
        Transform scoreT = scoreRow.transform.Find("Score");
        Transform hL = scoreRow.transform.Find("HorizLayout");
        string score = "unknown";
        string player = "unknown";
        if (scoreT != null)
        {
            TextMeshProUGUI scoreText = scoreT.GetComponent<TextMeshProUGUI>();
            if (scoreText != null) score = scoreText.text;
        }
        if (hL != null && hL.childCount > 2)
        {
            TextMeshProUGUI nameText = hL.GetChild(2).GetComponent<TextMeshProUGUI>();
            if (nameText != null) player = nameText.text;
        }
        return EscapeJson($"{scoreRow.transform.GetSiblingIndex()}|{player}|{score}");
    }

    string EscapeJson(string value)
    {
        if (value == null) return "";
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    void DebugLog(string runId, string hypothesisId, string location, string message, string dataJson)
    {
        string payload = "{\"sessionId\":\"f49438\",\"runId\":\"" + EscapeJson(runId) + "\",\"hypothesisId\":\"" + EscapeJson(hypothesisId) + "\",\"location\":\"" + EscapeJson(location) + "\",\"message\":\"" + EscapeJson(message) + "\",\"data\":" + dataJson + ",\"timestamp\":" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}";
        File.AppendAllText(DebugLogPath, payload + Environment.NewLine);
    }

    void UpdateScoreDisplay(GameObject scoreObject, ScoreData score)
    {
        Transform hL = scoreObject.transform.Find("HorizLayout");
        hL.GetChild(0).GetComponent<TextMeshProUGUI>().text = ""; // Rank (set later)
        scoreObject.transform.Find("Score").GetComponent<TextMeshProUGUI>().text = score.score.ToString("N0");
        hL.GetChild(2).GetComponent<TextMeshProUGUI>().text = score.playerName;

        Character ch = roster.allCharacters.Find(c => c.prefName == score.characterUsed);
        if (ch != null)
        {
            hL.GetChild(1).GetComponent<Image>().sprite = ch.sprite;
        }
    }

    void UpdateRank(GameObject scoreObject, int rank)
    {
        scoreObject.transform.Find("HorizLayout").GetChild(0).GetComponent<TextMeshProUGUI>().text = "#" + rank.ToString();
    }

    void OnScorePassedBy(GameObject scoreObject, float accel=1f)
    {
        AudioManager.instance.PlaySfx("Smash");
        SmashableObject smashable = scoreObject.GetComponent<SmashableObject>();
        if (smashable != null)
        {
            smashable.SpawnParticles(accel);
        }
    }
}
