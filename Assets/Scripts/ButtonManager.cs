using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : UnityEngine.MonoBehaviour
{
    public void NewGameBtn(string newGameLevel)
    {
        SceneManager.LoadScene(newGameLevel);
    }

    public void ExitGameBtn()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }
}

