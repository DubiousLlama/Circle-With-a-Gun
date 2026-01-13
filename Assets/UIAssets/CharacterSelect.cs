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
    public Roster roster;
    public MenuController menuController;

    public GameObject pauseMenuToDisable;
    public GameObject pauseMenuToEnable;
    public GameObject menuCanvas;

    Button selector;

    [Header("Menu GameObjects")]
    public List<GameObject> characterGameObjects;

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


    private void ToggleMystery(GameObject chGo, bool state)
    {
        chGo.transform.Find("Mystery")?.gameObject.SetActive(state);

        chGo.transform.Find("Image")?.gameObject.SetActive(!state);
        chGo.transform.Find("Name")?.gameObject.SetActive(!state);
        chGo.transform.Find("Quest")?.gameObject.SetActive(!state);
        chGo.transform.Find("Unlocked")?.gameObject.SetActive(!state);
    }

    private void updateChars()
    {
        bool MMUnlocked = false;
        try
        {
            if (SteamManager.Initialized)
            {
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
                bool success = SteamUserStats.SetAchievement(questPrefName);
                Debug.Log($"Setting achievement for {ch.name} ({questPrefName}): {success}");
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
            if (buyButton != null)
            {   // check if user owns miss microtransaction DLC
                if (unlocked)
                {
                    buyButton.gameObject.SetActive(false);
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
