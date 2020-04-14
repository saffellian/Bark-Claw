using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileType
    {
        Standard,
        Explosive
    }

    [SerializeField] private ProjectileType projectileType = ProjectileType.Standard;
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 5;
    [SerializeField] private float maxLifetime = 10;

    private List<string> _noCollideTags = new List<string>();
    private List<GameObject> _noCollideObjects = new List<GameObject>();

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    public void RegisterNoCollideObject(GameObject obj)
    {
        _noCollideObjects.Add(obj);
    }

    public void RemoveNoCollideTag(GameObject obj)
    {
        _noCollideObjects.RemoveAll(o => o == obj);
    }

    public void RegisterNoCollideTag(string tagName)
    {
        _noCollideTags.Add(tagName);
    }

    public void RemoveNoCollideTag(string tagName)
    {
        _noCollideTags.RemoveAll(t => t == tagName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_noCollideTags.Contains(other.tag) || _noCollideObjects.Contains(other.gameObject))
            return;

        if (other.transform.CompareTag("Player"))
        {
            PlayerHealth.Instance.ApplyDamage(damage);
        }
        else if (other.transform.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().ApplyDamage(damage);
        }

        damage = 0; // avoid multiple hits
        if (projectileType == ProjectileType.Explosive)
        {
            Transform t = Instantiate(explosionPrefab, transform.position, Quaternion.identity).transform;
            t.localScale = Vector3.one * explosionRadius;
            Collider[] colliders = Physics.OverlapSphere(t.position, explosionRadius, ~(LayerMask.GetMask(new string[] { "Enemy" })));
            if (colliders.Length > 0)
            {
                foreach (var c in colliders)
                {
                    if (c != other && c.GetComponent<Enemy>()) // prevent double damage
                        c.GetComponent<Enemy>().ApplyDamage(damage);
                }
            }
        }

        Destroy(gameObject);
    }
}
