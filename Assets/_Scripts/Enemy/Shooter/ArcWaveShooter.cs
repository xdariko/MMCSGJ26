using System.Collections;
using UnityEngine;

public class ArcWaveShooter : EnemyShooterBase
{
    [Header("Arc Settings")]
    [SerializeField] private int projectileCount = 5;
    [SerializeField] private float arcAngle = 120f;
    [SerializeField] private float delayBetweenShots = 0.05f;

    [Header("Direction Fix")]
    [SerializeField] private bool invertDirection = true;

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

        if (projectileCount <= 1)
        {
            SpawnProjectile(baseDir);
            yield break;
        }

        float startAngle = -arcAngle * 0.5f;
        float step = arcAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + step * i;

            Vector2 dir =
                Quaternion.Euler(0f, 0f, angle) * baseDir;

            SpawnProjectile(dir.normalized);

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }
}