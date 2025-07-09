using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    public AudioClip musicClip;
    public AudioClip clickClip;

    void Start()
    {
        musicSource.clip = musicClip;
        musicSource.Play();
    }

    public void playClickSound(AudioClip clip)
    {
        sfxSource.clip = clip;
        sfxSource.PlayOneShot(clip);
    }
}
