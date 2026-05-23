using System.Collections;
using UnityEngine;

public class ArcWaveShooter : EnemyShooterBase
{
    [Header("Projectile Variants")]
    [SerializeField] private OrbProjectile redProjectilePrefab;
    [SerializeField] private OrbProjectile blueProjectilePrefab;

    [Header("Arc Settings")]
    [SerializeField] private int projectileCount = 5;
    [SerializeField] private float arcAngle = 120f;
    [SerializeField] private float delayBetweenShots = 0.05f;

    [Header("Mixed Wave")]
    [SerializeField] private bool useMixedProjectiles = true;
    [SerializeField] private bool swapColorsEveryWave = true;

    [Header("Direction Fix")]
    [SerializeField] private bool invertDirection = true;

    private int waveIndex;

    protected override void Shoot()
    {
        StartCoroutine(ShootArc());
    }

    private IEnumerator ShootArc()
    {
        Vector2 baseDir = PlayerDirection();

        if (invertDirection)
            baseDir = -baseDir;

        if (baseDir.sqrMagnitude <= 0.001f)
            baseDir = Vector2.left;

        baseDir.Normalize();

        bool swapped = swapColorsEveryWave && waveIndex % 2 == 1;

        if (projectileCount <= 1)
        {
            SpawnProjectile(GetProjectileForIndex(0, swapped), baseDir);
            waveIndex++;
            yield break;
        }

        float startAngle = -arcAngle * 0.5f;
        float step = arcAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + step * i;

            Vector2 dir =
                Quaternion.Euler(0f, 0f, angle) * baseDir;

            OrbProjectile prefab = GetProjectileForIndex(i, swapped);

            SpawnProjectile(prefab, dir.normalized);

            yield return new WaitForSeconds(delayBetweenShots);
        }

        waveIndex++;
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