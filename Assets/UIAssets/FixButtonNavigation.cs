using UnityEngine;
using UnityEngine.UI;

public class FixButtonNavigation : MonoBehaviour
{
    [Header("Auto-Setup Navigation")]
    [Tooltip("Automatically configure navigation for all child buttons")]
    public bool setupOnStart = true;

    [Header("Navigation Mode")]
    [Tooltip("Automatic = Unity decides, Explicit = Manual setup, None = No navigation")]
    public Navigation.Mode navigationMode = Navigation.Mode.Automatic;

    [Header("Force All Interactable")]
    [Tooltip("Make all buttons interactable, even if marked as locked")]
    public bool forceAllInteractable = false;

    void Start()
    {
        if (setupOnStart)
        {
            SetupButtonNavigation();
        }
    }

    [ContextMenu("Setup Button Navigation")]
    public void SetupButtonNavigation()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        
        Debug.Log($"FixButtonNavigation: Found {buttons.Length} buttons in {gameObject.name}");

        int interactableCount = 0;
        int nonInteractableCount = 0;

        foreach (Button button in buttons)
        {
            // Force interactable if option is enabled
            if (forceAllInteractable && !button.interactable)
            {
                button.interactable = true;
                Debug.Log($"  FORCED Interactable: {button.gameObject.name}");
            }

            Navigation nav = button.navigation;
            nav.mode = navigationMode;
            button.navigation = nav;

            // Also ensure the button has proper highlight colors
            ColorBlock colors = button.colors;
            if (colors.highlightedColor.a < 0.1f)
            {
                // Set a visible highlight color if it's too transparent
                colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
                button.colors = colors;
            }

            string status = button.interactable ? "✓ Interactable" : "✗ NOT Interactable";
            Debug.Log($"  {status} - {button.gameObject.name} (Parent: {button.transform.parent.name})");
            
            if (button.interactable)
                interactableCount++;
            else
                nonInteractableCount++;
        }

        Debug.Log($"FixButtonNavigation: Complete! {interactableCount} interactable, {nonInteractableCount} not interactable");
    }
}
