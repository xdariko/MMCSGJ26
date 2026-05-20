using UnityEngine;
using UnityEngine.UI;

public class StabilityBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private PlayerStabilitySystem stability;

    private void OnEnable()
    {
        BindToCurrentPlayer();
    }

    private void Update()
    {
        // Player can be destroyed and spawned again without recreating this UI object.
        // If G.stability changed, re-bind the bar to the new player.
        if (stability != G.stability)
            BindToCurrentPlayer();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void BindToCurrentPlayer()
    {
        Unbind();

        stability = G.stability;

        if (stability == null || slider == null)
        {
            if (slider != null)
            {
                slider.maxValue = 1f;
                slider.value = 0f;
            }

            return;
        }

        slider.maxValue = stability.Max;
        slider.value = stability.Current;

        stability.OnStabilityChanged += HandleStabilityChanged;
    }

    private void Unbind()
    {
        if (stability != null)
            stability.OnStabilityChanged -= HandleStabilityChanged;

        stability = null;
    }

    private void HandleStabilityChanged(float current, float max)
    {
        if (slider == null)
            return;

        slider.maxValue = max;
        slider.value = current;
    }
}

