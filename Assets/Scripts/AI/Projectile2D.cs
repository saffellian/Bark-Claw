using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile2D : MonoBehaviour
{
    private int damage = 0;
    private Rigidbody2D rb;
    private string targetTag = String.Empty;

    public void SetProjectileDamage(int damageAmount)
    {
        damage = damageAmount;
    }

    public void SetTargetTag(string tag)
    {
        targetTag = tag;
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
        if (targetTag != other.tag)
            return;

        if (other.transform.CompareTag("Enemy")) // implement logic during 2D AI order
        {
            other.GetComponent<Enemy2D>().ApplyDamage(damage);
        }
        else if (other.transform.CompareTag("Player"))
        {
            PlayerHealth.Instance.ApplyDamage(damage);
        }
        
        Destroy(gameObject);
    }
}
