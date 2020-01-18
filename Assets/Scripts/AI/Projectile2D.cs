using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile2D : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private List<string> _noCollideTags = new List<string>();
    private List<GameObject> _noCollideObjects = new List<GameObject>();
    private Rigidbody2D rb;

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

    private void Start()
    {
        if (GetComponent<Rigidbody2D>())
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Update()
    {
        if (rb.velocity.x > 0)
            transform.localScale = Vector3.one;
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_noCollideTags.Contains(other.tag) || _noCollideObjects.Contains(other.gameObject))
            return;

        if (other.transform.CompareTag("Enemy")) // implement logic during 2D AI order
        {
            //other.GetComponent<Enemy>().ApplyDamage(damage);
            //damage = 0;
            Destroy(gameObject);
        }
        else
        {
            // explode
            Destroy(gameObject);
        }
    }
}
