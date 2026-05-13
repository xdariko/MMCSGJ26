using UnityEngine;

public class SineMovement : EnemyMovementBase
{
    [SerializeField] private Vector2 direction = Vector2.left;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float frequency = 2f;

    private float time;

    protected override void Move()
    {
        time += Time.deltaTime;

        Vector2 baseMove = direction.normalized * speed * Time.deltaTime;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        float sine = Mathf.Sin(time * frequency) * amplitude;

        Vector2 next = rb.position + baseMove + perpendicular * sine * Time.deltaTime;

        rb.MovePosition(next);
    }
}