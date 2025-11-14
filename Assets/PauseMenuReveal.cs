using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuReveal : MonoBehaviour
{
    // When the escape key is pressed, reveal the pause menu (set all children active)
    // If the pause menu is already active, hide it (set all children inactive)
    // Don't constantly toggle it while the key is held down

    [Header("References")]
    public GameObject levelUpMenu;
    public GameObject gameOverScreen;

    private bool isPaused = false;

    private PlayerInputActions inputActions = null;

    void Awake()
    {
        if (!Platform.IsMobile())
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Pause.performed += ctx => OnPausePressed();
            inputActions.Enable();
        }
    }


    void OnPausePressed()
    {
        isPaused = !isPaused;
    }

    void Update()
    {
        // Toggle pause state
        if (isPaused)
        {
            GameManager.Instance.RequestPause(GameManager.PauseReason.PauseMenu);
        } 
        else
        {
            GameManager.Instance.RemovePause(GameManager.PauseReason.PauseMenu);
        }
            
        // Show/hide pause menu children
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(isPaused);
        }
    }
}
