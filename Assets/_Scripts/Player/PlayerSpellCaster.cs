using System.Collections.Generic;
using UnityEngine;

public class PlayerSpellCaster : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Damage")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Beams")]
    [SerializeField] private SpellBeam beamPrefab;
    [SerializeField] private int baseBeamCount = 1;

    [Header("Hit FX")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float hitEffectLifetime = 1.5f;

    private readonly List<EnemyHealth> currentTargets = new();
    private readonly List<SpellBeam> currentBeams = new();

    private float cooldownTimer;

    private void Start()
    {
        PlayerStats.BaseDamage = baseDamage;
        PlayerStats.BaseBeamCount = baseBeamCount;
    }

    private void Update()
    {
        FindTargets();
        HandleAttack();
    }

    private void FindTargets()
    {
        int maxTargets = PlayerStats.BeamCount;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                detectionRadius,
                enemyLayer);

        List<EnemyHealth> candidates = new();

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy == null)
                enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy == null)
                continue;

            if (enemy.IsDead)
                continue;

            if (enemy.IsInvulnerable)
                continue;

            if (!candidates.Contains(enemy))
                candidates.Add(enemy);
        }

        candidates.Sort((a, b) =>
        {
            float distA = Vector2.SqrMagnitude(transform.position - a.transform.position);
            float distB = Vector2.SqrMagnitude(transform.position - b.transform.position);
            return distA.CompareTo(distB);
        });

        if (candidates.Count > maxTargets)
            candidates.RemoveRange(maxTargets, candidates.Count - maxTargets);

        for (int i = currentTargets.Count - 1; i >= 0; i--)
        {
            if (currentTargets[i] == null ||
                currentTargets[i].IsDead ||
                currentTargets[i].IsInvulnerable ||
                !candidates.Contains(currentTargets[i]))
            {
                if (i < currentBeams.Count && currentBeams[i] != null)
                    Destroy(currentBeams[i].gameObject);

                currentTargets.RemoveAt(i);

                if (i < currentBeams.Count)
                    currentBeams.RemoveAt(i);
            }
        }

        foreach (EnemyHealth enemy in candidates)
        {
            if (currentTargets.Contains(enemy))
                continue;

            currentTargets.Add(enemy);

            SpellBeam beam = Instantiate(beamPrefab);
            beam.Initialize(transform, enemy.transform);
            currentBeams.Add(beam);
        }
    }

    private void HandleAttack()
    {
        if (currentTargets.Count == 0)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer > 0f)
            return;

        cooldownTimer = attackCooldown;

        float baseDmg = PlayerStats.Damage;

        foreach (EnemyHealth target in currentTargets)
        {
            if (target == null)
                continue;

            if (target.IsDead)
                continue;

            if (target.IsInvulnerable)
                continue;

            bool isCrit = Random.value < PlayerStats.CritChance;
            float dmg = isCrit ? baseDmg * PlayerStats.CritMultiplier : baseDmg;

            target.TakeDamage(dmg, isCrit);
            SpawnHitEffect(target.transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRadius);
    }

    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab == null)
            return;

        GameObject fx = Instantiate(hitEffectPrefab, position, Quaternion.identity);

        if (hitEffectLifetime > 0f)
            Destroy(fx, hitEffectLifetime);
    }
}