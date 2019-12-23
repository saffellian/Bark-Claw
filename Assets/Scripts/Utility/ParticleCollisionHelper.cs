using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionHelper : MonoBehaviour
{
    private Weapon weapon;

    // Start is called before the first frame update
    void Start()
    {
        weapon = transform.parent.GetComponent<Weapon>();
    }

    void OnParticleCollision(GameObject other)
    {
        weapon.ParticleCollision(other);
    }
}
