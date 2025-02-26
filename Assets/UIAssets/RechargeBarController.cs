using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RechargeBarController : MonoBehaviour
{
    public Slider slider;

    public void SetRecharge(float health)
    {
        slider.value = health;
    }

    public void SetMaxRecharge(float health)
    {
        slider.maxValue = health;
    }
}

