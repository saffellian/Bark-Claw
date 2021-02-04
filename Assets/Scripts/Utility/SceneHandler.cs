using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private RectTransform barBackground, barForeground;
    
    public void ChangeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void ChangeSceneAsync(string scene)
    {
        if (!(loadingCanvas || barBackground || barForeground))
        {
            Debug.LogWarning("Loading Canvas components not linked. Unable to load level asynchronously.");
            return;
        }

        StartCoroutine(LoadScene(SceneManager.LoadSceneAsync(scene)));
    }

    private IEnumerator LoadScene(AsyncOperation operation)
    {
        operation.allowSceneActivation = false;
        float progress;
        float width = barBackground.rect.width;
        float y = barForeground.offsetMax.y;
        loadingCanvas.SetActive(true);
        while (operation.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame();
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            //Debug.Log(progress);
            barForeground.offsetMax = new Vector2(-(width - (width * progress)), y);
        }
        
        barForeground.offsetMax = new Vector2(0, y);
        yield return new WaitForSeconds(3);
        operation.allowSceneActivation = true;
    }
}
