using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Roster", menuName = "ScriptableObjects/Roster")]
public class Roster : ScriptableObject
{
    public static Character kevin;
    public static Character bombsMcGee;
    public static Character commando;
    public static Character electricJeff;
    public static Character demoMan;
    public static Character specialist;
    public static Character shockTrooper;
    public static Character wackySteve;
    public static Character missMicrotransaction;

    public List<Character> allCharacters = new List<Character>()
    {
        kevin,
        bombsMcGee,
        commando,
        electricJeff,
        demoMan,
        specialist,
        shockTrooper,
        wackySteve,
        missMicrotransaction
    };
}
