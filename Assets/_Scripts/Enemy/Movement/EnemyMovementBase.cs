using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyMovementBase : MonoBehaviour
{
    protected EnemyBrain brain;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        rb = GetComponent<Rigidbody2D>();

        // Movement is stepped at the physics rate, so interpolate to keep it smooth at high FPS.
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // MovePosition is only applied on the physics step, so it must be called from FixedUpdate.
    // In Update, several calls per physics step overwrite each other and enemies slow down as FPS grows.
    protected virtual void FixedUpdate()
    {
        if (brain != null && !brain.CanMove)
            return;

        Move();
    }

    protected abstract void Move();
}