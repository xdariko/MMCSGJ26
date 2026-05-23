using System.Collections;
using UnityEngine;

public class BossFinalPatternShooter : EnemyShooterBase
{
    [Header("Projectile Variants")]
    [SerializeField] private OrbProjectile redProjectilePrefab;
    [SerializeField] private OrbProjectile blueProjectilePrefab;

    [Header("Circle Phase")]
    [SerializeField] private int circlePairs = 3;
    [SerializeField] private int circleProjectileCount = 18;
    [SerializeField] private float delayBetweenCircles = 0.45f;
    [SerializeField] private float circleAngleOffsetPerShot = 10f;

    [Header("Aimed Red Burst")]
    [SerializeField] private int aimedProjectileCount = 12;
    [SerializeField] private float aimedSpreadAngle = 38f;
    [SerializeField] private float delayBetweenAimedShots = 0.06f;
    [SerializeField] private float aimedRandomAngle = 8f;

    [Header("Pauses")]
    [SerializeField] private float stopBeforePatternTime = 0.45f;
    [SerializeField] private float pauseBeforeAimedBurst = 0.85f;
    [SerializeField] private float pauseAfterFullPattern = 1.8f;

    [Header("Spawn Offset")]
    [SerializeField] private float projectileSpawnOffset = 0.3f;

    private bool isAttacking;
    private float circleRotationOffset;

    protected override void Update()
    {
        if (G.IsPaused || G.IsPlayerDead)
            return;

        if (isAttacking)
            return;

        StartCoroutine(AttackRoutine());
    }

    protected override void Shoot()
    {
        // Финальный босс использует AttackRoutine.
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (brain != null)
            brain.LockMovement();

        yield return new WaitForSeconds(stopBeforePatternTime);

        for (int i = 0; i < circlePairs; i++)
        {
            FireCircle(redProjectilePrefab);
            yield return new WaitForSeconds(delayBetweenCircles);

            FireCircle(blueProjectilePrefab);
            yield return new WaitForSeconds(delayBetweenCircles);
        }

        yield return new WaitForSeconds(pauseBeforeAimedBurst);

        Vector3 capturedPlayerPosition = player != null
            ? player.position
            : transform.position + Vector3.left;

        yield return StartCoroutine(FireAimedRandomRedBurst(capturedPlayerPosition));

        yield return new WaitForSeconds(pauseAfterFullPattern);

        if (brain != null)
            brain.UnlockMovement();

        isAttacking = false;
    }

    private void FireCircle(OrbProjectile projectile)
    {
        if (projectile == null)
            projectile = projectilePrefab;

        if (projectile == null)
            return;

        int count = Mathf.Max(1, circleProjectileCount);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = circleRotationOffset + step * i;

            Vector2 direction =
                Quaternion.Euler(0f, 0f, angle) * Vector2.right;

            SpawnProjectileFromDirection(projectile, direction);
        }

        circleRotationOffset += circleAngleOffsetPerShot;
    }

    private IEnumerator FireAimedRandomRedBurst(Vector3 capturedPlayerPosition)
    {
        OrbProjectile projectile = redProjectilePrefab != null
            ? redProjectilePrefab
            : projectilePrefab;

        if (projectile == null)
            yield break;

        Vector2 baseDirection =
            ((Vector2)capturedPlayerPosition - (Vector2)transform.position).normalized;

        if (baseDirection.sqrMagnitude <= 0.001f)
            baseDirection = Vector2.left;

        for (int i = 0; i < aimedProjectileCount; i++)
        {
            float t = aimedProjectileCount <= 1
                ? 0.5f
                : (float)i / (aimedProjectileCount - 1);

            float spreadAngle =
                Mathf.Lerp(-aimedSpreadAngle * 0.5f, aimedSpreadAngle * 0.5f, t);

            float randomAngle =
                Random.Range(-aimedRandomAngle, aimedRandomAngle);

            Vector2 direction =
                Quaternion.Euler(0f, 0f, spreadAngle + randomAngle) * baseDirection;

            SpawnProjectileFromDirection(projectile, direction.normalized);

            yield return new WaitForSeconds(delayBetweenAimedShots);
        }
    }

    private void SpawnProjectileFromDirection(OrbProjectile projectile, Vector2 direction)
    {
        Vector3 spawnPosition =
            transform.position +
            (Vector3)(direction.normalized * projectileSpawnOffset);

        SpawnProjectile(projectile, direction, spawnPosition);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        isAttacking = false;

        if (brain != null)
            brain.UnlockMovement();
    }
}