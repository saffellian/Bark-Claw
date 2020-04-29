using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Serializable]
    struct ItemDrop
    {
        public GameObject item;
        [Range(1, 100)]
        public int dropChance;
    }

    public enum EnemyType
    {
        Melee,
        Projectile
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
        Area
    }

    [Header("General")]
    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    [SerializeField] private int health;
    [SerializeField] private bool normalDeathOnly = false;
    [Header("Attack")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float chaseDistance;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackStrafeDistance, attackStrafeDepth, attackStrafeWidth;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawn;
    [Header("Patrol")] 
    [SerializeField] private PatrolType patrolType = PatrolType.Waypoints;
    [SerializeField] private MeshFilter patrolRegion;
    [SerializeField] private List<Transform> patrolPoints;
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
    private DeathType deathType = DeathType.Normal;

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

        StartCoroutine(BrainLogic());
        StartCoroutine(PatrolAudio());
    }

    private IEnumerator BrainLogic()
    {
        while (gameObject.activeInHierarchy)
        {
            if (patrolType == PatrolType.Area)
                yield return AreaPatrol();
            else if (patrolType == PatrolType.Waypoints)
                yield return PointPatrol();

            if (enemyType == EnemyType.Projectile)
                yield return ProjectileAttack();
            else if (enemyType == EnemyType.Melee)
                yield return MeleeAttack();
        }
    }

    private IEnumerator AreaPatrol()
    {
        animator.SetTrigger("Walk");
        agent.speed = patrolSpeed;
        RaycastHit hit;
        bool visible = false;
        do
        {
            agent.destination = new Vector3(Random.Range(patrolMin.x, patrolMax.x), patrolRegion.transform.position.y, Random.Range(patrolMin.z, patrolMax.z));
            yield return new WaitUntil(() => (!agent.pathPending && agent.remainingDistance < 0.5f));

            if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
            {
                Ray ray = new Ray(transform.position, (player.transform.position - transform.position));
                UnityEngine.Debug.DrawRay(transform.position, (player.transform.position - transform.position) * 20);
                if (Physics.Raycast(ray, out hit, attackDistance))
                {
                    visible = hit.transform.CompareTag("Player");
                }
            }

            yield return new WaitForEndOfFrame();
        } while (!visible);
    }

    private IEnumerator PointPatrol()
    {
        animator.SetTrigger("Walk");
        int i = 0;
        agent.speed = patrolSpeed;
        RaycastHit hit;
        bool visible = false;
        do
        {
            agent.destination = patrolPoints[i].position;
            i = (i + 1) % patrolPoints.Count;
            yield return new WaitUntil(() => (!agent.pathPending && agent.remainingDistance < 0.5f));

            if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
            {
                Ray ray = new Ray(transform.position, (player.transform.position - transform.position));
                UnityEngine.Debug.DrawRay(transform.position, (player.transform.position - transform.position) * 20);
                if (Physics.Raycast(ray, out hit, chaseDistance))
                {
                    visible = hit.transform.CompareTag("Player");
                }
            }

            yield return new WaitForEndOfFrame();
        } while (!visible);
    }

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

    private IEnumerator ProjectileAttack()
    {
        audioSource.clip = attackSound;
        Vector3 forwardDir, horizontalDir, centerPos;
        while (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
        {
            forwardDir = (player.transform.position - transform.position).normalized;
            forwardDir *= attackStrafeDepth;
            forwardDir = new Vector3(forwardDir.x, 0, forwardDir.z);
            horizontalDir = Quaternion.Euler(0, 90, 0) * (forwardDir.normalized * attackStrafeWidth);
            horizontalDir = new Vector3(horizontalDir.x, 0, horizontalDir.z);
            centerPos = player.transform.position + ((transform.position - player.transform.position).normalized * attackStrafeDistance);
            centerPos = new Vector3(centerPos.x, 0, centerPos.z);
            Vector3[] points = { centerPos + forwardDir + horizontalDir, centerPos - forwardDir - horizontalDir };

            Vector3 samplePoint = new Vector3(Random.Range(points[0].x, points[1].x), 0, Random.Range(points[0].z, points[1].z));
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(samplePoint, path) && path.status == NavMeshPathStatus.PathComplete) // check if agent can get to point
            {
                agent.destination = samplePoint;
            }
            else if (agent.CalculatePath(Vector3.LerpUnclamped(samplePoint, player.transform.position, 2), path) && path.status == NavMeshPathStatus.PathComplete) // test opposite direction
            {
                agent.destination = Vector3.LerpUnclamped(samplePoint, player.transform.position, 2);
            }
            else // fall back to finding a random point in a circular region near the squirrel
            {
                Vector2 rand;
                do
                {
                    rand = Random.insideUnitCircle * attackStrafeWidth;
                    samplePoint = new Vector3(rand.x, 0, rand.y) + transform.position;
                    yield return new WaitForEndOfFrame();
                } while (!agent.CalculatePath(samplePoint, path) && path.status == NavMeshPathStatus.PathComplete);

                agent.destination = samplePoint;
            }

            yield return new WaitUntil(() => (!agent.pathPending && agent.remainingDistance < 0.5f));

            if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
            {
                agent.isStopped = true;
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.3f); // delay to line up animation
                // spawn and shoot acorn
                Rigidbody obj = Instantiate(projectile, projectileSpawn.position, Quaternion.identity).GetComponent<Rigidbody>();
                obj.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                obj.AddForce((GameObject.Find("FirstPersonCharacter").transform.position - transform.position).normalized * 20, ForceMode.Impulse);

                yield return new WaitForSeconds(attackDelay);
                agent.isStopped = false;
            }

            yield return new WaitForEndOfFrame();
        }

        agent.speed = patrolSpeed;
        audioSource.Stop();
        audioSource.clip = null;
    }

    private IEnumerator MeleeAttack()
    {
        agent.speed = attackSpeed;
        while (Vector3.Distance(transform.position, player.transform.position) < chaseDistance)
        {
            if (Vector3.Distance(transform.position, player.transform.position) > 1.75f)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(player.transform.position, out hit, 1f, NavMesh.AllAreas))
                {
                    agent.destination = Vector3.LerpUnclamped(player.transform.position, hit.position, 1.7f);
                    UnityEngine.Debug.DrawLine(agent.destination, agent.destination + Vector3.up, Color.red, 100, false);
                }
            }

            yield return new WaitUntil(() => Vector3.Distance(transform.position, player.transform.position) < 1.75f || !agent.hasPath || agent.isPathStale);

            if (Vector3.Distance(transform.position, player.transform.position) < 1.75f)
            {
                agent.isStopped = true;
                animator.SetTrigger("Attack");
                audioSource.PlayOneShot(attackSound);
                if (Physics.CheckSphere(transform.position, 2, 1 << LayerMask.NameToLayer("Player")))
                {
                    PlayerHealth.Instance.ApplyDamage(attackDamage);
                    yield return new WaitForSeconds(attackDelay); // delay between attacks
                }
            }
            agent.isStopped = false;
            yield return new WaitForEndOfFrame();
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;
    }

    private void PlayerDied()
    {
        StopAllCoroutines();
        agent.isStopped = true;
    }

    private IEnumerator Death()
    {
        GetComponent<Collider>().enabled = false;
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        animator.SetTrigger(deathType.ToString());
        yield return null;
    }

    private void DropItem()
    {
        if (itemDrops.Count > 0)
        {
            List<GameObject> potentialDrops = new List<GameObject>(itemDrops.Count);
            foreach (ItemDrop item in itemDrops)
            {
                if (Random.Range(1, 100) <= item.dropChance)
                {
                    potentialDrops.Add(item.item);
                }
            }

            if (potentialDrops.Count > 0)
            {
                GameObject drop = potentialDrops[Random.Range(0, potentialDrops.Count - 1)];
                Instantiate(drop, transform.position, Quaternion.identity);
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
            StopAllCoroutines();
            StartCoroutine(Death());
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
        StopAllCoroutines();
        if (!normalDeathOnly && explode)
            deathType = DeathType.Explosion;
        StartCoroutine(Death());
    }
}
