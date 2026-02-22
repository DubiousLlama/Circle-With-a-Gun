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
    public bool flip = false;

    private Vector2 offset;
    private Transform tooltipChild;
    private Canvas cachedCanvas;
    private RectTransform cachedCanvasRect;

    void Start()
    {
        tooltipChild = transform.Find("Tooltip");
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

        cachedCanvas = GetComponentInParent<Canvas>();
        if (cachedCanvas != null)
        {
            cachedCanvasRect = cachedCanvas.GetComponent<RectTransform>();
        }

        offset = new Vector2(displayRect.rect.width + 50f - displayRect.anchoredPosition.x, -displayRect.rect.height / 2 - 20f);

        if (flip)
        {
            offset = new Vector2(-displayRect.rect.width - 50f - displayRect.anchoredPosition.x, -displayRect.rect.height / 2 - 20f);
        }

        originalPosition = tooltipRect.localPosition;
        tooltipChild.gameObject.SetActive(false);
        DisableRaycastingRecursive(tooltipChild);

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
        GameObject tooltipGO = tooltipChild.gameObject;

        if (isUsingGamepad)
        {
            HandleControllerTooltip(tooltipGO);
        }
        else
        {
            HandleMouseTooltip(tooltipGO);
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
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        if (cachedCanvas == null || cachedCanvasRect == null)
        {
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cachedCanvasRect,
            mousePos,
            cachedCanvas.worldCamera,
            out Vector2 localPoint
        );

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
