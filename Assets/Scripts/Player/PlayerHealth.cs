using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

public class PlayerHealth : MonoBehaviour
{

    public static PlayerHealth Instance;
    
    [HideInInspector] public UnityEvent onDeath = new UnityEvent();
    [HideInInspector] public IntEvent onDamaged = new IntEvent(); // invokes with current health amount
    
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
            onDeath.Invoke();
            FindObjectOfType<DeathCanvas>()?.BeginTransition();
        }
        else
        {
            onDamaged.Invoke(health);
        }
    }

    public bool IsAlive()
    {
        return health > 0;
    }
}
