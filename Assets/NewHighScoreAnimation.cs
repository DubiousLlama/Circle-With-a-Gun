using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewHighScoreAnimation : MonoBehaviour
{
    public bool playing = false;

    public GameObject content;
    public Roster roster;
    public GameObject scoreDisplayPrefab;
    public ScrollRect scrollRect;
    public float scrollSpeed = 8f; // How fast the scroll follows the moving score

    public float fadeInDuration = 0.5f;
    public float smashInterval = 0.15f; // Time between each smash

    [Range(0.85f, 1f)]
    public float accelIncrease = 0.95f;

    private float accelFactor = 1f;

    public class ScoreData
    {
        public int score;
        public string playerName;
        public string characterUsed;
        public GameObject scoreObject;

        public ScoreData(int score, string playerName, string characterUsed)
        {
            this.score = score;
            this.playerName = playerName;
            this.characterUsed = characterUsed;
            this.scoreObject = null;
        }
    }

    List<ScoreData> highScores = new List<ScoreData>()
    {
        new ScoreData(162375, "samjett", "BombsMcGee"),
        new ScoreData(161910, "samjett", "Kevin"),
        new ScoreData(87210, "Transcendent Pig", "MissMicro"),
        new ScoreData(64435, "samjett", "ElectricJeff"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(51995, "Transcendent Pig", "BombsMcGee"),
        new ScoreData(10, "John", "Kevin"),
    };

    private void Start()
    {
        // Create UI for all existing high scores
        CreateScoreUI();
        
        // Test animation
        // NewHighScore(new ScoreData(200000, "John", "MissMicro"));
    }

    void CreateScoreUI()
    {
        // Clear any existing score objects in content
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Create UI for each score in the list
        for (int i = 0; i < highScores.Count; i++)
        {
            GameObject scoreObj = Instantiate(scoreDisplayPrefab, content.transform);
            highScores[i].scoreObject = scoreObj;
            UpdateScoreDisplay(scoreObj, highScores[i]);
            UpdateRank(scoreObj, i + 1);
        }
        
        Canvas.ForceUpdateCanvases();
    }

    public void NewHighScore(ScoreData newScore)
    {
        if (playing) return;
        
        playing = true;
        accelFactor = 1f; // Reset acceleration factor
        StartCoroutine(AnimateNewHighScore(newScore));
    }

    IEnumerator AnimateNewHighScore(ScoreData newScore)
    {
        // Find old personal best
        ScoreData oldPersonalBest = highScores.FindLast(s => s.playerName == newScore.playerName);
        
        // Calculate where the new score should go
        int targetPosition = CalculateTargetPosition(newScore.score);
        
        // Create new score GameObject
        GameObject newScoreObject = Instantiate(scoreDisplayPrefab, content.transform);
        newScore.scoreObject = newScoreObject;
        UpdateScoreDisplay(newScoreObject, newScore);
        
        // Add CanvasGroup for visual effects
        CanvasGroup newScoreCG = newScoreObject.GetComponent<CanvasGroup>();
        if (newScoreCG == null)
        {
            newScoreCG = newScoreObject.AddComponent<CanvasGroup>();
        }
        newScoreCG.alpha = 1f;
        
        // Position new score right below old personal best
        int startIndex = oldPersonalBest != null ? highScores.IndexOf(oldPersonalBest) + 1 : highScores.Count;
        newScoreObject.transform.SetSiblingIndex(startIndex);
        
        // Force layout update
        Canvas.ForceUpdateCanvases();
        yield return null;
        
        // Scroll to center on the new score
        yield return StartCoroutine(ScrollToChild(newScoreObject.GetComponent<RectTransform>(), 0.5f));
        
        // Identify scores to smash through
        List<GameObject> scoresToSmash = new List<GameObject>();
        for (int i = startIndex - 1; i >= targetPosition; i--)
        {
            if (i >= 0 && i < highScores.Count && highScores[i].scoreObject != null)
            {
                scoresToSmash.Add(highScores[i].scoreObject);
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
        
        // Update the high scores list
        // Remove old personal best if same character
        if (oldPersonalBest != null && oldPersonalBest.characterUsed == newScore.characterUsed)
        {
            if (oldPersonalBest.scoreObject != null)
            {
                Destroy(oldPersonalBest.scoreObject);
            }
            highScores.Remove(oldPersonalBest);
        }
        
        // Insert new score into list
        targetPosition = CalculateTargetPosition(newScore.score);
        highScores.Insert(targetPosition, newScore);
        
        // Update all ranks (single source of truth: highScores list)
        for (int i = 0; i < highScores.Count; i++)
        {
            if (highScores[i].scoreObject != null)
            {
                UpdateRank(highScores[i].scoreObject, i + 1);
            }
        }
        
        // Fade in smashed scores
        yield return StartCoroutine(FadeInScores(smashedScores));
        
        AudioManager.instance.PlaySfx("Victory");
        playing = false;
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

    int CalculateTargetPosition(int score)
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
