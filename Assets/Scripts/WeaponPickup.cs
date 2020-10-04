using UnityEngine;

[RequireComponent(typeof (BoxCollider))]
[RequireComponent(typeof (Rigidbody))]
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [Range(2, 10)]
    [SerializeField] private int inventorySlot = 2;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GetComponent<Renderer>().isVisible)
        {
            if (FindObjectOfType<PlayerInventory>().TryAddItem(itemPrefab, inventorySlot - 1))
                Destroy(gameObject);
        }
    }
}
