using System.Collections;
using UnityEngine;

public abstract class EnemyShooterBase : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] protected OrbProjectile projectilePrefab;

    [Header("Shoot")]
    [SerializeField] protected float cooldown = 2f;

    [Header("Stop Before Shooting")]
    [SerializeField] private float stopBeforeShootTime = 0.35f;
    [SerializeField] private float stopAfterShootTime = 0.2f;

    protected Transform player;
    protected EnemyBrain brain;

    private float timer;
    private bool shooting;

    protected virtual void Awake()
    {
        brain = GetComponent<EnemyBrain>();
    }

    protected virtual void Start()
    {
        if (G.player != null)
            player = G.player.transform;

        timer = Random.Range(0f, cooldown);
    }

    protected virtual void Update()
    {
        if (G.IsPaused || G.IsPlayerDead)
            return;

        if (shooting)
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        StartCoroutine(ShootCycle());
    }

    private IEnumerator ShootCycle()
    {
        shooting = true;
        timer = cooldown;

        if (brain != null)
            brain.LockMovement();

        yield return new WaitForSeconds(stopBeforeShootTime);

        Shoot();

        yield return new WaitForSeconds(stopAfterShootTime);

        if (brain != null)
            brain.UnlockMovement();

        shooting = false;
    }

    protected abstract void Shoot();

    protected void SpawnProjectile(Vector2 direction)
    {
        SpawnProjectile(direction, transform.position);
    }

    protected void SpawnProjectile(Vector2 direction, Vector3 position)
    {
        if (projectilePrefab == null)
            return;

        OrbProjectile projectile = Instantiate(
            projectilePrefab,
            position,
            Quaternion.identity
        );

        projectile.Initialize(direction);
    }

    protected Vector2 PlayerDirection()
    {
        if (player == null)
            return Vector2.left;

        return (player.position - transform.position).normalized;
    }

    protected virtual void OnDisable()
    {
        shooting = false;

        if (brain != null)
            brain.UnlockMovement();
    }
}