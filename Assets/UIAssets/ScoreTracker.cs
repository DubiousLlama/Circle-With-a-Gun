using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{

    private int score = 0;
    private TextMeshProUGUI textObject;
    private bool gameOver = false;

    public int multiplier = 1;
    public GameObject scoreUI;

    public static event Action<FinalScoreArgs> FinalScore;
    public class FinalScoreArgs : EventArgs
    {
        public int finalScore;
    }

    void Start()
    {
        textObject = scoreUI.GetComponent<TextMeshProUGUI>();

        // Subscribe to the FoeDied event from EnemyHealth
        EnemyHealth.FoeDied += OnFoeDied;
        XPPickup.XPPickedUp += OnXPPickedUP;
        ReturnMainMenu.OnReturnToMainMenu += ReportFinalScore;

        Application.wantsToQuit += quitAttempt;
    }

    bool quitAttempt()
    {
        ReportFinalScore();
        return true;
    }

    void ReportFinalScore()
    {
        FinalScore?.Invoke(new FinalScoreArgs { finalScore = score });
    }

    void OnFoeDied(EnemyHealth.OnDeathEventArgs e)
    {
        IncreaseScore(e.enemy.GetComponent<EnemyHealth>().scoreValue);
    }

    void OnXPPickedUP(XPPickup.OnXPPickupEventArgs e)
    {
        IncreaseScore(e.xpAmount * 5);
    }

    public void IncreaseScore(int increase)
    {
        if (gameOver == false)
        {
            score += increase * multiplier;
        }

        textObject.text = score.ToString();
    }

    public void SetScore(int setTo)
    {
        if (gameOver == false)
        {
            score = setTo;
        }
        textObject.text = score.ToString("G7");

    }

    public void ResetScore()
    {
        if (gameOver == false)
        {
            score = 0;
        }
        
        textObject.text = score.ToString("G7");
    }

    public int GetScore()
    {
        return score;
    }

    public void GameOver()
    {
        gameOver = true;
    }
}
