using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    [Serializable]
    public struct LevelAudio
    {
        public string levelName;
        public AudioClip audioClip;
    }
    
    public static AudioController Instance;

    [SerializeField] private List<LevelAudio> levelMusic = new List<LevelAudio>();

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

        SceneManager.sceneLoaded += OnSceneLoaded;
        AudioSource[] sources = GetComponents<AudioSource>();
        musicSource = sources[0];
        effectsSource = sources[1];

        StartCoroutine(InitMusic());
    }

    private IEnumerator InitMusic()
    {
        yield return new WaitUntil(() => SettingsLoader.Instance && SettingsLoader.Instance.menuInitialized);
        foreach (LevelAudio la in levelMusic)
        {
            if (la.levelName == SceneManager.GetActiveScene().name && musicSource.clip != la.audioClip)
            {
                musicSource.clip = la.audioClip;
                musicSource.Play();
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        foreach (LevelAudio la in levelMusic)
        {
            if (la.levelName == SceneManager.GetActiveScene().name && musicSource.clip != la.audioClip)
            {
                musicSource.clip = la.audioClip;
                musicSource.Play();
                break;
            }
        }
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

    public AudioSource GetMusicSource()
    {
        return musicSource;
    }
}
