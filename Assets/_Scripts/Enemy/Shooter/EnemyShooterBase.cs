using UnityEngine;

public abstract class EnemyShooterBase : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] protected OrbProjectile projectilePrefab;

    [Header("Shoot")]
    [SerializeField] protected float cooldown = 2f;

    protected Transform player;

    private float timer;

    protected EnemyBrain brain;

    protected virtual void Awake()
    {
        brain = GetComponent<EnemyBrain>();
    }

    protected virtual void Start()
    {
        if (G.player != null)
            player = G.player.transform;
    }

    protected virtual void Update()
    {
        if (brain != null && !brain.CanShoot)
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = cooldown;

        Shoot();
    }

    protected abstract void Shoot();

    protected void SpawnProjectile(Vector2 direction)
    {
        OrbProjectile projectile =
            Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity);

        projectile.Initialize(direction);
    }

    protected Vector2 PlayerDirection()
    {
        if (player == null) return Vector2.left;

        return (player.position - transform.position).normalized;
    }
}