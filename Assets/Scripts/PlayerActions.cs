using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerActions : MonoBehaviour
{
    private FirstPersonController fpController;
    private Animator animator;
    private bool canAttack = true;
    
    
    // Start is called before the first frame update
    void Start()
    {
        fpController = FindObjectOfType<FirstPersonController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("IsMoving", fpController.HasMovement());

        if (canAttack && Input.GetMouseButtonDown(0))
        {
            canAttack = false;
            animator.SetTrigger("Action");
            GameObject obj = GameObject.Find("AttackCollider");
            Collider[] hits = Physics.OverlapBox(obj.transform.position, obj.GetComponent<Collider>().bounds.extents);
            if (hits.Length > 0)
            {
                foreach (Collider c in hits)
                {
                    if (c.CompareTag("Enemy"))
                    {
                        c.GetComponent<IEnemy>().ApplyDamage(20);
                    }
                }
            }
        }
    }

    public void ActionComplete()
    {
        canAttack = true;
    }
}
