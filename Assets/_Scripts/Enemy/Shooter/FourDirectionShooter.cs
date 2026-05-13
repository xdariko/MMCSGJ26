using UnityEngine;

public class FourDirectionShooter : EnemyShooterBase
{
    protected override void Shoot()
    {
        SpawnProjectile(Vector2.up);
        SpawnProjectile(Vector2.down);
        SpawnProjectile(Vector2.left);
        SpawnProjectile(Vector2.right);
    }
}