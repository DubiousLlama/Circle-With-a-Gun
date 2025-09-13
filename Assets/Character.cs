using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character")]
public class Character : ScriptableObject
{
    public new string name;
    public string prefName;
    public int id;
    public Sprite sprite;
    public GameObject primaryWeapon;
    public GameObject secondaryWeapon;

    public Quest unlockQuest = null;
}