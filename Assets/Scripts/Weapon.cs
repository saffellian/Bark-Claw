using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator), typeof(Rigidbody))]
public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        SemiAuto,
        Automatic,
        Shotgun,
        LayableExplosive,
        ThrowableExplosive,
        Fluid,
        AreaOfEffect
    }

    [SerializeField] private WeaponType weaponType = WeaponType.SemiAuto;
    [SerializeField] private float delayBetweenShots = 0.8f;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpread;
    [SerializeField] private float aoeRadius = 10;
    [SerializeField] private float throwForce = 5;
    [SerializeField] private float explosiveTimer = 3;
    [SerializeField] private int shotgunProjectileCount = 5;
    [SerializeField] private float projectileSpeed = 20;
    [SerializeField] private int fluidDamage = 1;
    [SerializeField] private List<Transform> projectileOrigins;

    private bool timerRunning = false;
    private Animator animator;
    private int i = 0;
    private ParticleSystem pSystem;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        if (weaponType == WeaponType.Fluid)
        {
            pSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (timerRunning)
            return;

        // semi-auto weapons
        if ((weaponType == WeaponType.SemiAuto && Input.GetButtonDown("Fire1")) ||
            (weaponType == WeaponType.Shotgun && Input.GetButtonDown("Fire1")) ||
            (weaponType == WeaponType.LayableExplosive && Input.GetButtonDown("Fire1")) ||
            (weaponType == WeaponType.ThrowableExplosive && Input.GetButtonDown("Fire1")) ||
            (weaponType == WeaponType.AreaOfEffect && Input.GetButtonDown("Fire1")))
        {
            animator.SetTrigger("Fire");
            StartCoroutine(FireTimer());
        }
        // automatic weapons
        else if ((weaponType == WeaponType.Automatic && Input.GetButton("Fire1")) ||
                 (weaponType == WeaponType.Fluid && Input.GetButton("Fire1")))
        {
            animator.SetBool("Fire", true);
            if (weaponType != WeaponType.Fluid)
                StartCoroutine(FireTimer());
            else if (!pSystem.isEmitting)
            {
                pSystem.Play();
            }
        }
        else
        {
            animator.SetBool("Fire", false);
            if (weaponType == WeaponType.Fluid)
            {
                pSystem.Stop();
            }
        }
    }
    private IEnumerator FireTimer()
    {
        timerRunning = true;
        yield return new WaitForSeconds(delayBetweenShots);
        timerRunning = false;
    }

    public void FireProjectile()
    {
        Rigidbody rb;
        switch (weaponType)
        {
            case WeaponType.SemiAuto:
                rb = Instantiate(projectile, projectileOrigins[0].position, projectile.transform.rotation).GetComponent<Rigidbody>();
                Debug.Log("fire");
                rb.velocity = projectileOrigins[0].forward * projectileSpeed;
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideTag("Player");
                break;
            case WeaponType.Automatic:
                rb = Instantiate(projectile, projectileOrigins[i].position, projectile.transform.rotation).GetComponent<Rigidbody>();
                rb.velocity = projectileOrigins[i].forward * projectileSpeed;
                i = (i + 1) % projectileOrigins.Count;
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideTag("Player");
                break;
            case WeaponType.Shotgun:
                for (int i = 0; i < shotgunProjectileCount; ++i)
                {
                    float xStray = Random.Range(-projectileSpread, projectileSpread);
                    float yStray = Random.Range(-projectileSpread, projectileSpread);
                    float zStray = Random.Range(-projectileSpread, projectileSpread);
                    rb = Instantiate(projectile, projectileOrigins[0].position, projectile.transform.rotation).GetComponent<Rigidbody>();
                    rb.transform.Rotate(xStray, yStray, zStray);
                    rb.velocity = projectileOrigins[0].rotation * rb.transform.forward * projectileSpeed;
                    rb.gameObject.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                    rb.gameObject.GetComponent<Projectile>().RegisterNoCollideTag("Player");
                    rb.gameObject.GetComponent<Projectile>().RegisterNoCollideTag("Projectile");
                }
                break;
            case WeaponType.LayableExplosive:
                Transform exp = Instantiate(projectile, transform.position, Quaternion.identity).transform.GetChild(0);
                exp.GetComponent<Explosive>().StartDeployable();
                break;
            case WeaponType.ThrowableExplosive:
                Rigidbody r = Instantiate(projectile, projectileOrigins[0].position, Quaternion.identity).GetComponent<Rigidbody>();
                r.velocity = projectileOrigins[0].forward * throwForce;
                r.GetComponent<Explosive>().StartTimer(explosiveTimer);
                break;
            case WeaponType.Fluid:

                break;
            case WeaponType.AreaOfEffect:
                Collider[] enemyColliders = Physics.OverlapSphere(transform.position, aoeRadius, ~LayerMask.NameToLayer("Default"), QueryTriggerInteraction.Collide);
                foreach (Collider c in enemyColliders)
                {
                    if (c.GetComponent<Enemy>() && !c.GetComponent<Enemy>().IsDead())
                    {
                        Debug.Log(c.name);
                        c.GetComponent<Enemy>().InstantDeath(true);
                    }
                }
                break;
        }
    }

    public void ParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().ApplyDamage(fluidDamage, Enemy.DeathType.Acid);
        }
    }
}
