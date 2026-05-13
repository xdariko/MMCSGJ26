using UnityEngine;

public class RandomPointMovement : EnemyMovementBase
{
    [Header("Area")]
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachDistance = 0.2f;
    [SerializeField] private float waitTime = 1f;

    private Vector2 target;
    private float waitTimer;

    private void Start()
    {
        PickNewTarget();
    }

    protected override void Move()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        rb.MovePosition(
            Vector2.MoveTowards(rb.position, target, speed * Time.deltaTime)
        );

        if (Vector2.Distance(transform.position, target) <= reachDistance)
        {
            waitTimer = waitTime;
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        float x = Random.Range(minBounds.x, maxBounds.x);
        float y = Random.Range(minBounds.y, maxBounds.y);

        target = new Vector2(x, y);
    }
}