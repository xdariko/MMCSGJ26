using UnityEngine;
using UnityEngine.UI;

public class StabilityBarUI : MonoBehaviour
{
    private PlayerStabilitySystem player;
    [SerializeField] private Slider slider;

    private void Start()
    {
        player = G.player.GetComponent<PlayerStabilitySystem>();
        slider.maxValue = player.Max;
    }

    private void Update()
    {
        slider.value = player.Current;
    }
}