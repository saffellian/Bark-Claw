using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private GameObject _spawnFrom;

    public void SetSpawnFrom(GameObject obj)
    {
        _spawnFrom = obj;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            PlayerHealth.Instance.ApplyDamage(damage);
            damage = 0;
            Destroy(gameObject);
        }
        else if (other.gameObject != _spawnFrom)
        {
            // explode
            Destroy(gameObject);
        }
    }
}
