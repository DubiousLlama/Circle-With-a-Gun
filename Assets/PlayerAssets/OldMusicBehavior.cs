using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldMusicBehavior : MonoBehaviour
{


    /*
    public void Update()
    {
        QueueMusic();
    }

    public void QueueMusic()
    {
        List<string> songs = new List<string>() { "cwag1", "cwag2", "cwag4" };

        // Whenever the music ends, play a new song
        if (!MusicSource.isPlaying)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
            {
                currentSong = "title";
                PlayMusic(currentSong);
                return;
            }
            // Get a random song from the list, other than the current song
            // This is kind of a hacky way to do it, it produces a 66% chance to play the next song, and a 33% chance to play the previous song lol. Seems fine for now.
            int songIndex = Random.Range(0, songs.Count);
            if (songs[songIndex] == currentSong)
            {
                songIndex = (songIndex + 1) % songs.Count;
            }
            currentSong = songs[songIndex];
            PlayMusic(currentSong);
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
    */
}
