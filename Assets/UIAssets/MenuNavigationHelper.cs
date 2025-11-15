using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNavigationHelper : MonoBehaviour
{
    [Header("First Selected Button")]
    [Tooltip("The button to select when this menu opens")]
    public Button firstSelectedButton;

    [Header("Auto-Find First Button")]
    [Tooltip("If no button is assigned, automatically find the first active button")]
    public bool autoFindFirstButton = true;

    [Header("Watch For Child Activation")]
    [Tooltip("Check every frame if buttons become active (useful for parent objects)")]
    public bool watchForActivation = false;

    [Header("Recover From Mouse Input")]
    [Tooltip("Re-select button when gamepad input is detected after mouse use")]
    public bool recoverFromMouseInput = true;

    private bool hasSelectedButton = false;
    private bool wasMouseUsed = false;
    private bool gamepadInputDetected = false;

    private void OnEnable()
    {
        hasSelectedButton = false;
        wasMouseUsed = false;
        // Wait a frame for the menu to fully initialize
        StartCoroutine(SelectFirstButtonDelayed());
    }

    private void Update()
    {
        // If watching for activation, keep trying to select button when one becomes available
        if (watchForActivation && !hasSelectedButton)
        {
            TrySelectButton();
        }

        // Check if we should recover from mouse input
        if (recoverFromMouseInput)
        {
            HandleInputModeSwitch();
        }

        // Check if any gamepad/joystick input is detected
        bool gamepadInputDetected = false;

        // Check analog sticks
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            gamepadInputDetected = true;
        }

        // Check gamepad buttons
        if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
        {
            gamepadInputDetected = true;
        }

        // If gamepad input is detected and nothing is currently selected, reselect button
        if (gamepadInputDetected && EventSystem.current != null)
        {
            if (EventSystem.current.currentSelectedGameObject == null || EventSystem.current.currentSelectedGameObject.activeInHierarchy == false)
            {
                TrySelectButton();
                wasMouseUsed = false;
            }
        }
    }

    private void HandleInputModeSwitch()
    {
        // Check mouse input
        bool mouseInputDetected = Input.GetMouseButton(0) || Input.GetMouseButton(1) || 
                                   Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0;

        // Track if mouse is being used
        if (mouseInputDetected)
        {
            wasMouseUsed = true;

            // Deselect any selected button when mouse is used
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }

    private IEnumerator SelectFirstButtonDelayed()
    {
        yield return null;
        yield return null;
        TrySelectButton();
    }

    private void TrySelectButton()
    {
        Button buttonToSelect = firstSelectedButton;

        // Auto-find first button if none assigned
        if (buttonToSelect == null && autoFindFirstButton)
        {
            buttonToSelect = GetFirstActiveButton();
            if (buttonToSelect != null)
            {
                Debug.Log($"MenuNavigationHelper: Found button to select: {buttonToSelect.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"MenuNavigationHelper on {gameObject.name}: No active button found!");
            }
        }

        // Select the button for gamepad navigation
        if (buttonToSelect != null && buttonToSelect.gameObject.activeInHierarchy)
        {
            if (EventSystem.current == null)
            {
                Debug.LogError("MenuNavigationHelper: No EventSystem found in scene!");
                return;
            }

            buttonToSelect.Select();
            EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
            hasSelectedButton = true;
            Debug.Log($"MenuNavigationHelper: Successfully selected button: {buttonToSelect.gameObject.name}");
        }
    }

    private Button GetFirstActiveButton()
    {
        // Find all buttons in this menu (including inactive parent objects)
        Button[] buttons = GetComponentsInChildren<Button>(true);
        
        Debug.Log($"MenuNavigationHelper on {gameObject.name}: Found {buttons.Length} total buttons");
        
        foreach (Button button in buttons)
        {
            Debug.Log($"  Button: {button.gameObject.name}, Active: {button.gameObject.activeInHierarchy}, Interactable: {button.interactable}");
            if (button.gameObject.activeInHierarchy && button.interactable)
            {
                return button;
            }
        }

        return null;
    }
}
