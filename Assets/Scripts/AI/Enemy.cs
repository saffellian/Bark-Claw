using NPBehave;
using SensorToolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [System.Serializable]
    struct ItemDrop
    {
        [Tooltip("Prefab of item that will drop.")]
        public GameObject item;
        [Tooltip("Percent chance that this item drops when the enemy dies.")]
        [Range(1, 100)]
        public int dropChance;
    }

    public enum EnemyType
    {
        Melee,
        Projectile,
        Mixed
    }

    public enum DeathType
    {
        Normal,
        Acid,
        Explosion
    }

    public enum PatrolType
    {
        Waypoints,
        Area,
        None
    }

    [Header("General")]
    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    [SerializeField] private int health;
    [SerializeField] private bool normalDeathOnly = false;
    [Header("Attack")]
    [SerializeField] [EnemyType("Melee, Mixed")] private float meleeAttackDistance;
    [SerializeField] [EnemyType("Projectile, Mixed")] private float projectileAttackRange = 20f;
    [SerializeField] [EnemyType("Melee, Mixed")] private float chaseDistance;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackDelay;
    [SerializeField] [EnemyType("Projectile, Mixed")] private float attackStrafeDistance, attackStrafeDepth, attackStrafeWidth;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] [EnemyType("Projectile, Mixed")] private GameObject projectile;
    [SerializeField] [EnemyType("Projectile, Mixed")] private Transform projectileSpawn;
    [Header("Patrol")] 
    [SerializeField] private PatrolType patrolType = PatrolType.Waypoints;
    [SerializeField] [PatrolType("Area")] private MeshFilter patrolRegion;
    [SerializeField] [PatrolType("Waypoints")] private List<Transform> patrolPoints;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private List<AudioClip> patrolAudio;
    [Header("Death")]
    [SerializeField] private AudioClip deathSound;
    [Header("Drops")]
    [SerializeField] private List<ItemDrop> itemDrops;

    private bool isPatroling = false;
    private GameObject player;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private Vector3 patrolMin;
    private Vector3 patrolMax;
    private Queue<Transform> patrolPointQueue;
    private DeathType deathType = DeathType.Normal;
    private RangeSensor rangeSensor;
    private Root behaviorTree;
    private Blackboard blackboard;
    private Vector3 startPosition;
    private const float NAV_AGENT_TOLERANCE = 0.5f;

    void Start()
    {
        PlayerHealth.Instance.onDeath.AddListener(PlayerDied);
        player = GameObject.Find("FPSController");
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        if (patrolType == PatrolType.Area)
        {
            patrolMin = patrolRegion.GetComponent<Renderer>().bounds.min;
            patrolMax = patrolRegion.GetComponent<Renderer>().bounds.max;
        }
        else if (patrolType == PatrolType.Waypoints)
        {
            patrolPointQueue = new Queue<Transform>(patrolPoints);
        }
        
        startPosition = transform.position;

        rangeSensor = GetComponent<RangeSensor>();

        behaviorTree = InitializeBehaviorTree();
        blackboard = behaviorTree.Blackboard;
        behaviorTree.Start();

        // attach the debugger component if executed in editor (helps to debug in the inspector) 
#if UNITY_EDITOR
        Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
        debugger.BehaviorTree = behaviorTree;
#endif
    }

    private Root InitializeBehaviorTree()
    {
        return new Root(

            // service to update player distance sensor
            new Service(0.125f, UpdateBlackboard,
                    new Sequence(
                        new Succeeder(
                        new BlackboardCondition("isDead", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                            new Repeater(
                            new Sequence(
                                // jump out of node when player is in range
                                new Succeeder(
                                new BlackboardCondition("playerInRange", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART,
                                    new Sequence(
                                        new Action(() => {
                                            StartCoroutine("PatrolAudio");
                                        }),
                                        new Repeater( patrolType == PatrolType.Area ? AreaPatrol() : patrolType == PatrolType.Waypoints ? PointPatrol() : NonePatrol() )
                                    )
                                )),

                                new Action(() => {
                                    StopCoroutine("PatrolAudio");
                                }),

                                new Succeeder(new Repeater(
                                enemyType == EnemyType.Melee ? MeleeAttack() : enemyType == EnemyType.Projectile ? ProjectileAttack() : MixedAttack()
                                ))
                            )
                            )
                        )),

                        new Action(() =>
                        {
                            agent.enabled = false;
                            GetComponent<Collider>().enabled = false;
                            audioSource.Stop();
                            audioSource.PlayOneShot(deathSound);
                            animator.SetTrigger(deathType.ToString());
                            DropItem();
                            if (behaviorTree != null && behaviorTree.CurrentState == Node.State.ACTIVE)
                                behaviorTree.Stop();
                        })
                        { Label = "Dead" }
                    )
            )
        );
    }

    private void UpdateBlackboard()
    {
        behaviorTree.Blackboard["isDead"] = health <= 0;
        behaviorTree.Blackboard["playerInRange"] = rangeSensor.DetectedObjects.Count > 0
                                                && (enemyType == EnemyType.Melee ? Vector3.Distance(player.transform.position, startPosition) < chaseDistance : true);
        behaviorTree.Blackboard["playerDistance"] = Vector3.Distance(transform.position, player.transform.position);
        behaviorTree.Blackboard["canChase"] = rangeSensor.DetectedObjects.Count > 0 
                                            && (enemyType == EnemyType.Projectile ? true : Vector3.Distance(transform.position, player.transform.position) > meleeAttackDistance) 
                                            && Vector3.Distance(player.transform.position, startPosition) < (enemyType == EnemyType.Melee ? chaseDistance : projectileAttackRange + rangeSensor.SensorRange);
    }

    private Node NonePatrol()
    {        
        return new Sequence(
            // set patrol target
            new Action(() =>
            {
                agent.isStopped = false;
                behaviorTree.Blackboard["navTarget"] = startPosition;
            })
            { Label = "Set Destination" },
            // move to patrol target
            new NavMoveTo(agent, "navTarget", NAV_AGENT_TOLERANCE)
            { Label = "Waiting to reach destination" },

            new WaitUntilStopped()
        )
        { Label = "None Patrol" };
    }

    private Node AreaPatrol()
    {
        return new Sequence(
            // set patrol target
            new Action(() =>
            {
                agent.isStopped = false;
                behaviorTree.Blackboard["navTarget"] = new Vector3(Random.Range(patrolMin.x, patrolMax.x), patrolRegion.transform.position.y, Random.Range(patrolMin.z, patrolMax.z));
            })
            { Label = "Set Destination" },
         
            // move to patrol target
            new NavMoveTo(agent, "navTarget", NAV_AGENT_TOLERANCE)
            { Label = "Waiting to reach destination" }
        )
        { Label = "Area Patrol" };
    }

    private Node PointPatrol()
    {
        return new Sequence(
                               // set patrol target
                               new Action(() =>
                               {
                                   agent.isStopped = false;
                                   Transform next = patrolPointQueue.Dequeue();
                                   behaviorTree.Blackboard["navTarget"] = next;
                                   patrolPointQueue.Enqueue(next);

                               })
                               { Label = "Set Destination" },

                               // move to patrol target
                               new NavMoveTo(agent, "navTarget", NAV_AGENT_TOLERANCE)
                               { Label = "Waiting to reach destination" }
                           )
        { Label = "Point Patrol" };
    }

    private Node MeleeAttack()
    {
        return new Sequence(
            // chase until close enough to attack or out of range
            new Succeeder(
            new BlackboardCondition("canChase", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                    // set player as chase target
                    new Action((bool _shouldCancel) =>
                    {
                        if (!_shouldCancel)
                        {
                            agent.isStopped = false;
                            agent.SetDestination(player.transform.position);
                            return Action.Result.PROGRESS;
                        }
                        else
                        {
                            return Action.Result.FAILED;
                        }

                    })
                    { Label = "Chase Player" }
            )),

            // attack if in range
            new Succeeder(
            new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, meleeAttackDistance, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    // set player as chase target
                    new Action(() =>
                    {
                        agent.isStopped = true;
                        animator.SetTrigger("Attack");
                        audioSource.PlayOneShot(attackSound);
                    })
                    { Label = "Set Destination" },
                    // damage delay
                    new Wait(0.5f)
                    { Label = "Wait for attack delay duration" },
                    new Action(() =>
                    {
                        if (Physics.CheckSphere(transform.position, 2, 1 << LayerMask.NameToLayer("Player")))
                        {
                            PlayerHealth.Instance.ApplyDamage(attackDamage);
                        }
                    })
                    { Label = "Check and apply damage" },
                    // attack delay
                    new Wait(attackDelay)
                    { Label = "Wait for attack delay duration" }
                )
            ))
        );
    }

    private Node ProjectileAttack()
    {
        return new BlackboardCondition("canChase", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                new Sequence(
                    new Action(() =>
                    {
                        Vector3 forwardDir = (player.transform.position - transform.position).normalized;
                        forwardDir *= attackStrafeDepth;
                        forwardDir = new Vector3(forwardDir.x, 0, forwardDir.z);
                        Vector3 horizontalDir = Quaternion.Euler(0, 90, 0) * (forwardDir.normalized * attackStrafeWidth);
                        horizontalDir = new Vector3(horizontalDir.x, 0, horizontalDir.z);
                        Vector3 centerPos = player.transform.position + ((transform.position - player.transform.position).normalized * attackStrafeDistance);
                        centerPos = new Vector3(centerPos.x, 0, centerPos.z);
                        Vector3[] points = { centerPos + forwardDir + horizontalDir, centerPos - forwardDir - horizontalDir };
                        
                        behaviorTree.Blackboard["navTarget"] = new Vector3(Random.Range(points[0].x, points[1].x), 0, Random.Range(points[0].z, points[1].z));

                    })
                    { Label = "Find Point Near Self" },
                    new NavMoveTo(agent, "navTarget", NAV_AGENT_TOLERANCE),
                    new Action(() => {
                        agent.isStopped = true;
                        animator.SetTrigger("Attack");
                    })
                    { Label = "Stop Agent and Start Attack" },
                    new Wait(0.3f),
                    new Action(() => 
                    {
                        Rigidbody obj = Instantiate(projectile, projectileSpawn.position, Quaternion.identity).GetComponent<Rigidbody>();
                        obj.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                        obj.AddForce((GameObject.Find("FirstPersonCharacter").transform.position - transform.position).normalized * 20, ForceMode.Impulse);
                    })
                    { Label = "Fire Projectile" },
                    new Wait(attackDelay),
                    new Action(() => 
                    {
                        agent.isStopped = false;
                    })
                    { Label = "End Attack" }
                )                
        );
    }

    private Node MixedAttack()
    {
        return new BlackboardCondition("playerInRange", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
            new Sequence(
                new Succeeder(new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, 5f, Stops.IMMEDIATE_RESTART,
                    new Action((bool _shouldCancel) =>
                    {
                        if (!_shouldCancel)
                        {
                            if ((float)behaviorTree.Blackboard["playerDistance"] <= meleeAttackDistance)
                                return Action.Result.FAILED;
                            agent.isStopped = false;
                            agent.SetDestination(player.transform.position);
                            return Action.Result.PROGRESS;
                        }
                        else
                        {
                            return Action.Result.FAILED;
                        }
                    })
                    { Label = "Chase Player" }
                )),

                new Succeeder(new BlackboardCondition("playerDistance", Operator.IS_SMALLER_OR_EQUAL, meleeAttackDistance, Stops.NONE,
                    new Sequence(
                        new Action(() =>
                        {
                            agent.isStopped = true;
                            animator.SetTrigger("Attack");
                            audioSource.PlayOneShot(attackSound);
                        })
                        { Label = "Set Destination" },
                        new Wait(0.5f)
                        { Label = "Wait for attack delay duration" },
                        new Action(() =>
                        {
                            if (Physics.CheckSphere(transform.position, 2, 1 << LayerMask.NameToLayer("Player")))
                            {
                                PlayerHealth.Instance.ApplyDamage(attackDamage);
                            }
                        })
                        { Label = "Check and apply damage" },
                        new Wait(attackDelay)
                        { Label = "Wait for attack delay duration" }
                    )
                )),

                new Succeeder(new BlackboardCondition("playerDistance", Operator.IS_GREATER, 5f, Stops.IMMEDIATE_RESTART,
                    new Sequence(
                        new Action(() =>
                        {
                            Vector3 forwardDir = (player.transform.position - transform.position).normalized;
                            forwardDir *= attackStrafeDepth;
                            forwardDir = new Vector3(forwardDir.x, 0, forwardDir.z);
                            Vector3 horizontalDir = Quaternion.Euler(0, 90, 0) * (forwardDir.normalized * attackStrafeWidth);
                            horizontalDir = new Vector3(horizontalDir.x, 0, horizontalDir.z);
                            Vector3 centerPos = player.transform.position + ((transform.position - player.transform.position).normalized * attackStrafeDistance);
                            centerPos = new Vector3(centerPos.x, 0, centerPos.z);
                            Vector3[] points = { centerPos + forwardDir + horizontalDir, centerPos - forwardDir - horizontalDir };
                            behaviorTree.Blackboard["navTarget"] = new Vector3(Random.Range(points[0].x, points[1].x), 0, Random.Range(points[0].z, points[1].z));
                        })
                        { Label = "Find Point Near Self" },
                        new NavMoveTo(agent, "navTarget", NAV_AGENT_TOLERANCE),
                        new Action(() =>
                        {
                            agent.isStopped = true;
                            animator.SetTrigger("Attack");
                        })
                        { Label = "Stop Agent and Start Attack" },
                        new Wait(0.3f),
                        new Action(() =>
                        {
                            Rigidbody obj = Instantiate(projectile, projectileSpawn.position, Quaternion.identity).GetComponent<Rigidbody>();
                            obj.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                            obj.AddForce((GameObject.Find("FirstPersonCharacter").transform.position - transform.position).normalized * 20, ForceMode.Impulse);
                        })
                        { Label = "Fire Projectile" },
                        new Wait(attackDelay),
                        new Action(() =>
                        {
                            agent.isStopped = false;
                        })
                        { Label = "End Attack" }
                    )
                ))
            )
        );
    }

    /// <summary>
    /// Play audio associated with enemy patrol
    /// </summary>
    private IEnumerator PatrolAudio()
    {
        AudioClip clip;
        while (patrolAudio.Count > 0 && gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(Random.Range(2, 6));
            clip = patrolAudio[Random.Range(0, patrolAudio.Count)];
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayerDied()
    {
        agent.isStopped = true;
    }

    private void DropItem()
    {
        if (itemDrops.Count > 0)
        {
            foreach (ItemDrop item in itemDrops)
            {
                if (Random.Range(1, 101) <= item.dropChance)
                {
                    float offset = GetComponent<CapsuleCollider>().height / 2f;
                    Instantiate(item.item, transform.position - (Vector3.up * offset), Quaternion.identity);
                }
            }
        }
    }

    public void ApplyDamage(int amount, DeathType dType = DeathType.Normal)
    {
        if (health <= 0)
            return;

        if (!normalDeathOnly)
            deathType = dType;

        health -= amount;

        if (health <= 0)
        {
            agent.isStopped = true;
        }
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public void InstantDeath(bool explode)
    {
        if (health <= 0)
            return;

        health = 0;
        agent.isStopped = true;
        if (!normalDeathOnly && explode)
            deathType = DeathType.Explosion;
    }
}
