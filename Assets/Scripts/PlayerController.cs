using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;
    private Rigidbody2D myRigidbody;

    public float jumpSpeed;

    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask whatIsGround;

    public bool isGrounded;
    public Vector3 projectileOffset;
    public GameObject projectile;
    public float projectileSpeed = 5f;

    private Animator animator;
    private SpriteRenderer sr;
    private bool attacking = false;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update() 
    {

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        animator.SetBool("InAir", !isGrounded);

        if (Input.GetAxisRaw("Horizontal") > 0.01f)
        {
            myRigidbody.velocity = new Vector3(moveSpeed, myRigidbody.velocity.y, 0f);
            sr.flipX = false;
            animator.SetBool("Movement", true);
        }
        else if (Input.GetAxisRaw("Horizontal") < -0.01f)
        {
            myRigidbody.velocity = new Vector3(-moveSpeed, myRigidbody.velocity.y, 0f);
            sr.flipX = true;
            animator.SetBool("Movement", true);
        }
        else
        {
            myRigidbody.velocity = new Vector3(0f, myRigidbody.velocity.y, 0f);
            animator.SetBool("Movement", false);
        }

        if (Input.GetButtonDown ("Jump") && isGrounded)
        {
            myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, jumpSpeed, 0f);
        }

        if (!attacking && Input.GetButtonDown("Fire1"))
        {
            attacking = true;
            animator.SetBool("Attack", true);
            StartCoroutine(FireHelper());
        }
    }

    private IEnumerator FireHelper()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        animator.SetBool("Attack", false);
        attacking = false;
    }

    private void FireProjectile()
    {
        GameObject p = Instantiate(projectile);
        p.GetComponent<Projectile2D>().RegisterNoCollideTag("Player");
        if (!sr.flipX) // facing right
        {
            p.transform.position = transform.position + projectileOffset;
            p.GetComponent<Rigidbody2D>().velocity = Vector3.right * projectileSpeed;
        }
        else // facing left
        {
            p.transform.position = transform.position - projectileOffset;
            p.transform.localScale = new Vector3(-p.transform.localScale.x, p.transform.localScale.y, p.transform.localScale.z);
            p.GetComponent<Rigidbody2D>().velocity = Vector3.left * projectileSpeed;
        }
    }
}   


