using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeFixer : MonoBehaviour
{

    public AudioMixer audioMixer;

    public static VolumeFixer instance;

    // Constants for tuning
    private const float MinVolumeDB = -80f;
    private const float MaxBassBoost = 12f;
    private const float MaxBassMusic = 6f;
    private const float MaxTrebleMusic = 2f;

    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetFromPrefs()
    {
        instance.SetMusicVolume(PlayerPrefs.GetFloat("musicVol", 1f));
        instance.SetSFXVolume(PlayerPrefs.GetFloat("sfxVol", 1f));
    }

    public void SetMusicVolume(float sliderValue)
    {
        float value = Mathf.Max(sliderValue, 0.0001f);

        // 1. Calculate Master Volume (Logarithmic)
        float masterVolumeDB = Mathf.Log10(value) * 20;
        audioMixer.SetFloat("MusicVolume", masterVolumeDB);

        // 2. Calculate Loudness Compensation (Inverse of Volume)

        // Normalize volume to a 0-1 range where 1 is loud (0dB) and 0 is silent (-80dB)
        float normalizedVolume = 1f - (masterVolumeDB / MinVolumeDB);

        // Invert it: 1 means we are silent (need max boost), 0 means we are loud (no boost)
        float compensationFactor = 1f - normalizedVolume;

        // Apply compensation
        float bassBoost = Mathf.Max(compensationFactor * MaxBassMusic, 1);
        float trebleBoost = Mathf.Max(compensationFactor * MaxTrebleMusic, 1);

        audioMixer.SetFloat("MusicBassGain", bassBoost);
        audioMixer.SetFloat("MusicTrebleGain", trebleBoost);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float value = Mathf.Max(sliderValue, 0.0001f);

        // 1. Calculate Master Volume (Logarithmic)
        float masterVolumeDB = Mathf.Log10(value) * 20;
        audioMixer.SetFloat("SfxVolume", masterVolumeDB);

        // 2. Calculate Loudness Compensation (Inverse of Volume)

        // Normalize volume to a 0-1 range where 1 is loud (0dB) and 0 is silent (-80dB)
        float normalizedVolume = 1f - (masterVolumeDB / MinVolumeDB);

        // Invert it: 1 means we are silent (need max boost), 0 means we are loud (no boost)
        float compensationFactor = 1f - normalizedVolume;

        // Apply compensation
        float bassBoost = Mathf.Max(1.15f, compensationFactor * MaxBassBoost);

        audioMixer.SetFloat("SfxBassGain", bassBoost);
    }
}
