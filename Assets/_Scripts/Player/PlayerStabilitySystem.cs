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

    public float Current => currentStability;
    public float Max => maxStability;

    private void Update()
    {
        ApplyDecay();
        CheckState();
    }

    private void ApplyDecay()
    {
        currentStability -= decayPerSecond * Time.deltaTime;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);
    }

    public void AddStability(float amount)
    {
        currentStability += amount;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);
    }

    public void RemoveStability(float amount)
    {
        currentStability -= amount;
        currentStability = Mathf.Clamp(currentStability, 0f, maxStability);
    }

    private void CheckState()
    {
        if (currentStability <= 0f)
        {
            Die();
        }

        if (currentStability >= maxStability)
        {
            Overload();
        }
    }

    private void Die()
    {
        // TODO
    }

    private void Overload()
    {
        // TODO
    }
}