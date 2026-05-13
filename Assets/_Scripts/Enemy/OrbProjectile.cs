using UnityEngine;

public class OrbProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;

    [SerializeField] private float lifetime = 5f;

    [Header("Data")]
    [SerializeField] private OrbData data;

    private Vector2 direction;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);
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