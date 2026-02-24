using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewHighScoreAnimation : MonoBehaviour
{
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
        
        // Find old personal best (same player, same character)
        ScoreData oldPersonalBest = highScores.FindLast(s => 
            s.playerName == newScore.playerName && s.characterUsed == newScore.characterUsed);
        GameObject oldPersonalBestObject = null;
        
        // Find the GameObject corresponding to the old personal best
        if (oldPersonalBest != null)
        {
            for (int i = 0; i < content.transform.childCount; i++)
            {
                Transform scoreTransform = content.transform.GetChild(i);
                if (scoreTransform != null)
                {
                    // Check if this score matches the old personal best
                    TextMeshProUGUI scoreText = scoreTransform.Find("Score").GetComponent<TextMeshProUGUI>();
                    if (scoreText != null && scoreText.text == oldPersonalBest.score.ToString("N0"))
                    {
                        TextMeshProUGUI playerNameText = scoreTransform.Find("HorizLayout").GetChild(2).GetComponent<TextMeshProUGUI>();
                        if (playerNameText != null && playerNameText.text == oldPersonalBest.playerName)
                        {
                            oldPersonalBestObject = scoreTransform.gameObject;
                            break;
                        }
                    }
                }
            }
        }
        
        // Calculate where the new score should go
        int targetPosition = CalculateTargetPosition(newScore.score, highScores);
        
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
        
        // Position new score right below old personal best
        int startIndex = oldPersonalBestObject != null ? oldPersonalBestObject.transform.GetSiblingIndex() + 1 : highScores.Count;
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
        
        // Separate smashed scores into those to fade in and those to destroy
        List<GameObject> scoresToFadeIn = new List<GameObject>();
        foreach (GameObject smashedScore in smashedScores)
        {
            // If this is the old personal best, destroy it instead of fading it in
            if (smashedScore == oldPersonalBestObject)
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

    int CalculateTargetPosition(int score, List<ScoreData> highScores)
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            if (score > highScores[i].score)
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
