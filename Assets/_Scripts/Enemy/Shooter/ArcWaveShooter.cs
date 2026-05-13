using System.Collections;
using UnityEngine;

public class ArcWaveShooter : EnemyShooterBase
{
    [Header("Arc Settings")]
    [SerializeField] private int projectileCount = 8;
    [SerializeField] private float arcAngle = 120f;
    [SerializeField] private float delayBetweenShots = 0.05f;

    protected override void Shoot()
    {
        StartCoroutine(ShootArc());
    }

    private IEnumerator ShootArc()
    {
        Vector2 baseDir = PlayerDirection();

        float startAngle = -arcAngle * 0.5f;
        float step = arcAngle / (projectileCount - 1);

        for (int i = 0; i < projectileCount; i++)
        {
            float angle = startAngle + step * i;

            Vector2 dir =
                Quaternion.Euler(0, 0, angle) * baseDir;

            SpawnProjectile(dir);

            yield return new WaitForSeconds(delayBetweenShots);
        }
    }
}