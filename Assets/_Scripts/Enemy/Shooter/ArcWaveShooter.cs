using System.Collections;
using UnityEngine;

public class ArcWaveShooter : EnemyShooterBase
{
    [Header("Projectile Variants")]
    [SerializeField] private OrbProjectile redProjectilePrefab;
    [SerializeField] private OrbProjectile blueProjectilePrefab;

    [Header("Arc Settings")]
    [SerializeField] private int projectileCount = 5;

    [Tooltip("Общий угол волны. Например 90 = веер на 90 градусов, центр направлен в игрока.")]
    [SerializeField] private float arcAngle = 120f;

    [SerializeField] private float delayBetweenShots = 0.05f;

    [Header("Mixed Wave")]
    [SerializeField] private bool useMixedProjectiles = true;
    [SerializeField] private bool swapColorsEveryWave = true;

    [Header("Aim")]
    [Tooltip("Если true, направление на игрока фиксируется в момент начала атаки. Волна не будет доворачиваться за игроком во время выстрела.")]
    [SerializeField] private bool capturePlayerDirectionOnWaveStart = true;

    private int waveIndex;

    protected override void Shoot()
    {
        StartCoroutine(ShootArc());
    }

    private IEnumerator ShootArc()
    {
        Vector2 baseDir = GetAimDirection();

        if (baseDir.sqrMagnitude <= 0.001f)
            baseDir = Vector2.left;

        baseDir.Normalize();

        bool swapped = swapColorsEveryWave && waveIndex % 2 == 1;

        int count = Mathf.Max(1, projectileCount);

        if (count == 1)
        {
            SpawnProjectile(GetProjectileForIndex(0, swapped), baseDir);
            waveIndex++;
            yield break;
        }

        float clampedArcAngle = Mathf.Clamp(arcAngle, 0f, 360f);
        float startAngle = -clampedArcAngle * 0.5f;
        float step = clampedArcAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;

            Vector2 dir =
                Quaternion.Euler(0f, 0f, angle) * baseDir;

            OrbProjectile prefab = GetProjectileForIndex(i, swapped);

            SpawnProjectile(prefab, dir.normalized);

            if (delayBetweenShots > 0f)
                yield return new WaitForSeconds(delayBetweenShots);
        }

        waveIndex++;
    }

    private Vector2 GetAimDirection()
    {
        if (capturePlayerDirectionOnWaveStart)
            return PlayerDirection();

        if (G.player != null)
            return ((Vector2)G.player.transform.position - (Vector2)transform.position).normalized;

        return PlayerDirection();
    }

    private OrbProjectile GetProjectileForIndex(int index, bool swapped)
    {
        if (!useMixedProjectiles)
            return projectilePrefab;

        bool useBlue = index % 2 == 1;

        if (swapped)
            useBlue = !useBlue;

        OrbProjectile prefab = useBlue
            ? blueProjectilePrefab
            : redProjectilePrefab;

        return prefab != null ? prefab : projectilePrefab;
    }
}