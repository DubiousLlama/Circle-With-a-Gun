using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Events;

public class QuestTracker : MonoBehaviour
{

    public Quest bombsMcGee;
    public Quest commando;
    public Quest electricJeff;
    public Quest demoMan;
    public Quest specialist;
    public Quest shockTrooper;
    public Quest wackySteve;

    private bool otherWeaponUsed = false;

    // Start is called before the first frame update
    void Awake()
    {
        // Bombs McGee
        // Get score at end of level and set progress
        ScoreTracker.FinalScore += (e) =>
        {
            bombsMcGee.SetProgress(e.finalScore);
            Debug.Log($"Bombs Final score: {e.finalScore}");
        };

        // Commando and Electric Jeff
        // Increase progress when Trapezoid/Triangle is killed
        EnemyHealth.FoeDied += (e) =>
        {
            if (e.enemy.GetComponent<TrapzController>() != null)
            {
                commando.IncrementProgress();
                Debug.Log($"Trapz Killed");
            }
            else if (e.enemy.GetComponent<TriangleController>() != null)
            {
                electricJeff.IncrementProgress();
                Debug.Log($"Triangle Killed");
                Debug.Log($"Setting electricJeff progress: {electricJeff.GetProgress()}");
                Debug.Log($"electricJeff isActive: {electricJeff.isActive}");
            }
        };

        // Demo Man
        // Set progress when foes are killed by a bomb
        BombLogic.OnBombSecondaryExplosion += (e) =>
        {
            demoMan.SetProgress(e.foesKilled);
        };

        // Specialist
        // Set score at end of level if character is Commando
        ScoreTracker.FinalScore += (e) =>
        {
            if(PlayerPrefs.GetInt("SelectedCharacter", 0) == 2)
            {
                specialist.SetProgress(e.finalScore);
            }
        };

        // Shock Trooper
        // Set score at end of level if character is Specialist
        ScoreTracker.FinalScore += (e) =>
        {
            if (PlayerPrefs.GetInt("SelectedCharacter", 0) == 5)
            {
                specialist.SetProgress(e.finalScore);
            }
        };

        // Wacky Steve
        // Track if any weapon other than Lightning Strike is used. If only Lightning Strike is used, set progress to score at end of level.
        ScoreTracker.FinalScore += (e) =>
        {
            if (!otherWeaponUsed)
            {
                wackySteve.SetProgress(e.finalScore);
            }
        };
    }

    private void Start()
    {
        Weapon.OnWeaponUsed += (e) =>
        {
            if (e.weaponName != "Lightning Strike")
            {
                otherWeaponUsed = true;
            }
        };
    }
}