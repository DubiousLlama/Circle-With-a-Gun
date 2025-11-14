using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Preloads scenes in the background without activating them.
/// Use PreloadScene() to start loading, then ActivatePreloaded() to switch to that scene.
/// </summary>
public class ScenePreloader : MonoBehaviour
{
    public static ScenePreloader Instance { get; private set; }

    // Map buildIndex -> AsyncOperation (preload)
    private Dictionary<int, AsyncOperation> _preloads = new Dictionary<int, AsyncOperation>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Start preloading a scene in the background. Does nothing if already preloading.
    /// </summary>
    /// <param name="buildIndex">The scene build index to preload</param>
    public void PreloadScene(int buildIndex)
    {
        if (_preloads.ContainsKey(buildIndex))
        {
            Debug.Log($"Scene {buildIndex} is already being preloaded.");
            return;
        }

        Debug.Log($"Starting preload of scene {buildIndex}");
        var op = SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
        op.allowSceneActivation = false; // Keep it from activating
        _preloads[buildIndex] = op;
        StartCoroutine(TrackPreload(buildIndex, op));
    }

    /// <summary>
    /// Returns the AsyncOperation if currently preloading, otherwise null
    /// </summary>
    public AsyncOperation GetOperation(int buildIndex)
    {
        _preloads.TryGetValue(buildIndex, out var op);
        return op;
    }

    /// <summary>
    /// True when the preload reached the "ready to activate" state (~progress 0.9)
    /// </summary>
    public bool IsReadyToActivate(int buildIndex)
    {
        var op = GetOperation(buildIndex);
        return op != null && op.progress >= 0.9f;
    }

    /// <summary>
    /// Returns normalized progress [0..1] (maps op.progress 0..0.9 to 0..1)
    /// </summary>
    public float GetProgress(int buildIndex)
    {
        var op = GetOperation(buildIndex);
        if (op == null) return 0f;
        return Mathf.Clamp01(op.progress / 0.9f);
    }

    /// <summary>
    /// Allow the preloaded scene to activate (will switch scenes if LoadSceneMode.Single)
    /// </summary>
    public void ActivatePreloaded(int buildIndex)
    {
        if (!_preloads.TryGetValue(buildIndex, out var op))
        {
            Debug.LogWarning($"Scene {buildIndex} was not preloaded. Loading normally instead.");
            // Not preloaded — fall back to a normal load
            SceneManager.LoadScene(buildIndex);
            return;
        }

        Debug.Log($"Activating preloaded scene {buildIndex}");
        op.allowSceneActivation = true;
        // Remove mapping now; the AsyncOperation will complete and isDone will become true
        _preloads.Remove(buildIndex);
    }

    /// <summary>
    /// Cancel a preload operation if it exists
    /// </summary>
    public void CancelPreload(int buildIndex)
    {
        if (_preloads.ContainsKey(buildIndex))
        {
            Debug.Log($"Canceling preload of scene {buildIndex}");
            _preloads.Remove(buildIndex);
        }
    }

    private IEnumerator TrackPreload(int buildIndex, AsyncOperation op)
    {
        // Wait until progress reaches the 0.9 ready-to-activate threshold
        while (op != null && op.progress < 0.9f)
        {
            yield return null;
        }
        Debug.Log($"Scene {buildIndex} preloaded and ready to activate (progress: {op.progress})");
        yield break;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
