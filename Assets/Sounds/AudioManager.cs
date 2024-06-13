using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AudioManager : MonoBehaviour
{
    public Sound[] sfx, music;
    
    public static AudioManager instance;

    private AudioSource SfxSource;

    private AudioSource MusicSource;

    private AudioSource gunSource;    

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

    private void Start()
    {
        SfxSource = gameObject.AddComponent<AudioSource>();
        MusicSource = gameObject.AddComponent<AudioSource>();
        gunSource = gameObject.AddComponent<AudioSource>();
    }


    public void PlaySfx(string name, float vol=-1f)
    {
        // Find the sound in the array
        Sound s = System.Array.Find(sfx, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // Set the source's clip and volume
        SfxSource.PlayOneShot(s.clip, vol == -1 ? s.volume : vol);
    }



    public void PlayMusic(string name, float fadeOutTime = 3f, float fadeInTime = -1)
    {
        Sound s = System.Array.Find(music, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (fadeInTime == -1)
        {
            fadeInTime = fadeOutTime / 2;
        }

        // Fade out the current music, if applicable.
        if (MusicSource.isPlaying)
        {
            StartCoroutine(FadeOut(MusicSource, fadeOutTime));
        }

        // Instantiate a new AudioSource to play the new music
        MusicSource = gameObject.AddComponent<AudioSource>();
        MusicSource.clip = s.clip;
        MusicSource.volume = s.volume;

        // Fade in the new music
        StartCoroutine(FadeIn(MusicSource, fadeInTime));
    }

    private IEnumerator FadeOut(AudioSource source, float fadeTime, bool destroySource = true)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
        if (destroySource)
        {
            Destroy(source);
        }
    }

    private IEnumerator FadeIn(AudioSource source, float fadeTime)
    {
        float startVolume = source.volume;
        source.volume = 0;
        source.Play();

        while (source.volume < startVolume)
        {
            source.volume += startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        source.volume = startVolume;
    }
}
