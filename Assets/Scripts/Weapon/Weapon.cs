using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Animator))]
public class Weapon : MonoBehaviour
{
    public static IntEvent onAmmoUpdate = new IntEvent();

    public enum WeaponType
    {
        Melee,
        SemiAuto,
        Automatic,
        Shotgun,
        LayableExplosive,
        ThrowableExplosive,
        Fluid,
        AreaOfEffect
    }

    private static float MIN_DROP_DELAY = 0.1f;

    [SerializeField] private WeaponType weaponType = WeaponType.SemiAuto;
    [SerializeField] private float delayBetweenShots = 0.8f;
    [SerializeField] private int ammo = 20;
    [SerializeField] private int maxAmmo = 20;
    [SerializeField] private bool dropOnEmpty = false;
    [SerializeField] private GameObject projectile;
    [SerializeField] private float projectileSpread;
    [SerializeField] private float aoeRadius = 10;
    [SerializeField] private float throwForce = 5;
    [SerializeField] private float explosiveTimer = 3;
    [SerializeField] private int shotgunProjectileCount = 5;
    [SerializeField] private float projectileSpeed = 20;
    [SerializeField] private int fluidDamage = 1;
    [SerializeField] private int meleeDamage = 5;
    [SerializeField] private List<Transform> projectileOrigins;

    private FirstPersonController fpController;
    private bool timerRunning = false;
    private Animator animator;
    private int i = 0;
    private ParticleSystem pSystem;
    private float fluidAmmo = 0;
    private bool fireButtonDown = false;
    private bool fireButtonHeld = false;
    private bool fireButtonPrev = false;
    private Vector3 meleeExtents;
    private Transform meleeTransform;

    // Start is called before the first frame update
    void Start()
    {
        fpController = FindObjectOfType<FirstPersonController>();
        animator = GetComponent<Animator>();

        if (weaponType == WeaponType.Fluid)
        {
            pSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
        }
        else if (weaponType == WeaponType.Melee)
        {
            GameObject obj = GameObject.Find("AttackCollider");
            meleeExtents = obj.GetComponent<Collider>().bounds.extents;
            meleeTransform = obj.transform;
            obj.SetActive(false);
        }
    }

    private void OnEnable()
    {
        timerRunning = false; // prevent stuck timer when switching weapons
    }

    public void WeaponSwapped()
    {
        if (weaponType == WeaponType.Melee)
            onAmmoUpdate.Invoke(-1);
        else
            onAmmoUpdate.Invoke(ammo);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire1") || Input.GetButtonDown("Fire1") || Input.GetAxis("Right Trigger") != 0)
        {
            if (fireButtonPrev == false)
                fireButtonDown = true;
            else
            {
                fireButtonDown = false;
                fireButtonHeld = true;
            }

            fireButtonPrev = true;
        } 
        else
        {
            fireButtonPrev = false;
            fireButtonDown = false;
            fireButtonHeld = false;
        }

        animator.SetBool("IsMoving", fpController.HasMovement());

        if (ammo <= 0 && weaponType != WeaponType.Melee)
        {
            animator.SetBool("Fire", false);
            if (weaponType == WeaponType.Fluid)
                pSystem.Stop();
            return;
        }

        if (timerRunning)
            return;
        
        // semi-auto weapons
        if ((weaponType == WeaponType.Melee && fireButtonDown) || 
            (weaponType == WeaponType.SemiAuto && fireButtonDown) ||
            (weaponType == WeaponType.Shotgun && fireButtonDown) ||
            (weaponType == WeaponType.LayableExplosive && fireButtonDown) ||
            (weaponType == WeaponType.ThrowableExplosive && fireButtonDown) ||
            (weaponType == WeaponType.AreaOfEffect && fireButtonDown))
        {
            animator.SetTrigger("Fire");
            StartCoroutine(FireTimer());
        }
        // automatic weapons
        else if ((weaponType == WeaponType.Automatic && fireButtonHeld) ||
                 (weaponType == WeaponType.Fluid && fireButtonHeld))
        {
            animator.SetBool("Fire", true);
            if (weaponType == WeaponType.Fluid)
            {
                fluidAmmo += Time.deltaTime;
                if (fluidAmmo >= 1)
                {
                    fluidAmmo = 0;
                    ammo--;
                    onAmmoUpdate.Invoke(ammo);
                }
            }

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
            if (weaponType == WeaponType.Fluid && pSystem.isPlaying)
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
            case WeaponType.Melee:
                Collider[] hits = Physics.OverlapBox(meleeTransform.position, meleeExtents);
                if (hits.Length > 0)
                {
                    foreach (Collider c in hits)
                    {
                        if (c.CompareTag("Enemy"))
                        {
                            c.GetComponent<Enemy>().ApplyDamage(meleeDamage);
                        }
                        else if (c.CompareTag("Fence"))
                        {
                            c.GetComponent<BreakableFence>().DamageFence(meleeDamage);
                        }
                    }
                }
                break;
            case WeaponType.SemiAuto:
                ammo--;
                onAmmoUpdate.Invoke(ammo);
                rb = Instantiate(projectile, projectileOrigins[0].position, projectile.transform.rotation).GetComponent<Rigidbody>();
                rb.velocity = projectileOrigins[0].forward * projectileSpeed;
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideTag("Player");
                break;
            case WeaponType.Automatic:
                ammo--;
                onAmmoUpdate.Invoke(ammo);
                rb = Instantiate(projectile, projectileOrigins[i].position, projectile.transform.rotation).GetComponent<Rigidbody>();
                rb.velocity = projectileOrigins[i].forward * projectileSpeed;
                i = (i + 1) % projectileOrigins.Count;
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideObject(gameObject);
                rb.gameObject.GetComponent<Projectile>().RegisterNoCollideTag("Player");
                break;
            case WeaponType.Shotgun:
                ammo--;
                onAmmoUpdate.Invoke(ammo);
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
                ammo--;
                onAmmoUpdate.Invoke(ammo);
                Transform exp = Instantiate(projectile, transform.position, Quaternion.identity).transform.GetChild(0);
                exp.GetComponent<Explosive>().StartDeployable();
                break;
            case WeaponType.ThrowableExplosive:
                ammo--;
                onAmmoUpdate.Invoke(ammo);
                Rigidbody r = Instantiate(projectile, projectileOrigins[0].position, Quaternion.identity).GetComponent<Rigidbody>();
                r.velocity = projectileOrigins[0].forward * throwForce;
                r.GetComponent<Explosive>().StartTimer(explosiveTimer);
                break;
            case WeaponType.Fluid:

                break;
            case WeaponType.AreaOfEffect:
                ammo--;
                onAmmoUpdate.Invoke(ammo);
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

        if (ammo <= 0 && dropOnEmpty)
        {
            StartCoroutine(RemoveItem());
        }
    }

    /// <summary>
    /// Drop item after its action animation has completed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RemoveItem()
    {
        yield return new WaitForSeconds(MIN_DROP_DELAY);
        yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(0).IsTag("Action"));
        FindObjectOfType<PlayerInventory>().RemoveItem(gameObject);
        Destroy(gameObject);
    }

    public void AddAmmo(int amount, bool isCurrentWeapon)
    {
        if (ammo + amount > maxAmmo)
        {
            ammo = maxAmmo;
        }
        else
        {
            ammo += amount;
        }
        
        if (isCurrentWeapon)
            onAmmoUpdate.Invoke(ammo);
    }

    public int GetAmmoAmount()
    {
        return ammo;
    }

    public void ParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().ApplyDamage(fluidDamage, Enemy.DeathType.Acid);
        }
    }
}
