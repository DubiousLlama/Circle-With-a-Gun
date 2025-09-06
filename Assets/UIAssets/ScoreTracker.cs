using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreTracker : MonoBehaviour
{

    private int score = 0;
    private TextMeshProUGUI textObject;
    private bool gameOver = false;

    public int multiplier = 1;

    void Start()
    {
        textObject = GetComponent<TextMeshProUGUI>();
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
