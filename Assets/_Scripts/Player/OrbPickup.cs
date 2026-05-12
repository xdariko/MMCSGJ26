using UnityEngine;

public class OrbPickup : ManagedBehaviour
{
    [SerializeField] private OrbData data;
    [SerializeField] private float pickupRadius = 1.2f;

    private Transform player;
    private bool isCollected;

    private void Start()
    {
        player = G.player.transform;
    }

    protected override void PausableUpdate()
    {
        if (isCollected || player == null) return;

        float distance =
            Vector2.Distance(transform.position, player.position);

        if (distance < pickupRadius)
        {
            Collect();
            return;
        }

        if (distance < 3.5f)
        {
            transform.position = Vector2.Lerp(
                transform.position,
                player.position,
                Time.deltaTime * data.attractSpeed);
        }
    }

    private void Collect()
    {
        isCollected = true;

        ApplyEffects();

        Destroy(gameObject);
    }

    private void ApplyEffects()
    {
        var stability = player.GetComponent<PlayerStabilitySystem>();

        foreach (var effect in data.effects)
        {
            switch (effect.type)
            {
                case OrbEffectType.Stability:
                    stability.AddStability(effect.value);
                    break;

                case OrbEffectType.Currency:
                    G.AddCurrency((int)effect.value);
                    break;

                case OrbEffectType.Instability:
                    stability.RemoveStability(effect.value);
                    break;
            }
        }
    }
}