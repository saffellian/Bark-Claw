using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class CatEnemy : Enemy
{
    [Serializable]
    struct ItemDrop
    {
        public GameObject item;
        [Range(1,100)]
        public int dropChance;
    }

    private enum DeathType
    {
        Normal,
        Acid,
        Explosion
    }
    
    [Header("General")]
    [SerializeField] private int health;
    [Header("Attack")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float chaseDistance;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackDelay;
    [SerializeField] private AudioClip attackSound;
    [Header("Death")]
    [SerializeField] private AudioClip deathSound;
    [Header("Patrol")]
    [SerializeField] private MeshFilter patrolRegion;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private List<AudioClip> patrolAudio;
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
    

    // Start is called before the first frame update
    void Start()
    {
        PlayerHealth.Instance.playerDeath.AddListener(PlayerDied);
        player = GameObject.Find("FPSController");
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        patrolMin = patrolRegion.GetComponent<Renderer>().bounds.min;
        patrolMax = patrolRegion.GetComponent<Renderer>().bounds.max;
        StartCoroutine(BrainLogic());
        StartCoroutine(PatrolAudio());
    }
    
    protected override IEnumerator BrainLogic()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return Patrol();
            yield return Attack();
        }
    }

    protected override IEnumerator Patrol()
    {
        animator.SetTrigger("Walk");
        int i = 0;
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
                Debug.DrawRay(transform.position, (player.transform.position - transform.position) * 20);
                if (Physics.Raycast(ray, out hit, attackDistance))
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

    protected override IEnumerator Attack()
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
                    Debug.DrawLine(agent.destination, agent.destination + Vector3.up, Color.red, 100, false);
                }
            }

            yield return new WaitUntil(() => !agent.hasPath || agent.isPathStale);
            if (Vector3.Distance(transform.position, player.transform.position) < 1.75f)
            {
                agent.isStopped = true;
                animator.SetTrigger("Attack");
                audioSource.PlayOneShot(attackSound);
                if (Physics.CheckSphere(transform.position, 2, 1 << LayerMask.NameToLayer("Player")))
                {
                    PlayerHealth.Instance.ApplyDamage(attackDamage);
                }
            }
            yield return new WaitForSeconds(attackDelay); // delay between attacks
            agent.isStopped = false;
            yield return new WaitForEndOfFrame();
        }

        agent.isStopped = false;
        agent.speed = patrolSpeed;
    }

    protected override IEnumerator Death()
    {
        yield return null;
        animator.SetTrigger(deathType.ToString());
        audioSource.clip = deathSound;
        audioSource.loop = false;
        audioSource.Play();
        
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"));

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

        //Destroy(gameObject);
    }

    protected override void PlayerDied()
    {
        StopAllCoroutines();
        agent.isStopped = true;
    }

    public override void ApplyDamage(int amount)
    {
        if (health <= 0)
            return;
        
        health -= amount;

        if (health <= 0)
        {
            agent.isStopped = true;
            StopAllCoroutines();
            StartCoroutine(Death());
        }
    }
}
