using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationStateManager : MonoBehaviour
{
    private Animator animator;
    private string currentState = string.Empty;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimationState(string newAnimState, int layer = 0)
    {
        if (currentState == newAnimState) return;

        animator.Play(Animator.StringToHash(newAnimState), layer);
        currentState = newAnimState;
    }
}
