using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NPBehave;
using SensorToolkit;

public class Enemy2D : MonoBehaviour
{
    public static int EnemiesAttacking = 0;

    public enum EnemyType
    {
        Squirrel,
        Cat,
        Hawk
    }

    [SerializeField] EnemyType enemyType = EnemyType.Squirrel;
    [SerializeField] int health = 50;
    [SerializeField] LayerMask whatIsGround;
    [SerializeField] float slopeFriction = 1;
    [SerializeField] float patrolDistance = 3;
    [SerializeField] float patrolSpeedMultiplier = 1;
    [SerializeField] float chaseSpeedMultiplier = 1.5f;
    [SerializeField] [EnemyType("Squirrel, Cat")] GameObject projectilePrefab;
    [SerializeField] [EnemyType("Squirrel, Cat")] int projectileDamage = 1;
    [SerializeField] [EnemyType("Squirrel, Cat")] float projectileVelocity = 3;
    [SerializeField] int meleeDamage = 5;
    [SerializeField] float attackDelay = 1.5f;
    [SerializeField] [EnemyType("Hawk")] AnimationCurve flightPath;
    [SerializeField] [EnemyType("Hawk")] float flightHeight = 10f;
    [SerializeField] [EnemyType("Hawk")] float flightPathWidth = 10f;
    [SerializeField] [EnemyType("Hawk")] float flightPathHeight = 1f;
    [SerializeField] [EnemyType("Hawk")] float attackAngle = 45f;
    [SerializeField] [EnemyType("Hawk")] float attackDistance = 8f;
    [SerializeField] [EnemyType("Hawk")] int attackDamage = 15;
    [SerializeField] [EnemyType("Hawk")] float patrolChangeTime = 10;
    [SerializeField] List<SensorMap> sensors = new List<SensorMap>();

    private float patrolLeftX, patrolRightX;
    private Rigidbody2D rb;
    private bool isGrounded = true;
    private SpriteRenderer sr;
    private Animator animator;
    private bool leftWalk;
    private AnimationCurve worldFlightCurve;
    private Root behaviorTree;
    private Blackboard blackboard;
    private Transform player;
    private Vector2 startPosition;
    private float attackTime;
    private Collider2D collider;
    private bool previouslyFlipped;
    private bool isDead;
    private bool playerInChaseRange;
    private bool playerInMeleeRange;
    private bool rightExtentReached;
    private bool leftExtentReached;

    [System.Serializable]
    private class SensorMap {
        public Sensor sensor;
        public SensorLocationSync locationSync;
        [HideInInspector]
        public float locationOffset;
    }

    private enum SensorLocationSync {
        NONE,
        MOVE,
        SCALE
    }

    // Start is called before the first frame update
    void Start()
    {
        attackTime = Time.time;

        player = GameObject.Find("Player").transform;
        
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider2D>();

        for (int i = 0; i < sensors.Count; i++)
        {
            if (sensors[i].locationSync == SensorLocationSync.MOVE)
            {
                sensors[i].locationOffset = sensors[i].sensor.transform.localPosition.x;
            }
        }

        startPosition = transform.position;
        patrolLeftX = (transform.position - (Vector3.right * patrolDistance)).x;
        patrolRightX = (transform.position + (Vector3.right * patrolDistance)).x;
        previouslyFlipped = !sr.flipX;

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

        switch(enemyType)
        {
            case EnemyType.Cat:
                behaviorTree = CatBehaviorTree();
                break;
            case EnemyType.Squirrel:
                behaviorTree = SquirrelBehaviorTree();
                break;
            case EnemyType.Hawk:
                behaviorTree = HawkBehaviorTree();
                break;
            default:
                Debug.LogError("Behavior tree not found for enemy type", this);
                break;
        }

        blackboard = behaviorTree.Blackboard;
        behaviorTree.Start();

#if UNITY_EDITOR
        // attach the debugger component if executed in editor (helps to debug in the inspector) 
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
    }

    private void Update()
    {
        isDead = health <= 0;
        rightExtentReached = !leftWalk && (transform.position.x > patrolRightX || bool.Equals(blackboard?.Get("cliff") ?? false, false));
        leftExtentReached = leftWalk && (transform.position.x < patrolLeftX || bool.Equals(blackboard?.Get("cliff") ?? false, false));

        // update sensor locations only when the sprite becomes flipped
        if (previouslyFlipped != sr.flipX)
        {
            float scaleFactor = sr.flipX ? -1f : 1f;
            for (int i = 0; i < sensors.Count; i++)
            {
                if (sensors[i].locationSync == SensorLocationSync.MOVE)
                {
                    sensors[i].sensor.transform.localPosition = new Vector3(scaleFactor * sensors[i].locationOffset, sensors[i].sensor.transform.localPosition.y, 0);
                }
                else if (sensors[i].locationSync == SensorLocationSync.SCALE)
                {
                    sensors[i].sensor.transform.localScale = new Vector3(scaleFactor, 1, 1);
                }
            }
        }

        previouslyFlipped = sr.flipX;
    }

    public void SetDetected(string key)
    {
        blackboard.Set(key, true);
    }

    public void SetNotDetected(string key)
    {
        blackboard.Set(key, false);
    }

    private void UpdateBlackboard()
    {
        behaviorTree.Blackboard["isDead"] = health <= 0;
    }

    private Root HawkBehaviorTree()
    {
        return null;
    }

    private Root CatBehaviorTree()
    {
        Root rootNode = new Root(new Service(0.125f, UpdateBlackboard,
            new BlackboardCondition("isDead", Operator.IS_EQUAL, false, Stops.SELF,
                new Sequence(
                    new Succeeder(
                    new BlackboardCondition(
                        "chase",
                        Operator.IS_NOT_EQUAL,
                        true,
                        Stops.SELF,
                        GroundPatrol()
                    ))
                    { Label = "Patrol" },

                    new Succeeder(
                    new BlackboardQuery(
                        new string[] {"chase", "melee"},
                        Stops.SELF,
                        () => { 
                            return blackboard.Get<bool>("chase") == true 
                                && blackboard.Get<bool>("melee") == false;
                        },
                        GroundChase()
                    ))
                    { Label = "Chase" },

                    new Succeeder(
                    new BlackboardCondition(
                        "melee",
                        Operator.IS_EQUAL,
                        true,
                        Stops.SELF,
                        MeleeAttack()
                    ))
                    { Label = "Attack" },
                    
                    new Action(() => {
                        animator.SetBool("Attack", false);
                    })
                )
            )
        ));

        // set necessary blackboard values
        rootNode.Blackboard.Set("melee", false);
        rootNode.Blackboard.Set("chase", false);

        return rootNode;
    }

    private Root SquirrelBehaviorTree()
    {
        Root rootNode = new Root(new Service(0.125f, UpdateBlackboard,
            new BlackboardCondition("isDead", Operator.IS_EQUAL, false, Stops.SELF,
                new Sequence(

                    new Succeeder(
                    new BlackboardCondition(
                        "playerClose",
                        Operator.IS_EQUAL,
                        false,
                        Stops.SELF,
                        GroundPatrol()
                    ))
                    { Label = "Ground Patrol" },

                    new Succeeder(
                    new BlackboardCondition(
                        "rangeAttack",
                        Operator.IS_EQUAL,
                        true,
                        Stops.SELF,
                        ProjectileAttack()
                    ))
                    { Label = "Projectile Attack" },

                    new Action(() => {
                        animator.SetBool("Attack", false);
                    })
                )
            )
        ));

        // set necessary blackboard values
        rootNode.Blackboard.Set("playerClose", false);
        rootNode.Blackboard.Set("attackRange", false);

        return rootNode;
    }

    private Node GroundPatrol()
    {
        return new Repeater(new Sequence(
            new Action(() =>
            {
                animator.SetBool("Movement", true);
                sr.flipX = false;
                leftWalk = false;
            }),
            new Wait(0.3f),
            new Succeeder(
                new Condition(() => { return !rightExtentReached; }, Stops.SELF,
                new Repeater(new Action(() =>
                {
                    rb.velocity = Vector2.right * patrolSpeedMultiplier;

                    NormalizeSlope();
                }))
            )),
            new Action(() =>
            {
                sr.flipX = true;
                leftWalk = true;
            }),
            new Wait(0.3f),
            new Succeeder(
                new Condition(() => { return !leftExtentReached; }, Stops.SELF,
                new Repeater(new Action(() =>
                {
                    rb.velocity = Vector2.left * patrolSpeedMultiplier;

                    NormalizeSlope();
                }))
            ))
        ))
        { Label = "Ground Patrol" };
    }

    private Node AirPatrol()
    {
        return null;
    }

    private Node GroundChase()
    {
        return new Repeater(
            new Sequence(
                new Succeeder(
                    new Condition(() => { return player.position.x < transform.position.x; }, Stops.SELF,
                        new Action(() =>
                        {
                            // move left
                            rb.velocity = Vector2.left * chaseSpeedMultiplier;
                        })
                    )
                ),
                new Succeeder(
                    new Condition(() => { return player.position.x > transform.position.x; }, Stops.SELF,
                        new Action(() =>
                        {
                            // move right
                            rb.velocity = Vector2.right * chaseSpeedMultiplier;
                        })
                    )
                )
            )
        );
    }

    private Node MeleeAttack()
    {
        return new Cooldown(attackDelay, attackDelay,
        new Sequence(
            new Action(() =>
            {
                animator.SetBool("Movement", false);
                rb.velocity = Vector2.zero;
                animator.SetBool("Attack", true);
                player.GetComponent<PlayerHealth>().ApplyDamage(meleeDamage);
            }),
            new WaitForCondition(() => {
                return animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
            }, 
            new WaitForCondition(() => {
                return !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
            }, new Action(() => {
                animator.SetBool("Attack", false);
            })))
        )
        );
    }

    private Node ProjectileAttack()
    {
        return new Cooldown(attackDelay, attackDelay,
        new Sequence(
            new Action(() =>
            {
                animator.SetBool("Attack", true);
                Rigidbody2D projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
                projectile.GetComponent<Projectile2D>().SetTargetTag("Player");
                projectile.GetComponent<Projectile2D>().SetProjectileDamage(projectileDamage);
                projectile.velocity = (GameObject.Find("Player").transform.position - transform.position).normalized * projectileVelocity;
            }),
            new WaitForCondition(() =>
            {
                return animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
            },
            new WaitForCondition(() =>
            {
                return !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
            }, new Action(() =>
            {
                animator.SetBool("Attack", false);
            })))
        )
        );
    }

    // private IEnumerator HawkAirPatrol()
    // {
    //     animator.SetBool("Flying", true);
    //     //fly up
    //     float height = worldFlightCurve.Evaluate(transform.position.x);
    //     rb.velocity = Vector2.zero;
    //     while (transform.position.y < height)
    //     {
    //         rb.AddRelativeForce(Vector2.up / 4, ForceMode2D.Impulse);
    //         yield return new WaitForFixedUpdate();
    //     }

    //     float time = Time.time;
    //     while (Time.time - time < patrolChangeTime)
    //     {
    //         if (transform.position.y < worldFlightCurve.Evaluate(transform.position.x))
    //         {
    //             rb.MovePosition(new Vector2(transform.position.x, worldFlightCurve.Evaluate(transform.position.x)));
    //         }

    //         yield return new WaitForFixedUpdate();

    //         if (leftWalk)
    //         {
    //             rb.velocity = Vector2.left * 3;
    //             sr.flipX = true;
    //             // raycast left
    //             RaycastHit2D r = Physics2D.Raycast(transform.position, (transform.position + (Quaternion.Euler(0, 0, -attackAngle) * Vector2.down)) - transform.position, attackDistance, ~(1 << LayerMask.NameToLayer("Enemy")));
    //             Debug.DrawRay(transform.position, (transform.position + (Quaternion.Euler(0, 0, -attackAngle) * Vector2.down)) - transform.position);
    //             if (r.collider != null && r.transform.CompareTag("Player"))
    //             {
    //                 yield return HawkAirAttack(r.collider);
    //                 // fly back up
    //                 rb.velocity = Vector2.zero;
    //                 while (transform.position.y < worldFlightCurve.Evaluate(transform.position.x))
    //                 {
    //                     rb.AddRelativeForce(Vector2.up / 4, ForceMode2D.Impulse);
    //                     yield return new WaitForFixedUpdate();
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             rb.velocity = Vector2.right * 3;
    //             sr.flipX = false;
    //             // raycast right
    //             RaycastHit2D r = Physics2D.Raycast(transform.position, (transform.position + (Quaternion.Euler(0, 0, attackAngle) * Vector2.down)) - transform.position, attackDistance, ~(1 << LayerMask.NameToLayer("Enemy")));
    //             Debug.DrawRay(transform.position, (transform.position + (Quaternion.Euler(0, 0, attackAngle) * Vector2.down)) - transform.position);
    //             if (r.collider != null && r.transform.CompareTag("Player"))
    //             {
    //                 yield return HawkAirAttack(r.collider);
    //                 // fly back up
    //                 rb.velocity = Vector2.zero;
    //                 while (transform.position.y < worldFlightCurve.Evaluate(transform.position.x))
    //                 {
    //                     rb.AddRelativeForce(Vector2.up / 4, ForceMode2D.Impulse);
    //                     yield return new WaitForFixedUpdate();
    //                 }
    //             }
    //         }

    //         animator.SetBool("Movement", Mathf.Abs(rb.velocity.x) > 0.01f);

    //         if (transform.position.x < patrolLeftX)
    //             leftWalk = false;
    //         else if (transform.position.x > patrolRightX)
    //             leftWalk = true;

    //         yield return new WaitForFixedUpdate();
    //     }

    //     // fly down
    //     rb.AddRelativeForce(Vector2.down / 4, ForceMode2D.Impulse);
    //     yield return new WaitForFixedUpdate();
    //     yield return new WaitUntil(() => Mathf.Abs(rb.velocity.y) <= Mathf.Epsilon);
    //     animator.SetBool("Flying", false);
    // }

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
            rb.bodyType = RigidbodyType2D.Static;
            collider.enabled = false;
        }
    }

    void NormalizeSlope()
    {
        // Attempt vertical normalization
        if (isGrounded)
        {
            Vector2 offset = rb.velocity.normalized.x > 0 ? Vector2.right : Vector2.left;
            offset *= 0.15f;
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + offset, -Vector2.up, 5f, whatIsGround);
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
