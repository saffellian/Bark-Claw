using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    private AudioSource musicSource, effectsSource;
    
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);

        AudioSource[] sources = GetComponents<AudioSource>();
        musicSource = sources[0];
        effectsSource = sources[1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayOneShot(AudioClip clip, float volume)
    {
        effectsSource.PlayOneShot(clip, volume);
    }
    
    public void PlayOneShot(AudioClip clip)
    {
        effectsSource.PlayOneShot(clip);
    }

    public void Play()
    {
        musicSource.Play();
    }

    public void Stop()
    {
        musicSource.Stop();
    }

    public void SetMusic(AudioClip music)
    {
        musicSource.clip = music;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.outputAudioMixerGroup.audioMixer.SetFloat("MusicVolume", volume);
    }
    
    public void SetEffectsVolume(float volume)
    {
        musicSource.outputAudioMixerGroup.audioMixer.SetFloat("EffectsVolume", volume);
    }
}
