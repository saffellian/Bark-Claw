using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;
    
    [HideInInspector] public UnityEvent playerDeath = new UnityEvent();
    
    [SerializeField] private int health = 100;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void ApplyDamage(int amount)
    {
        if (health <= 0)
            return;
        
        health -= amount;

        if (health <= 0)
        {
            playerDeath.Invoke();
            FindObjectOfType<DeathCanvas>().BeginTransition();
        }
    }

    public bool IsAlive()
    {
        return health > 0;
    }
}
