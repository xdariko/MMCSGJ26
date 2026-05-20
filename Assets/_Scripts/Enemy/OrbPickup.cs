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

        float distance = Vector2.Distance(
            transform.position,
            target.position);

        if (distance <= collectDistance)
        {
            PlayerStabilitySystem stability =
                target.GetComponent<PlayerStabilitySystem>();

            if (stability != null)
                Collect(stability);
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

    private void ApplyEffects(PlayerStabilitySystem stability)
    {
        if (data == null || data.effects == null)
            return;

        foreach (OrbEffect effect in data.effects)
        {
            if (effect == null)
                continue;

            ApplyEffect(effect, stability);
        }
    }

    private void ApplyEffect(
        OrbEffect effect,
        PlayerStabilitySystem stability)
    {
        switch (effect.effectType)
        {
            case OrbEffectType.Stability:
                stability.AddStability(effect.value);
                break;

            case OrbEffectType.Currency:
                int baseAmount = Mathf.RoundToInt(effect.value);
                int finalAmount = PlayerStats.ApplyCurrencyDropMultiplier(
                    effect.currencyType,
                    baseAmount);

                CurrencyManager.Add(effect.currencyType, finalAmount);
                break;

            case OrbEffectType.Instability:
                stability.RemoveStability(effect.value);
                break;
        }
    }
}