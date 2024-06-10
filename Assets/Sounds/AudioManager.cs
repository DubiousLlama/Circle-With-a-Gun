using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AudioManager : MonoBehaviour
{
    public Sound[] sfx, music;
    
    public static AudioManager instance;

    private List<AudioSource> SfxSources;

    private List<AudioSource> MusicSources;

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
        SfxSources = new List<AudioSource>();
        MusicSources = new List<AudioSource>();
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

        // Add an AudioSource to the AudioManager GameObject
        AudioSource source = gameObject.AddComponent<AudioSource>();
        SfxSources.Add(source);

        // Set the source's clip and volume
        source.clip = s.clip;
        if (vol == -1) {
            source.volume = s.volume;
        }
        else
        {
            source.volume = vol;
        }
        
        source.Play();

        // remove the AudioSource after the clip has finished playing
        Destroy(source, s.clip.length);
    }

    public int EndSfx(string name)
    {
        int count = 0;
        foreach (AudioSource source in SfxSources)
        {
            if (source.name == name)
            {
                source.Stop();
                count++;
            }
        }
        return count;
    }

    public void EndAll()
    {
        foreach (AudioSource source in SfxSources)
        {
            source.Stop();
        }
        foreach (AudioSource source in MusicSources)
        {
            source.Stop();
        }
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

        GameObject go = new GameObject("SFX");
        go.name = name;
        AudioSource source = go.AddComponent<AudioSource>();
        MusicSources.Add(source);

        // Fade out the current music, if applicable.
        if (MusicSources.Count > 1)
        {
            StartCoroutine(FadeOut(MusicSources[1], fadeOutTime));
        }

        // Instantiate a new AudioSource to play the new music
        source = gameObject.AddComponent<AudioSource>();
        source.clip = s.clip;
        source.volume = s.volume;

        // Destroy the GameObject after the clip has finished playing
        Destroy(go, s.clip.length);

        // Fade in the new music
        StartCoroutine(FadeIn(source, fadeInTime));
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
