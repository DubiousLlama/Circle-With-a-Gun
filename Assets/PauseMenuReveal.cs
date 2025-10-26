using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuReveal : MonoBehaviour
{
    // When the escape key is pressed, reveal the pause menu (set all children active)
    // If the pause menu is already active, hide it (set all children inactive)
    // Don't constantly toggle it while the key is held down

    private bool isPaused = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(isPaused);
            }
        }

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
