using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterSelect : MonoBehaviour
{
    public Roster roster;
    public MenuController menuController;

    public Character selectedCharacter;

    [Header("Menu GameObjects")]
    public List<GameObject> characterGameObjects;

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetInt(roster.allCharacters[0].prefName + "Unlocked", 1); // Ensure the first character is always unlocked
        selectedCharacter = roster.allCharacters.FirstOrDefault(c => c.id == PlayerPrefs.GetInt("SelectedCharacter", 0));
        updateChars();
    }

    public void ChangeSelection(int selected)
    {
        PlayerPrefs.SetInt("SelectedCharacter", selected);
        menuController.CharacterSelected();
    }

    private void updateChars()
    {
        for (int i = 0; i < characterGameObjects.Count; i++)
        {
            Character ch = roster.allCharacters[i];
            
            string unlockedPrefName = ch.prefName + "Unlocked";
            string questPrefName = ch.prefName + "Quest";
            bool unlocked = PlayerPrefs.GetInt(unlockedPrefName, 0) == 1;
            float questProgress = PlayerPrefs.GetFloat(questPrefName, 0f);

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
                    questSlider.value = questProgress;

                }
            }

            Transform buyButton = characterGameObjects[i].transform.Find("BuyButton");
            if (buyButton != null)
            {
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
}
