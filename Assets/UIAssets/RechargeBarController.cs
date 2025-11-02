using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RechargeBarController : MonoBehaviour
{
    public Slider slider;
    public Image barFlashImage;
    public float flashDuration = 0.1f;

    Color white = new Color(1f, 1f, 1f, 0.5f);
    Color black = new Color(0f, 0f, 0f, 0.5f);
    Color clear = new Color(0f, 0f, 0f, 0f);

    public void SetRecharge(float health)
    {
        slider.value = health;
    }

    public void SetMaxRecharge(float health)
    {
        slider.maxValue = health;
    }

    public void BarFlash(bool goodFlash)
    {
        StartCoroutine(FlashCoroutine(goodFlash ? white : black));
    }

    IEnumerator FlashCoroutine(Color color)
    {
        // Debug.Log("Flash Coroutine started");
        barFlashImage.color = color;
        yield return new WaitForSeconds(flashDuration);
        barFlashImage.color = clear;

    }
}

