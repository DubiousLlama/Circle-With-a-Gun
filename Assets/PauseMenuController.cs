using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    ScoreTracker scoreTracker;

    public GameObject musicSlider;
    public GameObject sfxSlider;

    [Range(1, 10)]
    public float falloff = 5f;

    void Start()
    {
        scoreTracker = GameObject.Find("Score").GetComponent<ScoreTracker>();
    }

    public void AbandonRun()
    {
        scoreTracker.GameOver();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OnMusicVolumeChange()
    {
        float volume = musicSlider.GetComponent<UnityEngine.UI.Slider>().value;
        GameMusic.instance.musicVolume = volTransform(volume);
    }

    public void OnSFXVolumeChange()
    {
        float volume = sfxSlider.GetComponent<UnityEngine.UI.Slider>().value;
        AudioManager.instance.sfxVol = volTransform(volume);
    }

    float volTransform(float x)
    {
        float b = 1f / (1 - Mathf.Exp(-falloff));
        return ((-1 * Mathf.Exp(-falloff * x)) + 1) * b; 
    }
}
