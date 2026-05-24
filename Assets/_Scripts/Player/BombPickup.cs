using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BombPickup : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionEffectLifetime = 1.5f;

    [Header("Explosion Sound")]
    [SerializeField] private AudioClip[] explosionSounds;
    [SerializeField] private float explosionSoundVolume = 0.8f;

    [Header("Fallback Values")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float damage = 5f;

    [Header("Animation")]
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseDuration = 0.45f;

    private bool exploded;
    private Tween pulseTween;
    private Vector3 baseScale;

    public void Setup(float radius, float damageAmount)
    {
        explosionRadius = Mathf.Max(0.1f, radius);
        damage = Mathf.Max(0f, damageAmount);
    }

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Start()
    {
        StartPulseAnimation();
    }

    private void StartPulseAnimation()
    {
        pulseTween?.Kill();

        transform.localScale = baseScale;

        pulseTween = transform
            .DOScale(baseScale * pulseScale, pulseDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
            return;

        if (!other.CompareTag("Player"))
            return;

        Explode();
    }

    private void Explode()
    {
        exploded = true;

        pulseTween?.Kill();

        PlayExplosionSound();

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();

            if (enemy == null)
                enemy = hit.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(
                explosionEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            if (explosionEffectLifetime > 0f)
                Destroy(fx, explosionEffectLifetime);
        }

        Destroy(gameObject);
    }

    private void PlayExplosionSound()
    {
        if (explosionSounds == null || explosionSounds.Length == 0)
            return;

        SoundManagerSO.PlaySoundFXClip(
            explosionSounds,
            transform.position,
            explosionSoundVolume
        );
    }

    private void OnDestroy()
    {
        pulseTween?.Kill();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}