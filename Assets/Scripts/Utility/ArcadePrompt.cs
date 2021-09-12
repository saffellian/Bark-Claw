using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ArcadePrompt : MonoBehaviour
{
    private bool inPrompt = false;

    private void Update()
    {
        if (inPrompt && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene("2DLevel");
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.transform.CompareTag("Player"))
        {
            inPrompt = true;
            Debug.Log("prompt displays");
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.transform.CompareTag("Player"))
        {
            inPrompt = false;
            Debug.Log("prompt goes away");
        }
    }
}
