using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BarkClaw;

public class PausedMenu: MonoBehaviour
{
    public static UserInterfaceEvent MenuEvent;
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI, settingCanvas;
    private GameStateManager gameStateManager;
    [SerializeField] private Button saveButton;

    void Start()
    {
        if (pauseMenuUI.activeInHierarchy)
        {
            pauseMenuUI.SetActive(false);
        }
        gameStateManager = GameStateManager.Instance;
        MenuEvent = new UserInterfaceEvent();
    }

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
        MenuEvent.Invoke(InterfaceEvents.RESUME);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    
    void Pause()
    {
        MenuEvent.Invoke(InterfaceEvents.PAUSE);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        if (SceneManager.GetActiveScene().name == "3DLevel")
        {
            saveButton.enabled = Enemy.EnemiesAttacking > 0;
        }
        else if (SceneManager.GetActiveScene().name == "2DLevel")
        {
            saveButton.enabled = Enemy2D.EnemiesAttacking > 0;
        }
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

    public void SaveGame()
    {
        if (gameStateManager.SaveGameState() == GameStateManager.GameStateProcessResult.SUCCESS)
        {
            Debug.Log("Game saved successfully.");
        }
        else
        {
            Debug.LogError("Failed to save game.");
        }
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void Exit2DLevel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene("3DLevel");
    }
}