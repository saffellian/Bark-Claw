using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;

public class DeathCanvas : MonoBehaviour
{

    [SerializeField] private Button retryButton;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeSpeed;
    [SerializeField] private float buttonActivateDelay;
    [SerializeField] private Animator statusAnimator;

    private bool transitioned = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        fadeGroup.alpha = 0;
        retryButton.onClick.AddListener(Retry);
        retryButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // debug
        {
            PlayerHealth.Instance.ApplyDamage(500);
        }
    }

    public void BeginTransition()
    {
        if (transitioned)
            return;
        
        transitioned = true;
        StartCoroutine(Transition());
    }

    private IEnumerator Transition()
    {
        statusAnimator.SetTrigger("Dead");
        gameObject.SetActive(true);
        while (fadeGroup.alpha < 1)
        {
            fadeGroup.alpha += Time.deltaTime / fadeSpeed;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(buttonActivateDelay);
        FindObjectOfType<FirstPersonController>().SetCursorLock(false);
        retryButton.gameObject.SetActive(true);
    }

    private void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // reload current scene
        // implementation can be changed in the future depending on how gameplay should be handled
    }
}
