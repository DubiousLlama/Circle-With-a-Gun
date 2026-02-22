using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuNavigationHelper : MonoBehaviour
{
    // The most recently enabled helper "owns" selection so overlapping menus don't fight.
    private static readonly List<MenuNavigationHelper> activeStack = new List<MenuNavigationHelper>();
    private bool IsTopmost => activeStack.Count > 0 && activeStack[activeStack.Count - 1] == this;

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
    private bool mouseInUse = false;

    private void OnEnable()
    {
        hasSelectedButton = false;
        activeStack.Remove(this);
        activeStack.Add(this);
        StartCoroutine(SelectFirstButtonDelayed());
    }

    private void OnDisable()
    {
        activeStack.Remove(this);
    }

    private void Update()
    {
        MenuInputMode.Tick();

        if (!IsTopmost) return;

        if (watchForActivation && !hasSelectedButton && MenuInputMode.ShouldSelectForController)
        {
            TrySelectButton();
        }

        if (recoverFromMouseInput)
        {
            HandleInputModeSwitch();
        }

        if (MenuInputMode.ShouldSelectForController && EventSystem.current != null)
        {
            mouseInUse = false;
            if (EventSystem.current.currentSelectedGameObject == null ||
                !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
            {
                TrySelectButton();
            }
        }
    }

    private void HandleInputModeSwitch()
    {
        if (MenuInputMode.MouseActiveThisFrame && !mouseInUse)
        {
            mouseInUse = true;
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

        if (!IsTopmost || !MenuInputMode.ShouldSelectForController)
            yield break;

        TrySelectButton();
    }

    private void TrySelectButton()
    {
        Button buttonToSelect = firstSelectedButton;

        if (buttonToSelect == null && autoFindFirstButton)
        {
            buttonToSelect = GetFirstActiveButton();
        }

        if (buttonToSelect != null && buttonToSelect.gameObject.activeInHierarchy)
        {
            if (EventSystem.current == null)
                return;

            buttonToSelect.Select();
            EventSystem.current.SetSelectedGameObject(buttonToSelect.gameObject);
            hasSelectedButton = true;
        }
    }

    private Button GetFirstActiveButton()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button.gameObject.activeInHierarchy && button.interactable)
                return button;
        }
        return null;
    }
}
