using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class SquirrelEnemy : MonoBehaviour, IEnemy
{
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
    [SerializeField] private float attackDelay;
    [SerializeField] private float attackStrafeDistance, attackStrafeDepth, attackStrafeWidth;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform projectileSpawn;
    [Header("Patrol")]
    [SerializeField] private MeshFilter patrolRegion;
    [SerializeField] private float patrolSpeed;
    [SerializeField] private List<AudioClip> patrolAudio;
    [Header("Death")] 
    [SerializeField] private AudioClip deathSound;

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
        player = GameObject.Find("FPSController");
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        patrolMin = patrolRegion.GetComponent<Renderer>().bounds.min;
        patrolMax = patrolRegion.GetComponent<Renderer>().bounds.max;
        StartCoroutine(BrainLogic());
        StartCoroutine(PatrolAudio());
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

    public IEnumerator Attack()
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
            Vector3[] points = {centerPos + forwardDir + horizontalDir, centerPos - forwardDir - horizontalDir};
            
            agent.destination = new Vector3(Random.Range(points[0].x, points[1].x), 0, Random.Range(points[0].z, points[1].z));
            yield return new WaitUntil(() => (!agent.pathPending && agent.remainingDistance < 0.5f));
            
            if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
            {
                agent.isStopped = true;
                animator.SetTrigger("Attack");
                yield return new WaitForSeconds(0.3f); // delay to line up animation
                // spawn and shoot acorn
                Rigidbody obj = Instantiate(projectile, projectileSpawn.position, Quaternion.identity).GetComponent<Rigidbody>();
                obj.GetComponent<Projectile>().SetSpawnFrom(gameObject);
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

    public IEnumerator Death()
    {
        yield return null;
        audioSource.Stop();
        audioSource.PlayOneShot(deathSound);
        animator.SetTrigger(deathType.ToString());
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"));
        
        //Destroy(gameObject);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 forwardDir, horizontalDir, centerPos;
        player = GameObject.Find("FPSController");
        forwardDir = (player.transform.position - transform.position).normalized;
        forwardDir *= attackStrafeDepth;
        forwardDir = new Vector3(forwardDir.x, 0, forwardDir.z);
        horizontalDir = Quaternion.Euler(0, 90, 0) * (forwardDir.normalized * attackStrafeWidth);
        horizontalDir = new Vector3(horizontalDir.x, 0, horizontalDir.z);
        centerPos = player.transform.position + ((transform.position - player.transform.position).normalized * attackStrafeDistance);
        centerPos = new Vector3(centerPos.x, 0, centerPos.z);
        Vector3[] points = {centerPos + forwardDir - horizontalDir, 
                            centerPos + forwardDir + horizontalDir, 
                            centerPos - forwardDir + horizontalDir,
                            centerPos - forwardDir - horizontalDir};
        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[3], points[2]);
        Gizmos.DrawLine(points[1], points[2]);
        Gizmos.DrawLine(points[0], points[3]);
    }
    #endif

    public void ApplyDamage(int amount)
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
