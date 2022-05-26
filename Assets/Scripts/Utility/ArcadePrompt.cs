using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArcadePrompt : MonoBehaviour
{
    private bool inPrompt = false;
    private GameStateManager gameStateManager;

    private void Start()
    {
        gameStateManager = GameStateManager.Instance;
    }

    private void Update()
    {
        if (inPrompt && Input.GetKeyDown(KeyCode.E))
        {
            gameStateManager?.SaveGameState("arcade", true);
            SceneManager.LoadScene("2DLevel");
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.transform.CompareTag("Player"))
        {
            inPrompt = true;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.transform.CompareTag("Player"))
        {
            inPrompt = false;
        }
    }
}
