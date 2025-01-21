using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CounterButton : MonoBehaviour
{
    [Tooltip("The amount available to the player")]
    public int amount = 10;
    public TMP_Text text;
    public Image fill;

    private float amountLeft;

    public void Start()
    {
        amountLeft = amount;
        text.text = amountLeft.ToString("F0");
    }

    public void OnClick()
    {
        if (amountLeft <= 0)
        {
            return;
        }
        amountLeft -= 1;
        text.text = amountLeft.ToString("F0");
        fill.fillAmount = amountLeft / amount;
    }
}
