using UnityEngine;

public class AimBurstShooter : EnemyShooterBase
{
    [SerializeField] private int count = 3;
    [SerializeField] private float spreadAngle = 15f;

    protected override void Shoot()
    {
        Vector2 baseDir = PlayerDirection();

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);

            float angle =
                Mathf.Lerp(-spreadAngle, spreadAngle, t);

            Vector2 dir =
                Quaternion.Euler(0, 0, angle) * baseDir;

            SpawnProjectile(dir);
        }
    }
}