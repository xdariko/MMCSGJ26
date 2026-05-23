using UnityEngine;

public class FourDirectionShooter : EnemyShooterBase
{
    [Header("Projectile Variants")]
    [SerializeField] private OrbProjectile redProjectilePrefab;
    [SerializeField] private OrbProjectile blueProjectilePrefab;

    [Header("Pattern")]
    [SerializeField] private bool swapColorsEveryShot = true;

    private int shotIndex;

    protected override void Shoot()
    {
        bool swapped = swapColorsEveryShot && shotIndex % 2 == 1;

        SpawnAlternatingProjectile(Vector2.up, 0, swapped);
        SpawnAlternatingProjectile(Vector2.down, 1, swapped);
        SpawnAlternatingProjectile(Vector2.left, 2, swapped);
        SpawnAlternatingProjectile(Vector2.right, 3, swapped);

        shotIndex++;
    }

    private void SpawnAlternatingProjectile(Vector2 direction, int directionIndex, bool swapped)
    {
        bool useBlue = directionIndex % 2 == 1;

        if (swapped)
            useBlue = !useBlue;

        OrbProjectile prefab = useBlue
            ? blueProjectilePrefab
            : redProjectilePrefab;

        if (prefab == null)
            prefab = projectilePrefab;

        SpawnProjectile(prefab, direction);
    }
}