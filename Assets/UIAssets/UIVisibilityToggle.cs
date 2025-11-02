using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIVisibilityToggle : MonoBehaviour
{
    [Header("Recording Mode Settings")]
    [Tooltip("Check this to hide UI for recording footage")]
    public bool hideUIForRecording = false;

    [Header("Scene Check")]
    [Tooltip("Only work in this scene (leave empty to work in all scenes)")]
    public string onlyInSceneName = "GunTime";

    [Header("UI Elements to Hide (Drag from Hierarchy)")]
    [Tooltip("Drag UI GameObjects here that you want to hide")]
    public List<GameObject> uiElementsToHide = new List<GameObject>();

    private bool IsInCorrectScene()
    {
        if (string.IsNullOrEmpty(onlyInSceneName))
            return true;
        
        string currentScene = SceneManager.GetActiveScene().name;
        return currentScene == onlyInSceneName;
    }

    private void Start()
    {
        if (IsInCorrectScene())
        {
            ApplyUIVisibility();
        }
    }

    public void ApplyUIVisibility()
    {
        // Only apply if we're in the correct scene
        if (!IsInCorrectScene())
            return;

        int affectedCount = 0;

        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null)
            {
                // Try to find and use CanvasGroup (best method - allows fade and keeps everything functional)
                CanvasGroup canvasGroup = uiElement.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    // If no CanvasGroup exists, add one
                    canvasGroup = uiElement.AddComponent<CanvasGroup>();
                }
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = hideUIForRecording ? 0 : 1;
                    // Also disable interaction when hidden
                    canvasGroup.interactable = !hideUIForRecording;
                    canvasGroup.blocksRaycasts = !hideUIForRecording;
                    affectedCount++;
                    Debug.Log($"Set {uiElement.name} alpha to {canvasGroup.alpha}");
                }
            }
        }

        if (affectedCount > 0)
        {
            Debug.Log($"Recording Mode: UI {(hideUIForRecording ? "HIDDEN" : "VISIBLE")} - {affectedCount} elements affected");
        }
        else
        {
            Debug.LogWarning("No UI elements were affected! Check if elements are assigned in the list.");
        }
    }

    void Update()
    {
        // Press F9 to toggle UI visibility during gameplay
        if (Input.GetKeyDown(KeyCode.F9))
        {
            hideUIForRecording = !hideUIForRecording;
            ApplyUIVisibility();
        }
    }
}
