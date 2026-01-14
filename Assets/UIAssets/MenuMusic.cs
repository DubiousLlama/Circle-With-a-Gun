using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMusic : MonoBehaviour
{
    public Sound music;

    [Range(0, 1)]
    public float leavingLevelTime;

    [Range(1, 8)]
    public float fadeInTime;

    public float volumeMod;

    public AudioSource musicSource;

    public static MenuMusic instance;

    private bool leavingLevel = false;

    void Awake()
    {
        volumeMod = 0f;
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        VolumeFixer.instance.SetFromPrefs();
        musicSource.clip = music.clip;
        // Ensure audio source is silent before any playback
        musicSource.volume = 0f;
    }

    void Update()
    {
        if (leavingLevel)
        {
            musicSource.volume = Mathf.Max(0, musicSource.volume - Time.unscaledDeltaTime / leavingLevelTime);
            if (musicSource.volume == 0)
            {
                Destroy(gameObject);
            }
        } else
        {
            musicSource.volume = Mathf.Clamp01(musicSource.volume + Time.unscaledDeltaTime / fadeInTime);
        }

        if (!musicSource.isPlaying && !leavingLevel && PlayerPrefs.GetFloat("musicVol", 1f) > 0)
        {
            musicSource.Play();
        }
    }

    public void LeaveLevel()
    {
        leavingLevel = true;
    }
}
