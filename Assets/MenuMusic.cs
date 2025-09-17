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

    public AudioSource musicSource;

    public static MenuMusic instance;

    private bool leavingLevel = false;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        musicSource.clip = music.clip;
        musicSource.volume = 0;
        musicSource.Play();
    }

    void Update()
    {
        if (leavingLevel)
        {
            musicSource.volume = Mathf.Max(0, musicSource.volume - Time.deltaTime / leavingLevelTime);
            if (musicSource.volume == 0)
            {
                Destroy(gameObject);
            }
        } else
        {
            musicSource.volume = Mathf.Min(1, musicSource.volume + Time.deltaTime / fadeInTime);
        }

        if (!musicSource.isPlaying && !leavingLevel)
        {
            musicSource.Play();
        }
    }

    public void LeaveLevel()
    {
        leavingLevel = true;
    }
}
