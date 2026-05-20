using UnityEngine;

public class CompositeBossController : MonoBehaviour
{
    [Header("Main Boss")]
    [SerializeField] private EnemyHealth mainBossHealth;

    [Header("Linked Enemies")]
    [SerializeField] private EnemyHealth[] linkedEnemies;

    [Header("Settings")]
    [SerializeField] private bool makeBossInvulnerableOnStart = true;

    private int aliveLinkedEnemies;

    private void Awake()
    {
        if (mainBossHealth == null)
            mainBossHealth = GetComponent<EnemyHealth>();

        aliveLinkedEnemies = 0;

        foreach (EnemyHealth enemy in linkedEnemies)
        {
            if (enemy == null)
                continue;

            aliveLinkedEnemies++;
            enemy.OnDied += HandleLinkedEnemyDied;
        }

        if (makeBossInvulnerableOnStart && aliveLinkedEnemies > 0)
            mainBossHealth.SetInvulnerable(true);
    }

    private void OnDestroy()
    {
        foreach (EnemyHealth enemy in linkedEnemies)
        {
            if (enemy != null)
                enemy.OnDied -= HandleLinkedEnemyDied;
        }
    }

    private void HandleLinkedEnemyDied(EnemyHealth deadEnemy)
    {
        aliveLinkedEnemies--;

        if (aliveLinkedEnemies <= 0)
            UnlockBoss();
    }

    private void UnlockBoss()
    {
        if (mainBossHealth != null)
            mainBossHealth.SetInvulnerable(false);
    }
}