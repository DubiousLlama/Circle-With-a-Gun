using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TipOfTheDay : MonoBehaviour
{
    TextMeshProUGUI tipTextObject;

    List<string> tips = new List<string>()
    {
        "Press Esc (Keyboard) or Start/Menu (Controller) to pause the game.",
        "Firing your secondary while moving kills your momentum. It's better to stop on your own.",
        "Circles are tough. You're immune to the effects of your own weapons and power-ups.",
        "The blue circles are power-ups that give you temporary boosts!",
        "The power-up that looks like a broken enemy unleashes a giant blast to clear the screen.",
        "The blue orbs are XP. Pick them up to earn powerful upgrades.",
        "XP, power-ups, and weapons don't expire. You can pick them up whenever!",
        "The upgrades you've already picked affect what new upgrades you can get.",
        "The Sniper deals more damage the farther away you are from your target.",
        "The Teleporter tries to move you as far as possible without putting you next to an enemy.",
        "The Weather Vane doesn't do anything if there are no enemies nearby.",
        "Pink circles are Legendary weapons. They're powerful, but don't last long!",
        "Always orient the handle of a pot away from the edge of the stovetop to prevent tipping.",
        "Triangles need distance to charge you. If you get close, they'll back off.",
        "Octos are durable and worth lots of points. Their blue clouds slow you down and damage you.",
        "Trapz shoot slow red projectiles. Don't let too many build up, unless you like dodging.",
        "The 'Close Door' button in most U.S. elevators does nothing. People just love the illusion of control.",
    };

    // Start is called before the first frame update
    void Start()
    {
        int tipIndex = SaveManager.instance.GetInt("TipIndex", 0);
        tipTextObject = GetComponent<TextMeshProUGUI>();
        tipTextObject.text = "Tip: " + tips[tipIndex];
        tipIndex = (tipIndex + 1) % tips.Count;
        SaveManager.instance.SetInt("TipIndex", tipIndex);
    }
}
