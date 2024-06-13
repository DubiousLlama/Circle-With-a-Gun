using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    private Image bomb;

    private float[] durationMax = new float[4];
    private float[] duration = new float[4];

    private TextMeshProUGUI healthtext;
    private TextMeshProUGUI speedtext;
    private TextMeshProUGUI lasertext;
    private TextMeshProUGUI bombtext;

    private PlayerStats playerStats;

    private Color powerUpGreen = new Color(0.054901960784313725f, 0.7686274509803922f, 0);

    private void Start()
    {
        regen = GameObject.Find("RegenFill").GetComponent<Image>();
        speed = GameObject.Find("SpeedFill").GetComponent<Image>();
        laser = GameObject.Find("LaserFill").GetComponent<Image>();
        bomb = GameObject.Find("BombFill").GetComponent<Image>();

        healthtext = GameObject.Find("BonusHealthText").GetComponent<TextMeshProUGUI>();
        speedtext = GameObject.Find("BonusSpeedText").GetComponent<TextMeshProUGUI>();
        lasertext = GameObject.Find("BonusDamageText").GetComponent<TextMeshProUGUI>();
        bombtext = GameObject.Find("ExplosionPowerText").GetComponent<TextMeshProUGUI>();

        playerStats = GameObject.Find("PC").GetComponent<PlayerStats>();
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
            // 3: bomb

            regen.fillAmount = 1 - duration[0] / durationMax[0];
            speed.fillAmount = 1 - duration[1] / durationMax[1];
            laser.fillAmount = 1 - duration[2] / durationMax[2];
            bomb.fillAmount = 1 - duration[3] / durationMax[3];
        }

        healthtext.text = "Chonk: " + Mathf.RoundToInt(playerStats.MaxHealth()*100) + "%";
        speedtext.text = "Zoom: " + Mathf.RoundToInt(playerStats.Speed()*100) + "%";
        lasertext.text = "Bang: " + Mathf.RoundToInt(playerStats.Damage()*100) + "%";
        bombtext.text = "Boom: " + Mathf.RoundToInt(playerStats.ExplosionDamage()*100) + "%";

        
        UpdateColors();

    }

    public void SetDuration(int index, float value)
    {
        duration[index] = value;

        if (duration[index] > durationMax[index])
        {
            durationMax[index] = value;
        }
    }

    public void SetDurationMax(int index, float value)
    {
        durationMax[index] = value;
    }

    public void UpdateColors()
    {
        if (playerStats.regenMod != 1f)
        {
            healthtext.color = powerUpGreen;
        }
        else
        {
            healthtext.color = Color.black;
        }
        if (playerStats.speedMod != 1f)
        {
            speedtext.color = powerUpGreen;
        }
        else
        {
            speedtext.color = Color.black;
        }
        if (playerStats.damageMod != 1f)
        {
            lasertext.color = powerUpGreen;
        }
        else
        {
            lasertext.color = Color.black;
        }
        if (playerStats.explosionDamageMod != 1f)
        {
            bombtext.color = powerUpGreen;
        }
        else
        {
            bombtext.color = Color.black;
        }
    }
}
