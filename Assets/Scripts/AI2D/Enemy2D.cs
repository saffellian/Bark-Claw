using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Enemy2D : MonoBehaviour
{
    public enum EnemyType
    {
        Squirrel,
        Cat,
        Hawk
    }

    [SerializeField] EnemyType enemyType = EnemyType.Squirrel;
    [SerializeField] int health = 50;
    [SerializeField] GameObject projectile;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float slopeFriction = 1;
    [SerializeField] float patrolDistance = 3;
    [SerializeField] float attackRange = 5;
    [SerializeField] float chaseDistance = 5;
    [SerializeField] float meleeRange = 3;
    [SerializeField] int meleeDamage = 5;
    [SerializeField] float attackDelay = 1.5f;
    [SerializeField] float projectileVelocity = 3;
    [SerializeField] AnimationCurve flightPath;
    [SerializeField] float flightHeight = 10f;
    [SerializeField] float flightPathWidth = 10f;
    [SerializeField] float flightPathHeight = 1f;
    [SerializeField] float attackAngle = 45f;
    [SerializeField] float attackDistance = 8f;
    [SerializeField] int attackDamage = 15;
    [SerializeField] float patrolChangeTime = 10;

    private float patrolLeftX, patrolRightX;
    private Rigidbody2D rb;
    private bool isGrounded = true;
    private SpriteRenderer sr;
    private Animator animator;
    private bool leftWalk;
    private AnimationCurve worldFlightCurve;
    private bool colliding = true;
    private Collider2D currCollider;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        patrolLeftX = (transform.position - (Vector3.right * patrolDistance)).x;
        patrolRightX = (transform.position + (Vector3.right * patrolDistance)).x;

        if (enemyType == EnemyType.Hawk)
        {
            worldFlightCurve = new AnimationCurve();
            worldFlightCurve.preWrapMode = WrapMode.PingPong;
            worldFlightCurve.postWrapMode = WrapMode.PingPong;
            for (float i = 0; i < 100; ++i)
            {
                worldFlightCurve.AddKey(transform.position.x + (i / (100 / flightPathWidth)) - flightPathWidth / 2, 
                    transform.position.y + flightHeight + flightPath.Evaluate(i * 1 / 100) * flightPathHeight);
            }
        }

        StartCoroutine(BrainLogic());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position - (Vector3.right * patrolDistance));
            Gizmos.DrawLine(transform.position, transform.position + (Vector3.right * patrolDistance));
            if (enemyType != EnemyType.Hawk)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(transform.position, Vector3.back, attackRange);
            }
            if (enemyType == EnemyType.Cat)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(transform.position, Vector3.back, meleeRange);
            }
            else if (enemyType == EnemyType.Hawk)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(transform.position, Vector3.back, meleeRange);
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(transform.position, Vector3.back, chaseDistance);
                List<Vector3> fPath = new List<Vector3>();
                for (float i = 0; i < 100; ++i)
                {
                    fPath.Add(transform.position + new Vector3((i / (100 / flightPathWidth)) - flightPathWidth / 2, flightHeight + flightPath.Evaluate(i * 1 / 100) * flightPathHeight, 0));
                }
                Handles.color = Color.green;
                Handles.DrawPolyLine(fPath.ToArray());
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, attackAngle) * Vector2.down * attackDistance));
                Gizmos.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, -attackAngle) * Vector2.down * attackDistance));
            }
        }
    }

    private IEnumerator BrainLogic()
    {
        while (gameObject.activeInHierarchy)
        {
            if (enemyType == EnemyType.Squirrel || enemyType == EnemyType.Cat)
                yield return GroundPatrol();
            else if (enemyType == EnemyType.Hawk)
                yield return HawkGroundPatrol();

            if (enemyType == EnemyType.Squirrel)
                yield return SquirrelAttack();
            else if (enemyType == EnemyType.Cat)
                yield return CatAttack();
        }
    }

    private IEnumerator GroundPatrol()
    {
        leftWalk = true;
        while (gameObject.activeInHierarchy)
        {
            Collider2D c = Physics2D.OverlapCircle(transform.position, attackRange, 1 << LayerMask.NameToLayer("Player"));
            if (c != null)
            {
                // check for line of site blockage
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, GameObject.Find("Player").transform.position - transform.position, attackRange);
                bool found = false;
                foreach (RaycastHit2D h in hits)
                {
                    if (h.transform == transform) // skip self hits
                        continue;

                    if (h.transform.CompareTag("Player"))
                    {
                        found = true;
                        break; // it's attack time
                    }

                    break; // not first hit, so it's being blocked
                }
                if (found)
                    break;
            }

            if (leftWalk)
            {
                rb.velocity = Vector2.left;
                sr.flipX = true;
            }
            else
            {
                rb.velocity = Vector2.right;
                sr.flipX = false;
            }

            NormalizeSlope();

            animator.SetBool("Movement", Mathf.Abs(rb.velocity.x) > 0.01f);

            if (transform.position.x < patrolLeftX)
                leftWalk = false;
            else if (transform.position.x > patrolRightX)
                leftWalk = true;

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator HawkGroundPatrol()
    {
        float time = Time.time;
        while (gameObject.activeInHierarchy)
        {
            if (Time.time - time >= patrolChangeTime)
            {
                yield return HawkAirPatrol();
                time = Time.time;
            }

            Collider2D c = Physics2D.OverlapCircle(transform.position, chaseDistance, 1 << LayerMask.NameToLayer("Player"));
            if (c != null)
            {
                // check for line of site blockage
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, GameObject.Find("Player").transform.position - transform.position, chaseDistance);
                foreach (RaycastHit2D h in hits)
                {
                    if (h.transform == transform) // skip self hits
                        continue;

                    if (h.transform.CompareTag("Player"))
                    {
                        yield return HawkGroundAttack(); // it's attack time
                    }

                    break; // not first hit, so it's being blocked
                }
            }

            if (leftWalk)
            {
                rb.velocity = Vector2.left;
                sr.flipX = true;
            }
            else
            {
                rb.velocity = Vector2.right;
                sr.flipX = false;
            }

            NormalizeSlope();

            animator.SetBool("Movement", Mathf.Abs(rb.velocity.x) > 0.01f);

            if (transform.position.x < patrolLeftX)
                leftWalk = false;
            else if (transform.position.x > patrolRightX)
                leftWalk = true;

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator HawkAirPatrol()
    {
        animator.SetBool("Flying", true);
        //fly up
        float height = worldFlightCurve.Evaluate(transform.position.x);
        rb.velocity = Vector2.zero;
        while (transform.position.y < height)
        {
            rb.AddRelativeForce(Vector2.up / 4, ForceMode2D.Impulse);
            yield return new WaitForFixedUpdate();
        }

        float time = Time.time;
        while (Time.time - time < patrolChangeTime)
        {
            if (transform.position.y < worldFlightCurve.Evaluate(transform.position.x))
            {
                rb.MovePosition(new Vector2(transform.position.x, worldFlightCurve.Evaluate(transform.position.x)));
            }

            yield return new WaitForFixedUpdate();

            if (leftWalk)
            {
                rb.velocity = Vector2.left * 3;
                sr.flipX = true;
                // raycast left
                RaycastHit2D r = Physics2D.Raycast(transform.position, (transform.position + (Quaternion.Euler(0, 0, -attackAngle) * Vector2.down)) - transform.position, attackDistance, ~(1 << LayerMask.NameToLayer("Enemy")));
                Debug.DrawRay(transform.position, (transform.position + (Quaternion.Euler(0, 0, -attackAngle) * Vector2.down)) - transform.position);
                if (r.collider != null && r.transform.CompareTag("Player"))
                {
                    yield return HawkAirAttack(r.collider);
                    // fly back up
                    rb.velocity = Vector2.zero;
                    while (transform.position.y < worldFlightCurve.Evaluate(transform.position.x))
                    {
                        rb.AddRelativeForce(Vector2.up / 4, ForceMode2D.Impulse);
                        yield return new WaitForFixedUpdate();
                    }
                }
            }
            else
            {
                rb.velocity = Vector2.right * 3;
                sr.flipX = false;
                // raycast right
                RaycastHit2D r = Physics2D.Raycast(transform.position, (transform.position + (Quaternion.Euler(0, 0, attackAngle) * Vector2.down)) - transform.position, attackDistance, ~(1 << LayerMask.NameToLayer("Enemy")));
                Debug.DrawRay(transform.position, (transform.position + (Quaternion.Euler(0, 0, attackAngle) * Vector2.down)) - transform.position);
                if (r.collider != null && r.transform.CompareTag("Player"))
                {
                    yield return HawkAirAttack(r.collider);
                    // fly back up
                    rb.velocity = Vector2.zero;
                    while (transform.position.y < worldFlightCurve.Evaluate(transform.position.x))
                    {
                        rb.AddRelativeForce(Vector2.up / 4, ForceMode2D.Impulse);
                        yield return new WaitForFixedUpdate();
                    }
                }
            }

            animator.SetBool("Movement", Mathf.Abs(rb.velocity.x) > 0.01f);

            if (transform.position.x < patrolLeftX)
                leftWalk = false;
            else if (transform.position.x > patrolRightX)
                leftWalk = true;

            yield return new WaitForFixedUpdate();
        }

        // fly down
        rb.AddRelativeForce(Vector2.down / 4, ForceMode2D.Impulse);
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => Mathf.Abs(rb.velocity.y) <= Mathf.Epsilon);
        animator.SetBool("Flying", false);
    }

    private IEnumerator HawkGroundAttack()
    {
        float time = Time.time;
        while (Physics2D.OverlapCircle(transform.position, chaseDistance, 1 << LayerMask.NameToLayer("Player")))
        {
            if (!Physics2D.OverlapCircle(transform.position, meleeRange, 1 << LayerMask.NameToLayer("Player")))
            {
                animator.SetBool("Movement", true);
                // check close distance
                // overlap circle for swipe attack
                if ((GameObject.Find("Player").transform.position - transform.position).x < 0) // player to the left
                {
                    rb.velocity = Vector2.left * 2;
                    sr.flipX = true;
                    leftWalk = true;
                    NormalizeSlope();
                }
                else // player to the right
                {
                    rb.velocity = Vector2.right * 2;
                    leftWalk = false;
                    sr.flipX = false;
                    NormalizeSlope();
                }
            }

            Collider2D c;
            if (c = Physics2D.OverlapCircle(transform.position, meleeRange, 1 << LayerMask.NameToLayer("Player")))
            {
                animator.SetBool("Movement", false);
                sr.flipX = (c.transform.position - transform.position).x < 0;
                if (Time.time - time > attackDelay)
                {
                    time = Time.time;
                    rb.velocity = Vector2.zero;
                    animator.SetBool("Attack", true);
                    c.GetComponent<PlayerHealth>().ApplyDamage(meleeDamage);
                    yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
                    yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
                    animator.SetBool("Attack", false);
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator HawkAirAttack(Collider2D playerCollider)
    {
        animator.SetBool("Attack", true);
        Debug.Log("start");
        rb.AddRelativeForce(playerCollider.transform.position - transform.position, ForceMode2D.Impulse);
        yield return new WaitUntil(() => colliding);
        Debug.Log("end");
        animator.SetBool("Attack", false);
        if (currCollider.transform.CompareTag("Player"))
            PlayerHealth.Instance.ApplyDamage(attackDamage);
        yield return new WaitForSeconds(1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        colliding = true;
        currCollider = collision.collider;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        colliding = false;
    }

    private IEnumerator SquirrelAttack()
    {
        Rigidbody2D acorn = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
        acorn.GetComponent<Projectile2D>().RegisterNoCollideObject(gameObject);
        acorn.velocity = (GameObject.Find("Player").transform.position - transform.position).normalized * projectileVelocity;
        animator.SetBool("Attack", true);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
        animator.SetBool("Attack", false);
        yield return new WaitForSeconds(attackDelay);
    }

    private IEnumerator CatAttack()
    {
        float time = Time.time;
        while (Physics2D.OverlapCircle(transform.position, attackRange, 1 << LayerMask.NameToLayer("Player")))
        {
            if (!Physics2D.OverlapCircle(transform.position, meleeRange, 1 << LayerMask.NameToLayer("Player")))
            {
                animator.SetBool("Movement", true);
                // check close distance
                // overlap circle for swipe attack
                if ((GameObject.Find("Player").transform.position - transform.position).x < 0) // player to the left
                {
                    rb.velocity = Vector2.left;
                    sr.flipX = true;
                    leftWalk = true;
                    NormalizeSlope();
                }
                else // player to the right
                {
                    rb.velocity = Vector2.right;
                    leftWalk = false;
                    sr.flipX = false;
                    NormalizeSlope();
                }
            }

            Collider2D c;
            if (c = Physics2D.OverlapCircle(transform.position, meleeRange, 1 << LayerMask.NameToLayer("Player")))
            {
                animator.SetBool("Movement", false);
                sr.flipX = (c.transform.position - transform.position).x < 0;
                if (Time.time - time > attackDelay)
                {
                    time = Time.time;
                    rb.velocity = Vector2.zero;
                    animator.SetBool("Attack", true);
                    c.GetComponent<PlayerHealth>().ApplyDamage(meleeDamage);
                    yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
                    yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"));
                    animator.SetBool("Attack", false);
                }
            }
            else if (c = Physics2D.OverlapCircle(transform.position, attackRange, 1 << LayerMask.NameToLayer("Player"))) // check far distance
            {
                if (Time.time - time > attackDelay)
                {
                    animator.SetBool("Movement", false);
                    time = Time.time;
                    rb.velocity = Vector2.zero;
                    Rigidbody2D furball = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                    furball.GetComponent<Projectile2D>().RegisterNoCollideObject(gameObject);
                    furball.velocity = (c.transform.position - transform.position).normalized * projectileVelocity;
                }
            }


            yield return new WaitForFixedUpdate();
        }
    }

    public void ApplyDamage(int amount)
    {
        if (health <= 0)
            return;

        health -= amount;

        if (health <= 0)
        {
            StopAllCoroutines();
            animator.SetBool("Dead", true);
            animator.SetTrigger("Dying");
        }
    }

    void NormalizeSlope()
    {
        // Attempt vertical normalization
        if (isGrounded)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, 5f, whatIsGround);
            //Debug.DrawRay(transform.position, Vector2.down * 5, Color.red, 5, false);

            if (hit.collider != null && Mathf.Abs(hit.normal.x) > 0.1f)
            {
                Rigidbody2D body = GetComponent<Rigidbody2D>();
                // Apply the opposite force against the slope force 
                // You will need to provide your own slopeFriction to stabalize movement
                body.velocity = new Vector2(body.velocity.x - (hit.normal.x * slopeFriction), body.velocity.y);

                //Move Player up or down to compensate for the slope below them
                Vector3 pos = transform.position;
                pos.y += -hit.normal.x * body.velocity.x * Time.deltaTime;//-hit.normal.x * Mathf.Abs(body.velocity.x) * Time.deltaTime * (body.velocity.x - hit.normal.x > 0 ? 1 : -1);
                transform.position = pos;
            }
        }
    }
}
