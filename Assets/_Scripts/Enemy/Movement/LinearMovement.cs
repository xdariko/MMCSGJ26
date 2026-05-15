using UnityEngine;

public class LinearMovement : EnemyMovementBase
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private float angleOffset = 25f;

    private Vector2 direction;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        Vector2 center = G.spawnArea != null
            ? (Vector2)G.spawnArea.Center
            : Vector2.zero;

        Vector2 toCenter =
            (center - (Vector2)transform.position).normalized;

        float randomAngle =
            Random.Range(-angleOffset, angleOffset);

        direction =
            Quaternion.Euler(0, 0, randomAngle) * toCenter;
    }

    protected override void Move()
    {
        Vector2 next =
            rb.position + direction * speed * Time.deltaTime;

        rb.MovePosition(next);
    }
}