using DG.Tweening;
using UnityEngine;

public class OrbProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 5f;

    [Header("Spawn")]
    [SerializeField] private float spawnDuration = 0.25f;

    [Header("Slowdown")]
    [SerializeField] private float slowdownPortion = 0.35f;
    [SerializeField] private float lingerTime = 0.2f;

    [Header("Data")]
    [SerializeField] private OrbData data;

    private Vector2 direction;
    private float currentSpeed;
    private float age;
    private float flyTime;
    private bool launched;
    private bool dying;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        currentSpeed = speed;
        flyTime = lifetime - spawnDuration;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, spawnDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => launched = true);
    }

    private void Update()
    {
        if (!launched || dying) return;

        age += Time.deltaTime;

        float slowdownStart = flyTime * (1f - slowdownPortion);

        if (age >= flyTime)
        {
            dying = true;
            currentSpeed = 0f;

            transform.DOScale(Vector3.zero, lingerTime)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Destroy(gameObject));
            return;
        }

        if (age > slowdownStart)
        {
            float t = (age - slowdownStart) / (flyTime - slowdownStart);
            currentSpeed = Mathf.Lerp(speed, 0f, t);
        }

        transform.position +=
            (Vector3)(direction * currentSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStabilitySystem stability =
            other.GetComponent<PlayerStabilitySystem>();

        if (stability == null)
            return;

        ApplyEffect(stability);

        Destroy(gameObject);
    }

    private void ApplyEffect(
        PlayerStabilitySystem stability)
    {
        switch (data.effectType)
        {
            case OrbEffectType.Stability:
                stability.AddStability(data.value);
                break;

            case OrbEffectType.Instability:
                stability.RemoveStability(data.value);
                break;
        }
    }
}