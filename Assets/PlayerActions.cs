using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerActions : MonoBehaviour
{
    private FirstPersonController fpController;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        fpController = GameObject.FindObjectOfType<FirstPersonController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsMoving", fpController.HasMovement());

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Action");
        }
    }
}
