using System.Collections;
using System.Collections.Generic;
using DFA;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class SettingsLoader : MonoBehaviour
{
    public static SettingsLoader Instance;

    public bool menuInitialized = false;
    
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
        
        StartCoroutine(LoadMenuScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        UpdateSettings(scene);
    }

    public void UpdateSettings(Scene scene)
    {
        switch (scene.name)
        {
            case "1":
                LoadGameplayScene();
                break;
            case "Settings":
                LoadSettingsScene();
                break;
        }
    }

    private IEnumerator LoadMenuScene()
    {
        yield return new WaitUntil(() => AudioController.Instance);
        AudioController.Instance.SetMusicVolume(PlayerPrefs.GetFloat(PlayerPrefKeys.MUSIC_VOLUME, 0));
        AudioController.Instance.SetEffectsVolume(PlayerPrefs.GetFloat(PlayerPrefKeys.SFX_VOLUME, 0));
        menuInitialized = true;
    }

    private void LoadSettingsScene()
    {
        FindObjectOfType<SettingsMenu>().LoadSettings();
    }

    private void LoadGameplayScene()
    {
        PostProcessVolume[] volumes = FindObjectsOfType<PostProcessVolume>();
        
        foreach (PostProcessVolume ppv in volumes)
        {
            if (ppv.profile.HasSettings<PixelNostalgia>())
            {
                ppv.profile.GetSetting<PixelNostalgia>().enabled.value = PlayerPrefs.GetInt(PlayerPrefKeys.POST_PROC, 1) == 1;
            }
        }
    }
}
