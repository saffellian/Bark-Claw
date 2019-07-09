using UnityEngine;

[RequireComponent(typeof (BoxCollider))]
[RequireComponent(typeof (Rigidbody))]
public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GetComponent<Renderer>().isVisible)
        {
            other.transform.parent.GetComponent<PlayerInventory>().TryAddItem(itemPrefab, true);
            Destroy(gameObject);
        }
    }
}
