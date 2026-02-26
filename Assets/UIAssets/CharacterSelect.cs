using Pathfinding;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterSelect : MonoBehaviour
{
    /// <summary>Indices always revealed on the character select screen: Kevin (0), Bombs (1), Commando (2), Miss Microtransaction (8).</summary>
    private static readonly int[] AlwaysVisibleCharacterIndices = { 0, 1, 2, 8 };

    public Roster roster;
    public MenuController menuController;

    public GameObject pauseMenuToDisable;
    public GameObject pauseMenuToEnable;
    public GameObject menuCanvas;

    Button selector;

    [Header("Menu GameObjects")]
    public List<GameObject> characterGameObjects;

    [Header("Editor shortcuts (F1/F2/F3)")]
    [Tooltip("Character index to lock and unhide when F1 is pressed (0=Kevin, 1=Bombs, 2=Commando, 3=Electric Jeff, 4=Demo Man, 5=Specialist, 6=Shock Trooper, 7=Wacky Steve, 8=Miss Microtransaction).")]
    public int editorF1CharacterIndex = 0;

    // Start is called before the first frame update
    void OnEnable()
    {
        pauseMenuToDisable.SetActive(false);
        pauseMenuToEnable.SetActive(true);

        // Ensure the first character is always unlocked
        SaveManager.instance.SetInt(roster.allCharacters[0].prefName + "Unlocked", 1);

        // Ensure MM, Commando, Bombs, and Kevin are revealed from the start
        SaveManager.instance.SetInt(roster.allCharacters[0].prefName + "Hidden", 1);
        SaveManager.instance.SetInt(roster.allCharacters[1].prefName + "Hidden", 1);
        SaveManager.instance.SetInt(roster.allCharacters[2].prefName + "Hidden", 1);
        SaveManager.instance.SetInt(roster.allCharacters[8].prefName + "Hidden", 1);
        updateChars();
    }

    public void ChangeSelection(int selected)
    {
        PlayerPrefs.SetInt("SelectedCharacter", selected);
        menuController.CharacterSelected();
    }

    public void BackToMenu()
    {
        pauseMenuToDisable.SetActive(true);
        pauseMenuToEnable.SetActive(false);
        menuCanvas.SetActive(true);
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isEditor || roster == null || roster.allCharacters == null)
            return;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            EditorLockAndUnhideCharacter(editorF1CharacterIndex);
            return;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            EditorUnlockAndRevealAll();
            return;
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            EditorLockAndHideExceptAlwaysVisible();
            return;
        }
    }

    /// <summary>Editor-only: F1 — Lock and unhide (reveal) the character at the specified index. Only active on character select screen.</summary>
    public void EditorLockAndUnhideCharacter(int characterIndex)
    {
        if (roster == null || roster.allCharacters == null || characterIndex < 0 || characterIndex >= roster.allCharacters.Count)
        {
            int max = (roster != null && roster.allCharacters != null) ? roster.allCharacters.Count - 1 : 0;
            Debug.LogWarning($"[CharacterSelect] F1: Invalid character index {characterIndex}. Use 0–{max}.");
            return;
        }
        Character ch = roster.allCharacters[characterIndex];
        SaveManager.instance.SetInt(ch.prefName + "Unlocked", 0);
        SaveManager.instance.SetInt(ch.prefName + "Hidden", 1);
        updateChars();
        Debug.Log($"[CharacterSelect] F1: {ch.name} (index {characterIndex}) locked and unhidden (editor only).");
    }

    /// <summary>Editor-only: F3 — Unlock and reveal all characters. Only active on character select screen.</summary>
    public void EditorUnlockAndRevealAll()
    {
        for (int i = 0; i < roster.allCharacters.Count; i++)
        {
            Character ch = roster.allCharacters[i];
            SaveManager.instance.SetInt(ch.prefName + "Unlocked", 1);
            SaveManager.instance.SetInt(ch.prefName + "Hidden", 1);
        }
        updateChars();
        Debug.Log("[CharacterSelect] F3: All characters unlocked and revealed (editor only).");
    }

    /// <summary>Editor-only: F2 — Lock and hide all characters except those always unlocked/visible (Kevin, Bombs, Commando, Miss Microtransaction).</summary>
    public void EditorLockAndHideExceptAlwaysVisible()
    {
        bool MMUnlocked = false;
        try
        {
            if (SteamManager.Initialized)
                MMUnlocked = SteamApps.BIsDlcInstalled((AppId_t)4137050);
        }
        catch { /* ignore */ }

        for (int i = 0; i < roster.allCharacters.Count; i++)
        {
            Character ch = roster.allCharacters[i];
            bool alwaysVisible = System.Array.IndexOf(AlwaysVisibleCharacterIndices, i) >= 0;
            if (alwaysVisible)
            {
                SaveManager.instance.SetInt(ch.prefName + "Hidden", 1);
                if (i == 0)
                    SaveManager.instance.SetInt(ch.prefName + "Unlocked", 1);
                else if (i == 8)
                    SaveManager.instance.SetInt(ch.prefName + "Unlocked", MMUnlocked ? 1 : 0);
                else
                    SaveManager.instance.SetInt(ch.prefName + "Unlocked", 0);
            }
            else
            {
                SaveManager.instance.SetInt(ch.prefName + "Unlocked", 0);
                SaveManager.instance.SetInt(ch.prefName + "Hidden", 0);
            }
        }
        updateChars();
        Debug.Log("[CharacterSelect] F2: All characters locked/hidden except always-visible (editor only).");
    }
#endif

    private void ToggleMystery(GameObject chGo, bool state)
    {
        chGo.transform.Find("Mystery")?.gameObject.SetActive(state);
        chGo.transform.Find("ComingSoon")?.gameObject.SetActive(state);

        chGo.transform.Find("Image")?.gameObject.SetActive(!state);
        chGo.transform.Find("Name")?.gameObject.SetActive(!state);
        chGo.transform.Find("Quest")?.gameObject.SetActive(!state);
        chGo.transform.Find("Unlocked")?.gameObject.SetActive(!state);
    }

    private void updateChars()
    {
        bool steamInitialized = false;
        bool MMUnlocked = false;
        try
        {
            if (SteamManager.Initialized)
            {
                steamInitialized = true;
                MMUnlocked = SteamApps.BIsDlcInstalled((AppId_t)4137050);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Steamworks not available: {e.Message}");
        }

        if (MMUnlocked)
        {
            SaveManager.instance.SetInt(roster.allCharacters[8].prefName + "Unlocked", 1);
            SaveManager.instance.SetInt(roster.allCharacters[8].prefName + "Hidden", 1);
        }
        else
        {
            SaveManager.instance.SetInt(roster.allCharacters[8].prefName + "Unlocked", 0);
        }

        for (int i = 0; i < characterGameObjects.Count; i++)
        {
            Character ch = roster.allCharacters[i];

            revealCriteria();
            bool hidden = SaveManager.instance.GetInt(ch.prefName + "Hidden", 0) == 0;
            ToggleMystery(characterGameObjects[i], hidden);
            if (hidden)
            {
                Debug.Log($"Character {ch.name} is hidden.");
                selector = characterGameObjects[i].transform.Find("Select").GetComponent<Button>();
                selector.interactable = !hidden;
                continue;
            }

            string unlockedPrefName = ch.prefName + "Unlocked";
            string questPrefName = ch.prefName + "Quest";
            bool unlocked = SaveManager.instance.GetInt(unlockedPrefName, 0) == 1;

            if (unlocked)
            {
                try
                {
                    if (SteamManager.Initialized)
                    {
                        bool success = SteamUserStats.SetAchievement(questPrefName);
                        Debug.Log($"Setting achievement for {ch.name} ({questPrefName}): {success}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Steam achievement set failed for {questPrefName}: {e.Message}");
                }
            }
            int questProgress = SaveManager.instance.GetInt(questPrefName, 0);
            int questMaximum = ch.unlockQuest != null ? ch.unlockQuest.questCompletionThreshold : 1;


            if (ch.unlockQuest != null && ch.unlockQuest.CheckCompletion() && !unlocked)
            {
                unlocked = true;
                SaveManager.instance.SetInt(unlockedPrefName, 1);
            }

            Debug.Log($"Character {ch.name} unlocked: {unlocked}, quest progress: {questProgress}");

            selector = characterGameObjects[i].transform.Find("Select").GetComponent<Button>();
            Debug.Log($"Found selector {selector.name} for character {ch.name} that is unlocked: {unlocked} with parent {selector.gameObject.transform.parent.name}");
            selector.interactable = unlocked;
            Debug.Log($"Set selector for {ch.name} to {selector.interactable}");

            Transform unlockedTransform = characterGameObjects[i].transform.Find("Unlocked");
            if (unlockedTransform != null)
            {
                unlockedTransform.gameObject.SetActive(unlocked);
            }

            Transform quest = characterGameObjects[i].transform.Find("Quest");
            if (quest != null)
            {
                if (unlocked)
                {
                    quest.gameObject.SetActive(false);
                }
                else
                {
                    quest.gameObject.SetActive(true);
                    Slider questSlider = quest.transform.GetChild(0).GetComponent<Slider>();
                    float realProgress = (float)questProgress / (float)questMaximum;
                    float minProgress = 0f;
                    float maxProgress = 0.95f;
                    if (realProgress > 0)
                    {
                        minProgress = 0.05f;
                    }
                    realProgress = Mathf.Clamp(realProgress, minProgress, maxProgress);
                    questSlider.value = realProgress;

                }
            }

            Transform buyButton = characterGameObjects[i].transform.Find("BuyButton");
            Transform errorChild = characterGameObjects[i].transform.Find("Error");
            if (errorChild != null) {
                errorChild.gameObject.SetActive(false);
            }

            if (buyButton != null)
            {
                // Miss Microtransaction (index 8): show Buy when unlocked via DLC, or Error when Steam not initialized
                if (unlocked)
                {
                    buyButton.gameObject.SetActive(false);
                }
                else if (i == 8 && !steamInitialized)
                {
                    buyButton.gameObject.SetActive(false);
                    if (errorChild != null) {

                        errorChild.gameObject.SetActive(true);
                    }
                }
                else
                {
                    buyButton.gameObject.SetActive(true);
                }
            }
        }
    }

    public void unhideIfUnlocked(int keyCharIndex, int charToUnhideIndex)
    {
        bool unlocked = SaveManager.instance.GetInt(roster.allCharacters[keyCharIndex].prefName + "Unlocked", 0) == 1;
        if (unlocked)
        {
            SaveManager.instance.SetInt(roster.allCharacters[charToUnhideIndex].prefName + "Hidden", 1);
        }
        else
        {
            SaveManager.instance.SetInt(roster.allCharacters[charToUnhideIndex].prefName + "Quest", 0);
            SaveManager.instance.SetInt(roster.allCharacters[charToUnhideIndex].prefName + "Hidden", 0);
        }
    }

    public void revealCriteria()
    {
        // Electric Jeff
        // Reveal when Commando is completed
        unhideIfUnlocked(2, 3);

        // Demo Man
        // Reveal when Bombs McGee is completed
        unhideIfUnlocked(1, 4);

        // Specialist
        // Reveal when Commando is completed
        unhideIfUnlocked(2, 5);

        // Shock Trooper
        // Reveal when Specialist is completed
        unhideIfUnlocked(5, 6);

        // Wacky Steve
        // Reveal when Electric Jeff is completed
        unhideIfUnlocked(3, 7);
    }

    public void PurchaseMissMicrotransactionDLC()
    {
        try
        {
            if (SteamManager.Initialized)
            {
                SteamFriends.ActivateGameOverlayToStore((AppId_t)4137050, EOverlayToStoreFlag.k_EOverlayToStoreFlag_AddToCartAndShow);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Steamworks not available: {e.Message}");
        }
    }
}
