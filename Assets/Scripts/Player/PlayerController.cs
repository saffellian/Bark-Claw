using System.Collections;
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
    public float attackDelay = 1f;
    public int enemyTouchDamageGiven = 2;
    public int enemyTouchDamageTaken = 5;
    public Collider2D stompCollider;

    private Animator animator;
    private SpriteRenderer sr;
    private bool attacking = false;
    private PlayerHealth health;
    private bool isDead = false;
    private float deathHeight = 0;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
        health.onDamaged.AddListener(Damaged);
        deathHeight = GameObject.FindGameObjectWithTag("KillBoundary").transform.position.y;
    }

    // Update is called once per frame
    void Update() 
    {
        if (!health.IsAlive())
            return;

        if (transform.position.y < deathHeight)
        {
            health.InstantDeath();
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        animator.SetBool("Jumping", !isGrounded && myRigidbody.velocity.y > 0.1f);

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

        if (!attacking && (Input.GetButtonDown("Fire2") || Input.GetAxis("Right Trigger") != 0))
        {
            attacking = true;
            animator.SetTrigger("Attack");
            StartCoroutine(FireHelper());
        }
    }

    private IEnumerator FireHelper()
    {
        yield return new WaitForSeconds(attackDelay);
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

    private void Damaged(int health)
    {
        if (!isDead && health <= 0)
        {
            isDead = true;
            animator.SetBool("IsDead", true);
        }
    }

    /// <summary>
    /// Sent when an incoming collider makes contact with this object's
    /// collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Enemy"))
        {
            if (stompCollider.IsTouching(other.collider))
            {
                other.gameObject.GetComponent<Enemy2D>().ApplyDamage(enemyTouchDamageGiven);
                myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, jumpSpeed, 0f);
                return;
            }

            health.ApplyDamage(enemyTouchDamageTaken);
        }
    }
}   


