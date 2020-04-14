using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerActions : MonoBehaviour
{
    [SerializeField] private int attackDamage = 10;
    
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

        // TODO: move this old claw logic into weapon script melee type
        if (PlayerHealth.Instance.IsAlive() && canAttack && Input.GetMouseButtonDown(0))
        {
            canAttack = false;
            animator.SetTrigger("Action");
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Equals))
            Debug.Break();
#endif
    }

    public void ActionComplete()
    {
        canAttack = true;
    }

    public void ActionAttack()
    {
        GameObject obj = GameObject.Find("AttackCollider");
        Collider[] hits = Physics.OverlapBox(obj.transform.position, obj.GetComponent<Collider>().bounds.extents);
        if (hits.Length > 0)
        {
            foreach (Collider c in hits)
            {
                if (c.CompareTag("Enemy"))
                {
                    c.GetComponent<Enemy>().ApplyDamage(attackDamage);
                }
            }
        }
    }
}
