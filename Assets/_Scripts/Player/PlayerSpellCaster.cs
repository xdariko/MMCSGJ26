using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellCaster : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Damage")]
    [SerializeField] private float damage = 1f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Beam")]
    [SerializeField] private SpellBeam beamPrefab;

    private EnemyHealth currentTarget;
    private SpellBeam currentBeam;

    private float cooldownTimer;

    private void Update()
    {
        FindTarget();
        HandleAttack();
    }

    private void FindTarget()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                detectionRadius,
                enemyLayer);

        if (hits.Length == 0)
        {
            currentTarget = null;

            if (currentBeam != null)
                Destroy(currentBeam.gameObject);

            return;
        }

        float closestDistance = Mathf.Infinity;
        EnemyHealth closestEnemy = null;

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy =
                hit.GetComponent<EnemyHealth>();

            if (enemy == null)
                continue;

            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        currentTarget = closestEnemy;

        if (currentTarget != null && currentBeam == null)
        {
            currentBeam = Instantiate(
                beamPrefab);

            currentBeam.Initialize(
                transform,
                currentTarget.transform);
        }
    }

    private void HandleAttack()
    {
        if (currentTarget == null)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        cooldownTimer = attackCooldown;

        currentTarget.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius);
    }
}