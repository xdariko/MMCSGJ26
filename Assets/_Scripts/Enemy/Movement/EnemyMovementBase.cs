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
    }

    protected virtual void Update()
    {
        if (brain != null && !brain.CanMove)
            return;

        Move();
    }

    protected abstract void Move();
}