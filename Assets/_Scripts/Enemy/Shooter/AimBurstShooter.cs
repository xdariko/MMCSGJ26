using UnityEngine;

public class AimBurstShooter : EnemyShooterBase
{
    private enum ProjectilePattern
    {
        OnlyRed,
        OnlyBlue,
        RandomMixed,
        Alternating,
        MostlyRed,
        MostlyBlue
    }

    [Header("Burst")]
    [SerializeField] private int count = 3;
    [SerializeField] private float spreadAngle = 15f;

    [Header("Projectile Variants")]
    [SerializeField] private OrbProjectile redProjectilePrefab;
    [SerializeField] private OrbProjectile blueProjectilePrefab;

    [Header("Projectile Pattern")]
    [SerializeField] private ProjectilePattern pattern = ProjectilePattern.MostlyRed;

    [Range(0f, 1f)]
    [SerializeField] private float blueChance = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float redChance = 0.25f;

    [SerializeField] private bool swapAlternatingEveryShot = true;

    private int burstIndex;

    protected override void Shoot()
    {
        Vector2 baseDir = PlayerDirection();

        if (baseDir.sqrMagnitude <= 0.001f)
            baseDir = Vector2.left;

        baseDir.Normalize();

        bool swapped = swapAlternatingEveryShot && burstIndex % 2 == 1;

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : (float)i / (count - 1);

            float angle = Mathf.Lerp(-spreadAngle, spreadAngle, t);

            Vector2 dir =
                Quaternion.Euler(0f, 0f, angle) * baseDir;

            OrbProjectile prefab = GetProjectileForIndex(i, swapped);

            SpawnProjectile(prefab, dir.normalized);
        }

        burstIndex++;
    }

    private OrbProjectile GetProjectileForIndex(int index, bool swapped)
    {
        OrbProjectile prefab;

        switch (pattern)
        {
            case ProjectilePattern.OnlyRed:
                prefab = redProjectilePrefab;
                break;

            case ProjectilePattern.OnlyBlue:
                prefab = blueProjectilePrefab;
                break;

            case ProjectilePattern.RandomMixed:
                prefab = Random.value < 0.5f
                    ? blueProjectilePrefab
                    : redProjectilePrefab;
                break;

            case ProjectilePattern.Alternating:
                bool useBlue = index % 2 == 1;

                if (swapped)
                    useBlue = !useBlue;

                prefab = useBlue
                    ? blueProjectilePrefab
                    : redProjectilePrefab;
                break;

            case ProjectilePattern.MostlyRed:
                prefab = Random.value < blueChance
                    ? blueProjectilePrefab
                    : redProjectilePrefab;
                break;

            case ProjectilePattern.MostlyBlue:
                prefab = Random.value < redChance
                    ? redProjectilePrefab
                    : blueProjectilePrefab;
                break;

            default:
                prefab = projectilePrefab;
                break;
        }

        if (prefab == null)
            prefab = projectilePrefab;

        return prefab;
    }
}