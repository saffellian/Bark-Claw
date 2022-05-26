using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using BarkClaw;

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
    public float iFrameDuration = 2f;

    private Animator animator;
    private SpriteRenderer sr;
    private bool attacking = false;
    private PlayerHealth health;
    private bool isDead = false;
    private float deathHeight = 0;
    private bool iFramesActive = false;

    // Start is called before the first frame update
    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
        health.onDamaged.AddListener(OnDamaged);
        deathHeight = GameObject.FindGameObjectWithTag("KillBoundary").transform.position.y;
        PausedMenu.MenuEvent.AddListener(PauseEventListener);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDamaged(int remainingHealth)
    {
        animator.SetTrigger("Hurt");
    }

    void PauseEventListener(InterfaceEvents e)
    {
        switch (e)
        {
            case InterfaceEvents.RESUME:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case InterfaceEvents.PAUSE:
            case InterfaceEvents.DEATH:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
            default:
                break;
        }
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
        p.GetComponent<Projectile2D>().SetTargetTag("Enemy");
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

    /// <summary>
    /// Sent when an incoming collider makes contact with this object's
    /// collider (2D physics only).
    /// </summary>
    /// <param name="other">The Collision2D data associated with this collision.</param>
    void OnCollisionEnter2D(Collision2D other)
    {
        if (health.IsAlive() && !iFramesActive && other.collider.CompareTag("Enemy"))
        {
            if (stompCollider.IsTouching(other.collider))
            {
                other.gameObject.GetComponent<Enemy2D>().ApplyDamage(enemyTouchDamageGiven);
                myRigidbody.velocity = new Vector3(myRigidbody.velocity.x, jumpSpeed, 0f);
                return;
            }

            health.ApplyDamage(enemyTouchDamageTaken);

            if (health.IsAlive())
            {
                StartCoroutine(StartIFrames());
            }
            else
            {
                animator.SetBool("IsDead", true);
            }
        }
    }

    private IEnumerator StartIFrames()
    {
        iFramesActive = true;
        int prevLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("PlayerIFrame");
        Color color = sr.color;
        Color prevColor = color;
        color.a = 0.3f;
        sr.color = color;
        yield return new WaitForSeconds(iFrameDuration);
        gameObject.layer = prevLayer;
        sr.color = prevColor;
        iFramesActive = false;
    }
}   


