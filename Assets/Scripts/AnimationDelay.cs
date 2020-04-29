using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDelay : MonoBehaviour
{
    // Start is called before the first frame update
    public float startTime;
    public float endTime;
    Animator animator;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(DelayedAnimation());
    }

    // The delay coroutine
    IEnumerator DelayedAnimation()
    {
        AudioSource musicSource = FindObjectOfType<AudioController>().GetMusicSource();
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(startTime);
            animator.SetTrigger("Start");
            yield return new WaitForSeconds(endTime - startTime);
            animator.SetTrigger("Stop");
            yield return new WaitUntil(() => musicSource.time < startTime); // check if it has looped yet
        }
    }

}