using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameMusic : MonoBehaviour
{
    [Range(0, 1)]
    public float transitionTime;

    public Sound[] baseTracks;
    public Sound legendaryTrack;
    public Sound lowHealthTrack;
    public Sound deathTrack;

    public static GameMusic instance;

    private static AudioSource sourceA;
    private static AudioSource sourceB;

    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private Sound currentTrack;

    private MenuMusic menuMusic;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        menuMusic = MenuMusic.instance;
        if (menuMusic != null)
        {
            menuMusic.LeaveLevel();
        }
    }

    private void Start()
    {
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();

        sourceA.loop = false;
        sourceB.loop = false;
        sourceA.volume = 0.0f;
        sourceB.volume = 0.0f;

        activeSource = sourceA;
        inactiveSource = sourceB;
    }

    private void Update()
    {
        // If the active source is not playing, play a new track
        if (!activeSource.isPlaying)
        {
            activeSource.clip = getNewBaseTrack().clip;
            activeSource.Play();
        }

        // Lower the volume of the inactive source and increase the volume of the active source
        activeSource.volume = Mathf.Min(1, activeSource.volume + Time.deltaTime / transitionTime);
        inactiveSource.volume = Mathf.Max(0, inactiveSource.volume - Time.deltaTime / transitionTime);
    }

    public void PlayEventTrack(string track)
    {
        Sound trackToPlay;
        switch (track)
        {
            case "legendary":
                trackToPlay = legendaryTrack;
                break;
            case "lowHealth":
                trackToPlay = lowHealthTrack;
                break;
            case "death":
                trackToPlay = deathTrack;
                break;
            default:
                Debug.LogWarning("Track: " + track + " not found!");
                return;
        }

        if (trackToPlay == currentTrack)
        {
            return;
        }

        Debug.Log("Playing " + track + " track");

        currentTrack = trackToPlay;

        inactiveSource.clip = currentTrack.clip;

        // Swap the active and inactive sources
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
        activeSource.Play();
    }

    private Sound getNewBaseTrack()
    {
        if (baseTracks.Length < 2)
        {
            return null;
        }
        while (true)
        {
            Sound newTrack = baseTracks[Random.Range(0, baseTracks.Length)];
            if (newTrack != currentTrack)
            {
                return newTrack;
            }
        }
    }
}


