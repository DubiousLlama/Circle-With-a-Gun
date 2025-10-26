using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AudioManager : MonoBehaviour
{
    public Sound[] sfx;
    
    public static AudioManager instance;

    private AudioSource SfxSource;

    public float sfxVol = 1.0f;

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
    }

    private void Start()
    {
        SfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySfx(string name, float vol=-1f)
    {
        vol = (vol < 0f) ? sfxVol : vol;

        // Find the sound in the array
        Sound s = System.Array.Find(sfx, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        // Set the source's clip and volume
        SfxSource.PlayOneShot(s.clip, vol * sfxVol);
    }
}
