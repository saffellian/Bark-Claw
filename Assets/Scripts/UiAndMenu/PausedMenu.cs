﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausedMenu: MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI, settingCanvas;

    // Update is called once per frame
    void Update()
    {
        if (PlayerHealth.Instance.IsAlive() && Input.GetButtonDown("Pause"))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    
    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void OpenSettings()
    {
        settingCanvas.GetComponent<SettingsMenu>()?.ActivateOverlay(gameObject);
        gameObject.SetActive(false);
    }
    
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}