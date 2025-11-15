using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    public ScoreTracker scoreTracker;
    public GameObject musicSlider;
    public GameObject sfxSlider;
    public MenuMusic menuMusic;

    void Start()
    {
        LoadAndApplyVolumes();
        menuMusic = FindObjectOfType<MenuMusic>();
    }

    private void LoadAndApplyVolumes()
    {
        // Load saved music volume
        float musicVol = PlayerPrefs.GetFloat("musicVol", 1f);
        musicSlider.GetComponent<UnityEngine.UI.Slider>().value = musicVol;
        
        if (GameMusic.instance != null)
        {
            GameMusic.instance.musicVolume = volTransform(musicVol);
        }
        
        // Load saved SFX volume
        float sfxVol = PlayerPrefs.GetFloat("sfxVol", 1f);
        sfxSlider.GetComponent<UnityEngine.UI.Slider>().value = sfxVol;
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.sfxVol = volTransform(sfxVol);
        }
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
        if (GameMusic.instance != null )
        {
            GameMusic.instance.musicVolume = volTransform(volume);
        }
        if (menuMusic != null)
        {
            menuMusic.volumeMod = volTransform(volume);
        }

        PlayerPrefs.SetFloat("musicVol", volume);
        if (volume == 0f)
        {
            bool success = SteamUserStats.SetAchievement("SamQuest");
            Debug.Log($"Achievement SamQuest set: {success}");
        }
    }

    public void OnSFXVolumeChange()
    {
        float volume = sfxSlider.GetComponent<UnityEngine.UI.Slider>().value;
        if (AudioManager.instance != null)
        {
            AudioManager.instance.sfxVol = volTransform(volume);
        }
        PlayerPrefs.SetFloat("sfxVol", volume);
    }

    public static float volTransform(float x)
    {
        float b = 1f / (1 - Mathf.Exp(-5f));
        return ((-1 * Mathf.Exp(-5f * x)) + 1) * b; 
    }
}
