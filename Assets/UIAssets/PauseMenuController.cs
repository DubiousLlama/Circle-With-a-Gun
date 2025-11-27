using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenuController : MonoBehaviour
{
    public ScoreTracker scoreTracker;
    public GameObject musicSlider;
    public GameObject sfxSlider;
    public MenuMusic menuMusic;

    void Start()
    {
        menuMusic = FindObjectOfType<MenuMusic>();
        LoadAndApplyVolumes();
    }

    private void LoadAndApplyVolumes()
    {
        // Load saved music volume
        float musicVol = PlayerPrefs.GetFloat("musicVol", 1f);
        musicSlider.GetComponent<UnityEngine.UI.Slider>().value = musicVol;
        
        
        // Load saved SFX volume
        float sfxVol = PlayerPrefs.GetFloat("sfxVol", 1f);
        sfxSlider.GetComponent<UnityEngine.UI.Slider>().value = sfxVol;
    }

    public void AbandonRun()
    {
        // If we are not in "GunTime", return
        if (SceneManager.GetActiveScene().name != "GunTime")
        {
            return;
        }

        scoreTracker.GameOver();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OnMusicVolumeChange()
    {
        float volume = musicSlider.GetComponent<UnityEngine.UI.Slider>().value;

        PlayerPrefs.SetFloat("musicVol", volume);
        if (volume == 0f)
        {
            bool success = SteamUserStats.SetAchievement("SamQuest");
            Debug.Log($"Achievement SamQuest set: {success}");
        }

        VolumeFixer.instance.SetMusicVolume(volume);
    }

    public void OnSFXVolumeChange()
    {
        float volume = sfxSlider.GetComponent<UnityEngine.UI.Slider>().value;
        PlayerPrefs.SetFloat("sfxVol", volume);
        VolumeFixer.instance.SetSFXVolume(volume);
    }

    //public static float volTransform(float x)
    //{
    //    if (x <= 0f) { return 0f; }
    //    // Debug.Log($"Volume transform input: {x}");
    //    float A = -5f;
    //    float b = 1f / (1 - Mathf.Exp(-A));
    //    float _ans = ((-1 * Mathf.Exp(-A * x)) + 1) * b;
    //    // Debug.Log($"Volume output: {_ans}");
    //    return _ans;
        
    //}
}
