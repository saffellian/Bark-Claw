using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class VacuumEnemy : MonoBehaviour, IEnemy
{
    [Header("General")]
    [SerializeField] private int health;
    [Header("Attack")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float chaseDistance;
    [SerializeField] private int attackDamage = 5;
    [SerializeField] private float attackDelay;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private float attackSpeed;
    [Header("Patrol")]
    [SerializeField] private List<Transform> patrolPoints;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private AudioClip patrolSound;
    [Header("Death")] 
    [SerializeField] private AudioClip deathSound;

    private bool isPatroling = false;
    private GameObject player;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("FPSController");
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = patrolSound;
        audioSource.Play();
        animator = GetComponent<Animator>();
        StartCoroutine(BrainLogic());
    }
    
    public IEnumerator BrainLogic()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return Patrol();
            yield return Attack();
        }
    }

    public IEnumerator Patrol()
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
                Debug.DrawRay(transform.position, (player.transform.position - transform.position) * 20);
                if (Physics.Raycast(ray, out hit, chaseDistance))
                {
                    visible = hit.transform.CompareTag("Player");
                }
            }
            
            yield return new WaitForEndOfFrame();
        } while (!visible);
    }

    public IEnumerator Attack()
    {
        agent.speed = attackSpeed;
        while (Vector3.Distance(transform.position, player.transform.position) < chaseDistance)
        {
            if (Vector3.Distance(transform.position, player.transform.position) > 1.5f)
            {
                agent.destination = player.transform.position;
            }

            yield return new WaitUntil(() => !agent.hasPath || Vector3.Distance(transform.position, player.transform.position) < 1.5f);
            if (Vector3.Distance(transform.position, player.transform.position) < 1.5f)
            {
                // apply damage to player
                audioSource.PlayOneShot(attackSound);
                animator.SetTrigger("Attack");
                PlayerHealth.Instance.ApplyDamage(attackDamage);
            }
            yield return new WaitForSeconds(attackDelay); // delay between attacks
            yield return new WaitForEndOfFrame();
        }

        agent.speed = patrolSpeed;
    }

    public IEnumerator Death()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        animator.SetTrigger("Death");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"));
        Destroy(gameObject);
    }

    public void ApplyDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            agent.isStopped = true;
            StopAllCoroutines();
            StartCoroutine(Death());
        }
    }
}
