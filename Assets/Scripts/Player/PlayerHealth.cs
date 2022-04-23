using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

public class PlayerHealth : Saveable
{
    public static PlayerHealth Instance;

    [HideInInspector] public UnityEvent onDeath = new UnityEvent();
    [HideInInspector] public IntEvent onDamaged = new IntEvent(); // invokes with current health amount
    [HideInInspector] public IntEvent onHealed = new IntEvent(); // invokes with current health amount
    
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private DeathCanvas deathCanvas = null;

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
        
        health = Mathf.Clamp(health - amount, 0, maxHealth);

        if (health <= 0)
        {
            onDeath.Invoke();
            deathCanvas?.BeginTransition();
        }
        else
        {
            onDamaged.Invoke(health);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>Amount of health healed</returns>
    public int ApplyHealth(int amount)
    {
        if (health == maxHealth)
            return 0;

        int previousHealth = health;
        health = Mathf.Clamp(health + amount, 0, maxHealth);

        onHealed.Invoke(health);
        return health - previousHealth;
    }

    public void InstantDeath()
    {
        health = 0;
        onDamaged.Invoke(health);
        onDeath.Invoke();
        deathCanvas?.BeginTransition();
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public override SaveableData GetObjectState()
    {
        saveData["health"] = health;
        var data = new SaveableData(GetDictionaryKey(), JsonConvert.SerializeObject(saveData));
        return data;
    }

    public override void ApplyObjectState(string objectJson)
    {
        var state = JsonConvert.DeserializeObject<Dictionary<string, object>>(objectJson);

        health = System.Convert.ToInt32(state["health"]);
        // update health UI
        // TODO: prevent damaged effects when loading health
        onDamaged.Invoke(health);
    }
}
