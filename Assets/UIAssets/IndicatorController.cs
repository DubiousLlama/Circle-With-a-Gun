using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorController : MonoBehaviour
{
    // private TextMeshProUGUI multiplier;
    // private Image x2;
    private Image regen;
    private Image speed;
    private Image laser;

    private float[] durationMax = new float[4];
    private float[] duration = new float[4];

    private void Start()
    {
        // multiplier = GameObject.Find("Multiplier").GetComponent<TextMeshProUGUI>();
        // x2 = GameObject.Find("x2Fill").GetComponent<Image>();
        regen = GameObject.Find("RegenFill").GetComponent<Image>();
        speed = GameObject.Find("SpeedFill").GetComponent<Image>();
        laser = GameObject.Find("LaserFill").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < duration.Length; i++)
        {

            // PowerUp Indicies
            // 0: Regen
            // 1: Speed
            // 2: Laser
            // 3: x2

            regen.fillAmount = 1 - duration[0] / durationMax[0];
            speed.fillAmount = 1 - duration[1] / durationMax[1];
            laser.fillAmount = 1 - duration[2] / durationMax[2];
            // x2.fillAmount = 1 - duration[3] / durationMax[3];
        }

        //if (x2.fillAmount <= 0)
        //{
        //    multiplier.text = "x2";
        //}

    }

    public void SetDuration(int index, float value)
    {
        duration[index] = value;

        if (duration[index] > durationMax[index])
        {
            durationMax[index] = value;
        }
    }

    public void SetMultiplier(int mult)
    {
        // multiplier.text = "x" + mult.ToString();
    }

    public void SetDurationMax(int index, float value)
    {
        durationMax[index] = value;
    }
}
