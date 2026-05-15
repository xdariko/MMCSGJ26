using UnityEngine;
using UnityEngine.UI;

public class StabilityBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private PlayerStabilitySystem stability;

    private void Start()
    {
        stability = G.stability;

        if (stability == null) return;

        slider.maxValue = stability.Max;
        slider.value = stability.Current;

        stability.OnStabilityChanged += HandleStabilityChanged;
    }

    private void OnDestroy()
    {
        if (stability != null)
            stability.OnStabilityChanged -= HandleStabilityChanged;
    }

    private void HandleStabilityChanged(float current, float max)
    {
        if (slider == null) return;
        slider.maxValue = max;
        slider.value = current;
    }
}