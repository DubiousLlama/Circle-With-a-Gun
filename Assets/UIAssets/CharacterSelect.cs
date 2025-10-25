using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;


public class CharacterSelect : MonoBehaviour
{
    public Roster roster;
    public MenuController menuController;

    [Header("Menu GameObjects")]
    public List<GameObject> characterGameObjects;

    // Start is called before the first frame update
    void Start()
    {
        // Ensure the first character is always unlocked
        PlayerPrefs.SetInt(roster.allCharacters[0].prefName + "Unlocked", 1);

        // Ensure MM, Commando, Bombs, and Kevin are revealed from the start
        PlayerPrefs.SetInt(roster.allCharacters[0].prefName + "Hidden", 1);
        PlayerPrefs.SetInt(roster.allCharacters[1].prefName + "Hidden", 1);
        PlayerPrefs.SetInt(roster.allCharacters[2].prefName + "Hidden", 1);
        PlayerPrefs.SetInt(roster.allCharacters[8].prefName + "Hidden", 1);
        updateChars();
    }

    public void ChangeSelection(int selected)
    {
        PlayerPrefs.SetInt("SelectedCharacter", selected);
        menuController.CharacterSelected();
    }


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
        bool MMUnlocked = SteamApps.BIsDlcInstalled((AppId_t)4137050);
        if (MMUnlocked)
        {
            PlayerPrefs.SetInt(roster.allCharacters[8].prefName + "Unlocked", 1);
            PlayerPrefs.SetInt(roster.allCharacters[8].prefName + "Hidden", 1);
        }

        for (int i = 0; i < characterGameObjects.Count; i++)
        {
            Character ch = roster.allCharacters[i];

            revealCriteria();
            bool hidden = PlayerPrefs.GetInt(ch.prefName + "Hidden", 0) == 0;
            ToggleMystery(characterGameObjects[i], hidden);
            if (hidden)
            {
                continue;
            }

            string unlockedPrefName = ch.prefName + "Unlocked";
            string questPrefName = ch.prefName + "Quest";
            bool unlocked = PlayerPrefs.GetInt(unlockedPrefName, 0) == 1;
            int questProgress = PlayerPrefs.GetInt(questPrefName, 0);
            int questMaximum = ch.unlockQuest != null ? ch.unlockQuest.questCompletionThreshold : 1;


            if (ch.unlockQuest != null && ch.unlockQuest.CheckCompletion() && !unlocked)
            {
                unlocked = true;
                PlayerPrefs.SetInt(unlockedPrefName, 1);
            }

            Debug.Log($"Character {ch.name} unlocked: {unlocked}, quest progress: {questProgress}");

            Button selector = characterGameObjects[i].transform.GetChild(0).GetComponent<Button>();
            selector.interactable = unlocked;

            GameObject unlockedText = characterGameObjects[i].transform.GetChild(3).gameObject;
            unlockedText.SetActive(unlocked);

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
                    float realProgress =  (float)questProgress / (float)questMaximum;
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
        bool unlocked = PlayerPrefs.GetInt(roster.allCharacters[keyCharIndex].prefName + "Unlocked", 0) == 1;
        if (unlocked)
        {
            PlayerPrefs.SetInt(roster.allCharacters[charToUnhideIndex].prefName + "Hidden", 1);
        }
        else
        {
            PlayerPrefs.SetInt(roster.allCharacters[charToUnhideIndex].prefName + "Quest", 0);
            PlayerPrefs.SetInt(roster.allCharacters[charToUnhideIndex].prefName + "Hidden", 0);
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
}
