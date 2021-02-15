using System.Collections;
using UnityEngine;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        UpdateSettings(scene);
    }

    public void UpdateSettings(Scene scene)
    {
        switch (scene.name)
        {
            case "3DLevel":
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
        AudioController.Instance.SetMusicVolume(Mathf.Log10(PlayerPrefs.GetFloat(PlayerPrefKeys.MUSIC_VOLUME, 0)) * 20);
        AudioController.Instance.SetEffectsVolume(Mathf.Log10(PlayerPrefs.GetFloat(PlayerPrefKeys.SFX_VOLUME, 0)) * 20);
        menuInitialized = true;
    }

    private void LoadSettingsScene()
    {
        FindObjectOfType<SettingsMenu>().LoadSettings();
    }

    private void LoadGameplayScene()
    {
       
    }
}
