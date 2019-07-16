using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] private GameObject loadingCanvas;
    
    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ChangeSceneAsync(string scene)
    {
        if (loadingCanvas == null)
        {
            Debug.LogWarning("Loading Canvas is not linked. Unable to load level asynchronously.");
            return;
        }

        StartCoroutine(LoadScene(SceneManager.LoadSceneAsync(scene)));
    }

    private IEnumerator LoadScene(AsyncOperation operation)
    {
        Debug.Log("started");
        while (!operation.isDone)
        {
            Debug.Log(operation.progress);
            yield return null;
        }
    }
}
