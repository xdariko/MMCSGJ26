using System.Collections;
using UnityEngine;

public class BossOppositeWaveShooter : EnemyShooterBase
{
    [Header("Boss Pattern")]
    [SerializeField] private float attackDuration = 3f;
    [SerializeField] private float waitAfterAttack = 1.5f;
    [SerializeField] private float delayBetweenPairs = 0.12f;

    [Header("Stop Before Shooting")]
    [SerializeField] private float stopBeforeAttackTime = 0.35f;
    [SerializeField] private float stopAfterAttackTime = 0.2f;

    [Header("Rotation")]
    [SerializeField] private float angleStep = 12f;
    [SerializeField] private bool clockwise = true;

    [Header("Start Direction")]
    [SerializeField] private bool aimFirstPairAtPlayer = true;
    [SerializeField] private float startAngle = 0f;

    [Header("Spawn Offset")]
    [SerializeField] private float projectileSpawnOffset = 0.25f;

    private bool isAttacking;
    private float waitTimer;

    protected override void Update()
    {
        if (G.IsPaused || G.IsPlayerDead)
            return;

        if (isAttacking)
            return;

        waitTimer -= Time.deltaTime;

        if (waitTimer > 0f)
            return;

        StartCoroutine(AttackRoutine());
    }

    protected override void Shoot()
    {
        // Босс использует свой AttackRoutine
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (brain != null)
            brain.LockMovement();

        yield return new WaitForSeconds(stopBeforeAttackTime);

        float elapsed = 0f;
        float currentAngle = GetInitialAngle();

        while (elapsed < attackDuration)
        {
            FireOppositePair(currentAngle);

            float stepDirection = clockwise ? -1f : 1f;
            currentAngle += angleStep * stepDirection;

            yield return new WaitForSeconds(delayBetweenPairs);

            elapsed += delayBetweenPairs;
        }

        yield return new WaitForSeconds(stopAfterAttackTime);

        if (brain != null)
            brain.UnlockMovement();

        waitTimer = waitAfterAttack;
        isAttacking = false;
    }

    private float GetInitialAngle()
    {
        if (!aimFirstPairAtPlayer)
            return startAngle;

        Vector2 dir = PlayerDirection();

        if (dir.sqrMagnitude <= 0.001f)
            return startAngle;

        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    private void FireOppositePair(float angle)
    {
        Vector2 firstDirection =
            Quaternion.Euler(0f, 0f, angle) * Vector2.right;

        Vector2 secondDirection = -firstDirection;

        SpawnProjectileFromDirection(firstDirection);
        SpawnProjectileFromDirection(secondDirection);
    }

    private void SpawnProjectileFromDirection(Vector2 direction)
    {
        Vector3 spawnPosition =
            transform.position +
            (Vector3)(direction.normalized * projectileSpawnOffset);

        SpawnProjectile(direction, spawnPosition);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        isAttacking = false;

        if (brain != null)
            brain.UnlockMovement();
    }
}