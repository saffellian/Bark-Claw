using UnityEngine;

/// <summary>
/// Ensures proper game state is set for traversal of the main menu.
/// </summary>
public class MainMenuSetup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;   
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
