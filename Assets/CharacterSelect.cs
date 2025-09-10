using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Character
{
    Kevin,
    BombsMcGee,
    Commando,
    ElectricJeff,
    DemoMan,
    Specialist,
    ShockTrooper,
    WackySteve,
    MissMicrotransaction
}

public class CharacterSelect : MonoBehaviour
{
    private Dictionary<Character, string> questPrefs = new()
    {
        { Character.Kevin, "KevinQuest" },
        { Character.BombsMcGee, "BombsMcGeeQuest" },
        { Character.Commando, "CommandoQuest" },
        { Character.ElectricJeff, "ElectricJeffQuest" },
        { Character.DemoMan, "DemoManQuest" },
        { Character.Specialist, "SpecialistQuest" },
        { Character.ShockTrooper, "ShockTrooperQuest" },
        { Character.WackySteve, "WackySteveQuest" },
        { Character.MissMicrotransaction, "MissMicrotransactionQuest" }
    };

    private Dictionary<Character, string> unlockedPrefs = new()
    {
        { Character.Kevin, "KevinUnlocked" },
        { Character.BombsMcGee, "BombsMcGeeUnlocked" },
        { Character.Commando, "CommandoUnlocked" },
        { Character.ElectricJeff, "ElectricJeffUnlocked" },
        { Character.DemoMan, "DemoManUnlocked" },
        { Character.Specialist, "SpecialistUnlocked" },
        { Character.ShockTrooper, "ShockTrooperUnlocked" },
        { Character.WackySteve, "WackySteveUnlocked" },
        { Character.MissMicrotransaction, "MissMicrotransactionUnlocked" }
    };

    public Character selectedCharacter;
    public MenuController menuController;

    [Header("Character GameObjects")]
    public List<GameObject> characterGameObjects;

    // Start is called before the first frame update
    void Start()
    {
        selectedCharacter = (Character)PlayerPrefs.GetInt("SelectedCharacter", 0);
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
            Character ch = (Character)i;

            bool unlocked = PlayerPrefs.GetInt(unlockedPrefs[ch], 0) == 1;
            float questProgress = PlayerPrefs.GetFloat(questPrefs[ch], 0f);

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
