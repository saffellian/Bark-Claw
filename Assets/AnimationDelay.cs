using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDelay : MonoBehaviour
{
    // Start is called before the first frame update
    public int startDelay;
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
        yield return new WaitForSeconds(startDelay);
        animator.SetTrigger("Start");
    }

}