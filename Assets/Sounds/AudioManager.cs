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
        
        // Load saved SFX volume from PlayerPrefs
        float savedSfxVol = SaveManager.instance.GetFloat("sfxVol", 1f);
        float b = 1f / (1 - Mathf.Exp(-5f)); // Using falloff value from PauseMenuController
        sfxVol = ((-1 * Mathf.Exp(-5f * savedSfxVol)) + 1) * b;
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

        vol = (vol < 0f) ? s.volume : vol;

        // Set the source's clip and volume
        SfxSource.PlayOneShot(s.clip, vol * sfxVol);
    }
}
