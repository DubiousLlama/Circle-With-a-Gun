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

    private bool hasSelectedButton = false;

    private void OnEnable()
    {
        hasSelectedButton = false;
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
    }

    private IEnumerator SelectFirstButtonDelayed()
    {
        yield return null; // Wait one frame
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
