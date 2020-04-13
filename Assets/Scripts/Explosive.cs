using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [SerializeField] private float radius = 10;
    [SerializeField] private int damage = 25;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private bool instantDeath = false;

    private bool deployable = false;

    public void StartTimer(float duration)
    {
        StartCoroutine(Timer(duration));
    }

    public void StartDeployable()
    {
        RaycastHit hit;
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hit, Mathf.Infinity,
            LayerMask.NameToLayer("Player"), QueryTriggerInteraction.Ignore))
        {
            transform.parent.position = hit.point;
        }
        deployable = true;
    }

    private IEnumerator Timer(float duration)
    {
        yield return new WaitForSeconds(duration);
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (var c in colliders)
        {
            if (c.GetComponent<Enemy>())
            {
                if (!instantDeath)
                    c.GetComponent<Enemy>().ApplyDamage(damage);
                else
                    c.GetComponent<Enemy>().InstantDeath(true);
            }
        }

        Transform t = Instantiate(explosionPrefab, transform.position, Quaternion.identity).transform;
        t.localScale = Vector3.one * radius;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (deployable && other.CompareTag("Enemy"))
        {
            if (!instantDeath)
                other.GetComponent<Enemy>().ApplyDamage(damage);
            else
                other.GetComponent<Enemy>().InstantDeath(true);

            Transform t = Instantiate(explosionPrefab, transform.position, Quaternion.identity).transform;
            t.localScale = Vector3.one * radius;
            Destroy(transform.parent.gameObject);
        }
    }
}
