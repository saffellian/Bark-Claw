using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillBoundary2D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.transform.CompareTag("Player"))
        {
            return;
        }

        other.GetComponent<PlayerHealth>().InstantDeath();
    }
}
