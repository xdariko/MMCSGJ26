using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.DespawnWithoutReward();
            return;
        }

        if (other.CompareTag("Projectile"))
        {
            Destroy(other.transform.root.gameObject);
            return;
        }

        if (other.CompareTag("Orb"))
        {
            Destroy(other.transform.root.gameObject);
            return;
        }
    }
}