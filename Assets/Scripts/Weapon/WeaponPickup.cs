using UnityEngine;

[RequireComponent(typeof (BoxCollider))]
[RequireComponent(typeof (Rigidbody))]
public class WeaponPickup : MonoBehaviour
{
    public GameObject itemPrefab;
    [Range(2, 10)]
    [SerializeField] private int inventorySlot = 2;
    public bool overridePickupAmmo = false;
    public int overrideAmmoAmount = 0;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GetComponent<Renderer>().isVisible)
        {
            if (FindObjectOfType<PlayerInventory>().TryAddItem(this, inventorySlot - 1))
                Destroy(gameObject);
        }
    }
}
