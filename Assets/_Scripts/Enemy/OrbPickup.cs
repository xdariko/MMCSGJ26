using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class OrbPickup : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private OrbData data;

    [Header("Collect")]
    [SerializeField] private float collectDistance = 0.25f;

    private Transform target;

    private bool isFollowing;
    private bool isCollected;

    public void StartFollowing(Transform followTarget)
    {
        if (isCollected)
            return;

        target = followTarget;
        isFollowing = true;
    }

    private void Update()
    {
        if (!isFollowing || isCollected || target == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            data.attractSpeed * Time.deltaTime);

        float distance =
            Vector2.Distance(
                transform.position,
                target.position);

        if (distance <= collectDistance)
        {
            PlayerStabilitySystem stability =
                target.GetComponent<PlayerStabilitySystem>();

            if (stability != null)
            {
                Collect(stability);
            }
        }
    }

    private void Collect(PlayerStabilitySystem stability)
    {
        if (isCollected)
            return;

        isCollected = true;

        ApplyEffects(stability);

        Destroy(gameObject);
    }

    private void ApplyEffects(
        PlayerStabilitySystem stability)
    {
        switch (data.effectType)
        {
            case OrbEffectType.Stability:
                stability.AddStability(data.value);
                break;

            case OrbEffectType.Currency:
                G.AddCurrency((int)data.value);
                break;

            case OrbEffectType.Instability:
                stability.RemoveStability(data.value);
                break;
        }
    }
}