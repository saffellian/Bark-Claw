using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityStandardAssets.CrossPlatformInput;
using Button = UnityEngine.UI.Button;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider effectsSlider, musicSlider, mouseSlider;
    public TMP_InputField mouseTextField;
    public Dropdown resolutionDropdown;
    public Button applyButton;
    public Toggle postProcessingToggle;

    private Resolution[] resolutions;
    private bool initialized = false;
    private GameObject calledBy = null;
    private Scene currentScene;

    private void OnEnable()
    {
        currentScene = SceneManager.GetActiveScene();
        
        resolutions = Screen.resolutions.Select(res => new Resolution {width = res.width, height = res.height}).Distinct().ToArray();

        resolutionDropdown.ClearOptions();
        
        List<string> options = new List<string>();

        int currentRes = 1;
        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + "x" + resolutions[i].height);
            
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                Debug.Log(i);
                currentRes = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentRes;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.AddListener(delegate { applyButton.interactable = true; });
        applyButton.onClick.AddListener(delegate { 
            SetResolution(resolutionDropdown.value);
            applyButton.interactable = false;
        });
        effectsSlider.onValueChanged.AddListener(delegate { SetEffectsVolume(effectsSlider.value); });
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicSlider.value); });
        mouseSlider.onValueChanged.AddListener(delegate { SetMouseSensitivity(mouseSlider.value); });
        mouseTextField.onEndEdit.AddListener(delegate { UpdateMouseInputField(mouseTextField.text); });
        postProcessingToggle.onValueChanged.AddListener(delegate { SetActivePostProcessing(postProcessingToggle.isOn); });
        initialized = true;

        if (currentScene.name == "1")
        {
            LoadSettings();
        }
    }

    private void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat(PlayerPrefKeys.MUSIC_VOLUME, musicSlider.value);
    }
    
    private void SetEffectsVolume(float volume)
    {
        audioMixer.SetFloat("EffectsVolume", volume);
        PlayerPrefs.SetFloat(PlayerPrefKeys.SFX_VOLUME, effectsSlider.value);
    }

    private void SetMouseSensitivity(float value)
    {
        PlayerPrefs.SetFloat(PlayerPrefKeys.MOUSE_SENS, mouseSlider.value);
        mouseTextField.placeholder.GetComponent<TextMeshProUGUI>().text = mouseSlider.value.ToString("F2");
    }

    private void UpdateMouseInputField(string text)
    {
        float result;
        if (float.TryParse(text, out result))
        {
            result = Mathf.Clamp(result, mouseSlider.minValue, mouseSlider.maxValue); // prevent input outside of slider range
            mouseTextField.placeholder.GetComponent<TextMeshProUGUI>().text = result.ToString("F2");
            mouseTextField.text = string.Empty;
            mouseSlider.value = result;
        }
    }

    private void SetActivePostProcessing(bool enable)
    {
        PlayerPrefs.SetInt(PlayerPrefKeys.POST_PROC, enable ? 1 : 0);
        if (SettingsLoader.Instance)
        {
            SettingsLoader.Instance.UpdateSettings(currentScene);
        }
    }

    private void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        if (Screen.height != res.height || Screen.width != res.width)
        {
            Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        }
        PlayerPrefs.SetInt(PlayerPrefKeys.RES_WIDTH, Screen.width);
        PlayerPrefs.SetInt(PlayerPrefKeys.RES_HEIGHT, Screen.height);
    }

    public void LoadSettings()
    {
        musicSlider.value = PlayerPrefs.GetFloat(PlayerPrefKeys.MUSIC_VOLUME, 0);
        effectsSlider.value = PlayerPrefs.GetFloat(PlayerPrefKeys.SFX_VOLUME, 0);
        mouseSlider.value = PlayerPrefs.GetFloat(PlayerPrefKeys.MOUSE_SENS, 1);
        postProcessingToggle.isOn = PlayerPrefs.GetInt(PlayerPrefKeys.POST_PROC, 1) == 1;
    }

    public void ActivateOverlay(GameObject callingObject)
    {
        calledBy = callingObject;
        callingObject.SetActive(false);
        gameObject.SetActive(true);
    }
    
    public void BackButtonPressed()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Settings":
                SceneManager.LoadScene("Menu");
                break;
            case "1":
                if (calledBy != null)
                {
                    calledBy.SetActive(true);
                    gameObject.SetActive(false);
                }
                break;
        }
        
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Delete PlayerPref data related to settings for testing purposes.
    /// </summary>
    [ContextMenu("Clear Player Prefs")]
    private void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(PlayerPrefKeys.MUSIC_VOLUME);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.SFX_VOLUME);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.MOUSE_SENS);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.POST_PROC);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.RES_WIDTH);
        PlayerPrefs.DeleteKey(PlayerPrefKeys.RES_HEIGHT);
    }
#endif
}
