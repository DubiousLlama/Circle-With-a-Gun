using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuReveal : MonoBehaviour
{
    // When the escape key is pressed, reveal the pause menu (set all children active)
    // If the pause menu is already active, hide it (set all children inactive)
    // Don't constantly toggle it while the key is held down

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

    private void OnEnable()
    {
        isPaused = false;
        Update();
    }

    private void OnDisable()
    {
        isPaused = false;
        Update();
    }

    public void OnPausePressed()
    {
        isPaused = !isPaused;
    }

    void Update()
    {
        // Toggle pause state
        if (isPaused)
        {
            if (GameManager.Instance != null) { GameManager.Instance.RequestPause(GameManager.PauseReason.PauseMenu); }
            EventSystem.current.sendNavigationEvents = true;
        } 
        else if (GameManager.Instance != null)
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
