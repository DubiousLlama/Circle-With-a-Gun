using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuReveal : MonoBehaviour
{
    // When the escape key is pressed, reveal the pause menu (set all children active)
    // If the pause menu is already active, hide it (set all children inactive)
    // Don't constantly toggle it while the key is held down

    [Header("References")]
    public GameObject levelUpMenu;
    public GameObject gameOverScreen;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Don't allow pausing if upgrade menu or game over screen is active
            if (IsOtherMenuActive())
            {
                return;
            }

            // Toggle pause state
            if (!isPaused)
            {
                Time.timeScale = 0f; // Pause the game
            } 
            else
            {
                Time.timeScale = 1f; // Unpause the game
            }
            
            isPaused = !isPaused;
            
            // Show/hide pause menu children
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(isPaused);
            }
        }
    }

    private bool IsOtherMenuActive()
    {
        // Check if level up menu is showing
        if (levelUpMenu != null && levelUpMenu.activeInHierarchy)
        {
            return true;
        }

        // Check if game over screen is showing
        if (gameOverScreen != null && gameOverScreen.activeInHierarchy)
        {
            return true;
        }

        return false;
    }
}
