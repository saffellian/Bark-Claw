
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField]
    private float delayBeforefloating = 12f;
    [SerializeField]
    private string sceneNameToLoad;

    private float timeElapsed;

    private void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > delayBeforefloating)
        {
            SceneManager.LoadScene(sceneNameToLoad) ;
        }
    }
}
