using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;

    private float currentHealth;
    private EnemyDrop drop;

    private void Awake()
    {
        currentHealth = maxHealth;
        drop = GetComponent<EnemyDrop>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (drop != null)
            drop.DropLoot();

        FindFirstObjectByType<WaveDirector>().NotifyDeath(gameObject);

        Destroy(gameObject);
    }
}