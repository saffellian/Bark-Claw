using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class StringEvent : UnityEvent<string> { }

public class AudioController : MonoBehaviour
{
    [Serializable]
    public struct TimedEvent
    {
        public float timeStamp;
        public string eventName;
    }

    [Serializable]
    public struct LevelAudio
    {
        public string levelName;
        public AudioClip audioClip;
        public TimedEvent[] timedEvent;
    }
    
    public static AudioController Instance;
    public StringEvent audioEvent = new StringEvent();

    [SerializeField] private List<LevelAudio> levelMusic = new List<LevelAudio>();

    private AudioSource musicSource, effectsSource;
    private Dictionary<float, string> audioEvents = new Dictionary<float, string>();
    private float prevTime = 0;
    private float[] keys;
    
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
                audioEvents = new Dictionary<float, string>();
                for (int i = 0; i < la.timedEvent.Length; i++)
                {
                    audioEvents.Add(la.timedEvent[i].timeStamp, la.timedEvent[i].eventName);
                }
                prevTime = 0;
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (audioEvents.Count == 0)
            return;

        keys = audioEvents.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i] >= prevTime && keys[i] < musicSource.time)
            {
                audioEvent.Invoke(audioEvents[keys[i]]);
            }
        }

        prevTime = musicSource.time;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        foreach (LevelAudio la in levelMusic)
        {
            if (la.levelName == SceneManager.GetActiveScene().name && musicSource.clip != la.audioClip)
            {
                musicSource.clip = la.audioClip;
                musicSource.Play();
                audioEvents = new Dictionary<float, string>();
                for (int i = 0; i < la.timedEvent.Length; i++)
                {
                    audioEvents.Add(la.timedEvent[i].timeStamp, la.timedEvent[i].eventName);
                }
                prevTime = 0;
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
