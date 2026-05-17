using System;
using UnityEngine;

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

        if (!Mathf.Approximately(prev, currentStability))
            OnStabilityChanged?.Invoke(currentStability, maxStability);
    }

    public void AddStability(float amount)
    {
        currentStability += amount;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);
        OnStabilityChanged?.Invoke(currentStability, maxStability);
    }

    public void RemoveStability(float amount)
    {
        currentStability -= amount;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);
        OnStabilityChanged?.Invoke(currentStability, maxStability);
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

        // Example: animator.SetTrigger("Death");
        // Example: Instantiate(deathVFX, transform.position, Quaternion.identity);

        OnDeath?.Invoke();

        if (G.main != null)
            G.main.OnPlayerDeath();
    }

    private void Overload()
    {
        if (isDead) return;
        isDead = true;

        // Example: animator.SetTrigger("Overload");
        // Example: Instantiate(overloadVFX, transform.position, Quaternion.identity);

        OnDeath?.Invoke();

        if (G.main != null)
            G.main.OnPlayerDeath();
    }
}