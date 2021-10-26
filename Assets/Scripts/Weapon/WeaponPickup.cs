using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[RequireComponent(typeof (BoxCollider))]
[RequireComponent(typeof (Rigidbody))]
public class WeaponPickup : MonoBehaviour, ISaveable
{
    Dictionary<string, object> saveData = new Dictionary<string, object>();

    public GameObject itemPrefab;
    [Range(2, 10)]
    [SerializeField] private int inventorySlot = 2;
    public bool overridePickupAmmo = false;
    public int overrideAmmoAmount = 0;

    private bool hasBeenPickedUp = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GetComponent<Renderer>().isVisible)
        {
            if (FindObjectOfType<PlayerInventory>().TryAddItem(this))
            {
                saveData["hasBeenPickedUp"] = true;
                GameStateManager.Instance.StoreSaveData(GetDictionaryKey(), JsonConvert.SerializeObject(saveData));
                Destroy(gameObject);
            }
        }
    }

    public SaveableData GetObjectState()
    {
        saveData["hasBeenPickedUp"] = hasBeenPickedUp;
        var data = new SaveableData(GetDictionaryKey(), JsonConvert.SerializeObject(saveData));
        return data;
    }

    public void ApplyObjectState(string objectJson)
    {
        var state = JsonConvert.DeserializeObject<Dictionary<string, object>>(objectJson);

        hasBeenPickedUp = System.Convert.ToBoolean(state["hasBeenPickedUp"]);

        // switch this to enabled = false if performance issues arise
        if (hasBeenPickedUp)
            Destroy(gameObject);
    }

    public string GetDictionaryKey()
    {
        return $"{gameObject.GetInstanceID()}:{this.GetType().Name}";
    }
}
