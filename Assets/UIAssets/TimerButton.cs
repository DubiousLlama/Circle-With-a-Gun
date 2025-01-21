using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerButton : MonoBehaviour
{
    [Tooltip("The duration of the timer in seconds")]
    public float duration = 10.0f;
    public TMP_Text text;
    public Image fill;
    public Button button;

    private float timeLeft;
    private bool isRunning = false;

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            timeLeft -= Time.deltaTime;
            text.text = timeLeft.ToString("F0");
            fill.fillAmount = timeLeft / duration;
            if (timeLeft <= 0)
            {
                isRunning = false;
                button.interactable = true;
                text.text = "0";
                fill.fillAmount = 0.0f;
            }
        }
    }

    public void OnClick()
    {
        if (isRunning)
        {
            return;
        }
        timeLeft = duration;
        isRunning = true;
        button.interactable = false;
    }
}
