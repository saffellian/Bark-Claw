using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroSceneTransition : MonoBehaviour
{
    private VideoPlayer player;

    private void Start()
    {
        player = GetComponent<VideoPlayer>();
        StartCoroutine(WaitForVideo());
    }

    private IEnumerator WaitForVideo()
    {
        yield return new WaitUntil(() => player.isPlaying);
        yield return new WaitUntil(() => player.isPlaying == false);
        // load next scene in build order
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
