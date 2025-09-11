using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WeaponsManager))]
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterManager : MonoBehaviour
{
    public Roster roster;

    Character selectedCharacter;

    void Start()
    {
        selectedCharacter = roster.allCharacters[PlayerPrefs.GetInt("SelectedCharacter", 0)];

        GetComponent<SpriteRenderer>().sprite = selectedCharacter.sprite;
        GetComponent<WeaponsManager>().EquipWeapon(Instantiate(selectedCharacter.primaryWeapon));
        GetComponent<WeaponsManager>().EquipWeapon(Instantiate(selectedCharacter.secondaryWeapon));

        PlayerPrefs.SetInt("WackySteveUnlocked", 1); // For testing purposes, unlock Wacky Steve
    }
}