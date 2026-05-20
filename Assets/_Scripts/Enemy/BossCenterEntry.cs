using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossCenterEntry : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Если пусто, босс пойдет в центр SpawnArea. Если SpawnArea нет — в Vector2.zero.")]
    [SerializeField] private Transform centerPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.08f;

    [Header("Lock Before Arrival")]
    [Tooltip("Сюда закинь shooter-скрипты босса, например BossOppositeWaveShooter. Они включатся только когда босс дошел до центра.")]
    [SerializeField] private MonoBehaviour[] scriptsToEnableAfterArrival;

    [Tooltip("Если true, скрипт сам найдет все EnemyShooterBase на боссе и выключит их до прибытия.")]
    [SerializeField] private bool autoControlShooters = true;

    [Tooltip("Если true, обычные movement-скрипты врага будут выключены, чтобы они не мешали входу босса.")]
    [SerializeField] private bool disableEnemyMovementScripts = true;

    private Rigidbody2D rb;
    private EnemyShooterBase[] autoShooters;
    private EnemyMovementBase[] movementScripts;

    private Vector2 targetPosition;
    private bool arrived;

    public bool HasArrived => arrived;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (autoControlShooters)
            autoShooters = GetComponents<EnemyShooterBase>();

        if (disableEnemyMovementScripts)
            movementScripts = GetComponents<EnemyMovementBase>();

        targetPosition = GetTargetPosition();

        SetControlledScripts(false);

        if (movementScripts != null)
        {
            foreach (EnemyMovementBase movement in movementScripts)
            {
                if (movement != null)
                    movement.enabled = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (arrived)
            return;

        Vector2 currentPosition = rb.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime);

        rb.MovePosition(nextPosition);

        if (Vector2.Distance(nextPosition, targetPosition) <= stopDistance)
            Arrive();
    }

    private Vector2 GetTargetPosition()
    {
        if (centerPoint != null)
            return centerPoint.position;

        if (G.spawnArea != null)
            return G.spawnArea.Center;

        return Vector2.zero;
    }

    private void Arrive()
    {
        arrived = true;

        rb.linearVelocity = Vector2.zero;
        rb.MovePosition(targetPosition);

        SetControlledScripts(true);

        enabled = false;
    }

    private void SetControlledScripts(bool enabledState)
    {
        if (autoShooters != null)
        {
            foreach (EnemyShooterBase shooter in autoShooters)
            {
                if (shooter != null)
                    shooter.enabled = enabledState;
            }
        }

        if (scriptsToEnableAfterArrival != null)
        {
            foreach (MonoBehaviour script in scriptsToEnableAfterArrival)
            {
                if (script != null)
                    script.enabled = enabledState;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 pos;

        if (centerPoint != null)
            pos = centerPoint.position;
        else if (G.spawnArea != null)
            pos = G.spawnArea.Center;
        else
            pos = Vector3.zero;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(pos, stopDistance);
        Gizmos.DrawLine(transform.position, pos);
    }
}
