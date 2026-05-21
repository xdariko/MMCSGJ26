using System;
using UnityEngine;

public enum StabilityChangeType
{
    Decay,
    Gain,
    Damage
}

public class PlayerStabilitySystem : MonoBehaviour
{
    [Header("Stability")]
    [SerializeField] private float maxStability = 100f;
    [SerializeField] private float currentStability = 50f;

    [Header("Decay")]
    [SerializeField] private float decayPerSecond = 2f;

    [Header("Critical Zones")]
    [SerializeField] private float minSafe = 20f;
    [SerializeField] private float maxSafe = 80f;

    private bool isDead;

    public float Current => currentStability;
    public float Max => maxStability;

    public event Action<float, float> OnStabilityChanged;
    public event Action<float, float, float, StabilityChangeType> OnStabilityDeltaChanged;
    public event Action OnDeath;

    private void Awake()
    {
        G.stability = this;
    }

    private void Start()
    {
        PlayerStats.BaseStabilityDecay = decayPerSecond;
        OnStabilityChanged?.Invoke(currentStability, maxStability);
    }

    private void Update()
    {
        if (isDead) return;

        ApplyDecay();
        CheckState();
    }

    private void ApplyDecay()
    {
        float prev = currentStability;

        currentStability -= PlayerStats.StabilityDecay * Time.deltaTime;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);

        NotifyStabilityChanged(prev, StabilityChangeType.Decay);
    }

    public void AddStability(float amount)
    {
        if (isDead) return;

        float prev = currentStability;

        currentStability += amount;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);

        NotifyStabilityChanged(prev, StabilityChangeType.Gain);
        CheckState();
    }

    public void RemoveStability(float amount)
    {
        if (isDead) return;

        float prev = currentStability;

        currentStability -= amount;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);

        NotifyStabilityChanged(prev, StabilityChangeType.Damage);
        CheckState();
    }

    private void NotifyStabilityChanged(float previousValue, StabilityChangeType changeType)
    {
        if (Mathf.Approximately(previousValue, currentStability))
            return;

        float delta = currentStability - previousValue;

        OnStabilityChanged?.Invoke(currentStability, maxStability);
        OnStabilityDeltaChanged?.Invoke(currentStability, maxStability, delta, changeType);
    }

    private void CheckState()
    {
        if (currentStability <= 0f)
        {
            Die();
        }
        else if (currentStability >= maxStability)
        {
            Overload();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();

        if (G.main != null)
            G.main.OnPlayerDeath();
    }

    private void Overload()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();

        if (G.main != null)
            G.main.OnPlayerDeath();
    }
}