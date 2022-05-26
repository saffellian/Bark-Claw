using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Component collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (collider.GetComponent<PlayerHealth>().ApplyHealth(healAmount) > 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
