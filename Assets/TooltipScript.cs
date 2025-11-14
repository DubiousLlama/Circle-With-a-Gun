using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TooltipScript : MonoBehaviour
{
    private bool isHoveringDisplay = false;
    private bool isUsingGamepad = false;
    private Vector3 originalPosition;
    private RectTransform tooltipRect;
    private RectTransform displayRect;

    private Vector2 offset;

    void Start()
    {
        // Find the Tooltip child
        Transform tooltipChild = transform.Find("Tooltip");
        if (tooltipChild == null)
        {
            Debug.LogError("TooltipScript: No 'Tooltip' child found on " + gameObject.name);
            enabled = false;
            return;
        }

        tooltipRect = tooltipChild.GetComponent<RectTransform>();
        displayRect = GetComponent<RectTransform>();

        if (tooltipRect == null)
        {
            Debug.LogError("TooltipScript: Tooltip child does not have a RectTransform component");
            enabled = false;
            return;
        }

        offset = new Vector2(displayRect.rect.width + 50f - displayRect.anchoredPosition.x, -displayRect.rect.height / 2 - 20f);

        // Store the original position of the tooltip
        originalPosition = tooltipRect.localPosition;

        // Initially hide the tooltip
        tooltipChild.gameObject.SetActive(false);

        // Disable raycasting on the tooltip and all its children so they don't block pointer events
        DisableRaycastingRecursive(tooltipChild);

        // Add event triggers for mouse hover on the Display element
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        AddEventTrigger(trigger, EventTriggerType.PointerEnter, OnPointerEnter);
        AddEventTrigger(trigger, EventTriggerType.PointerExit, OnPointerExit);
    }

    private void DisableRaycastingRecursive(Transform transform)
    {
        // Disable raycasting on all Graphic components (Image, Text, etc.)
        Graphic[] graphics = transform.GetComponentsInChildren<Graphic>();
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }
    }

    void Update()
    {
        DetectInputType();
        UpdateTooltipVisibility();
    }

    private void DetectInputType()
    {
        // Check if mouse moved recently
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
        {
            isUsingGamepad = false;
        }
        // Check if gamepad input occurred
        else if (Gamepad.current != null)
        {
            bool gamepadActive = (Mathf.Abs(Gamepad.current.leftStick.x.ReadValue()) > 0.1f ||
                                 Mathf.Abs(Gamepad.current.leftStick.y.ReadValue()) > 0.1f ||
                                 Mathf.Abs(Gamepad.current.rightStick.x.ReadValue()) > 0.1f ||
                                 Mathf.Abs(Gamepad.current.rightStick.y.ReadValue()) > 0.1f ||
                                 Gamepad.current.aButton.isPressed ||
                                 Gamepad.current.bButton.isPressed ||
                                 Gamepad.current.xButton.isPressed ||
                                 Gamepad.current.yButton.isPressed ||
                                 Gamepad.current.leftShoulder.isPressed ||
                                 Gamepad.current.rightShoulder.isPressed);

            if (gamepadActive)
            {
                isUsingGamepad = true;
            }
        }
    }

    private void UpdateTooltipVisibility()
    {
        GameObject tooltipChild = transform.Find("Tooltip").gameObject;

        if (isUsingGamepad)
        {
            // Controller mode: Show tooltip only if the parent Unlocked component's character is selected
            HandleControllerTooltip(tooltipChild);
        }
        else
        {
            // Mouse mode: Show tooltip when hovering over this Display element
            HandleMouseTooltip(tooltipChild);
        }
    }

    private void HandleMouseTooltip(GameObject tooltipChild)
    {
        if (isHoveringDisplay)
        {
            tooltipChild.SetActive(true);
            PositionTooltipAtMouse();
        }
        else
        {
            tooltipChild.SetActive(false);
        }
    }

    private void HandleControllerTooltip(GameObject tooltipChild)
    {
        // Check if the parent Unlocked component's character is selected
        // This Display element's parent is Unlocked
        Transform unlockedComponent = transform.parent;
        if (unlockedComponent == null)
        {
            tooltipChild.SetActive(false);
            return;
        }

        // The parent of Unlocked is the character display, and its first child (index 0) is "Select"
        Transform characterDisplay = unlockedComponent.parent;
        if (characterDisplay == null)
        {
            tooltipChild.SetActive(false);
            return;
        }

        Transform selectComponent = characterDisplay.GetChild(0);
        if (selectComponent == null || selectComponent.name != "Select")
        {
            tooltipChild.SetActive(false);
            return;
        }

        // Check if the Select button is currently selected using the EventSystem
        Button selectButton = selectComponent.GetComponent<Button>();
        EventSystem eventSystem = EventSystem.current;
        bool isSelected = selectButton != null && eventSystem != null && eventSystem.currentSelectedGameObject == selectButton.gameObject;

        if (isSelected)
        {
            tooltipChild.SetActive(true);
            // Position at original position
            tooltipRect.localPosition = originalPosition;
        }
        else
        {
            tooltipChild.SetActive(false);
        }
    }

    private void PositionTooltipAtMouse()
    {
        // Convert mouse position to canvas space
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        // Get the canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("TooltipScript: No Canvas found in parents");
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Convert screen position to canvas rect position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mousePos,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        // Offset to bottom-right of mouse
        tooltipRect.anchoredPosition = localPoint + offset;
    }

    private void OnPointerEnter(BaseEventData data)
    {
        isHoveringDisplay = true;
    }

    private void OnPointerExit(BaseEventData data)
    {
        isHoveringDisplay = false;
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }
}
