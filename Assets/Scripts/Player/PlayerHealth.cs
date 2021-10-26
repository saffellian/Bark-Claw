using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

[System.Serializable]
public class IntEvent : UnityEvent<int> { }

public class PlayerHealth : MonoBehaviour, ISaveable
{
    public static PlayerHealth Instance;
    
    Dictionary<string, object> saveData = new Dictionary<string, object>();

    [HideInInspector] public UnityEvent onDeath = new UnityEvent();
    [HideInInspector] public IntEvent onDamaged = new IntEvent(); // invokes with current health amount
    [HideInInspector] public IntEvent onHealed = new IntEvent(); // invokes with current health amount
    
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;

    private DeathCanvas deathCanvas;

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

        deathCanvas = FindObjectOfType<DeathCanvas>();
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

    public void ApplyHealth(int amount)
    {
        if (health == maxHealth)
            return;

        health = Mathf.Clamp(health + amount, 0, maxHealth);

        onHealed.Invoke(health);
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

    public SaveableData GetObjectState()
    {
        saveData["health"] = health;
        var data = new SaveableData(GetDictionaryKey(), JsonConvert.SerializeObject(saveData));
        return data;
    }

    public void ApplyObjectState(string objectJson)
    {
        var state = JsonConvert.DeserializeObject<Dictionary<string, object>>(objectJson);

        health = System.Convert.ToInt32(state["health"]);
        // update health UI
        // TODO: prevent damaged effects when loading health
        onDamaged.Invoke(health);
    }

    public string GetDictionaryKey()
    {
        return $"{gameObject.GetInstanceID()}:{this.GetType().Name}";
    }
}
