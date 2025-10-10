using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{

    int level = 1;
    int currentXP = 0;
    public int nextLevelXP = 5;

    public GameObject levelUpMenu;

    public Image indicatorFill;
    public TextMeshProUGUI levelText;

    float maxFillAmount = 0.99f;

    public bool levelUpAllowed = true;

    public static event Action<OnLevelUpEventArgs> LevelUp;
    public class OnLevelUpEventArgs : EventArgs {
        public int newLevel;
    }

    // Start is called before the first frame update
    void Start()
    {
        XPPickup.XPPickedUp += OnXPPickedUp;
        OfferLevelUp.LevelUpSelected += () => { levelUpAllowed = true; };
    }

    // Update is called once per frame
    void Update()
    {

        if (indicatorFill.fillAmount >= maxFillAmount - 0.04f & currentXP >= nextLevelXP && levelUpAllowed)
        {
            levelUpAllowed = false;
            level++;
            currentXP -= nextLevelXP;
            nextLevelXP = Mathf.RoundToInt(nextLevelXP * 2.5f);
            AudioManager.instance.PlaySfx("LevelUp", 1f);
            Invoke("TriggerLevelUpEvent", 0.01f);
        }

        float lerpSpeed = GetTargetFillAmount() < indicatorFill.fillAmount ? 10f : 5f;
        indicatorFill.fillAmount = Mathf.Lerp(indicatorFill.fillAmount, GetTargetFillAmount(), Time.deltaTime * lerpSpeed);
    }

    void OnXPPickedUp(XPPickup.OnXPPickupEventArgs args)
    {
        currentXP += args.xpAmount;
    }

    float GetTargetFillAmount()
    {
        return Mathf.Clamp((float)currentXP / nextLevelXP, 0.025f, maxFillAmount);
    }

    void TriggerLevelUpEvent()
    {
        Debug.Log($"Leveled up to {level}! Next level at {nextLevelXP} XP.");
        levelUpMenu.SetActive(true);
        levelText.text = level.ToString();
        LevelUp?.Invoke(new OnLevelUpEventArgs { newLevel = level });
    }
}
