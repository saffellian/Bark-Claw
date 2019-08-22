using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Player"))
        {
            PlayerHealth.Instance.ApplyDamage(damage);
        }
        else
        {
            // explode
            Destroy(gameObject);
        }
    }
}
