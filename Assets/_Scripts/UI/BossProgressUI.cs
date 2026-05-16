using UnityEngine;
using UnityEngine.UI;

public class BossProgressUI : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void OnEnable()
    {
        BossProgress.OnXPChanged += HandleXPChanged;
    }

    private void OnDisable()
    {
        BossProgress.OnXPChanged -= HandleXPChanged;
    }

    private void HandleXPChanged(int current, int required)
    {
        if (slider == null) return;

        slider.maxValue = required;
        slider.value = current;
    }
}
