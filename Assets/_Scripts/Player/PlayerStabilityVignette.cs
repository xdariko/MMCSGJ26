using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerStabilityVignette : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume volume;
    [SerializeField] private PlayerStabilitySystem stability;

    [Header("Intensity")]
    [SerializeField] private float minIntensity = 0.15f;
    [SerializeField] private float maxIntensity = 0.5f;

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float pulseStrength = 0.1f;

    [Header("Colors")]
    [SerializeField] private Color lowStabilityColor = new(0.4f, 0.6f, 1f);
    [SerializeField] private Color highStabilityColor = new(1f, 0.35f, 0.1f);

    [Header("Reset")]
    [SerializeField] private Color defaultColor = Color.black;
    [SerializeField] private float defaultIntensity = 0f;

    private Vignette vignette;

    private void Awake()
    {
        if (volume == null)
            volume = GetComponent<Volume>();

        if (volume != null)
            volume.profile.TryGet(out vignette);
    }

    private void OnEnable()
    {
        FindStability();
        ResetVignette();
    }

    private void Update()
    {
        if (vignette == null)
            return;

        if (stability == null)
            FindStability();

        if (stability == null || G.IsPlayerDead)
        {
            ResetVignette();
            return;
        }

        float ratio = stability.Max > 0f
            ? stability.Current / stability.Max
            : 0f;

        ratio = Mathf.Clamp01(ratio);

        vignette.color.value =
            Color.Lerp(lowStabilityColor, highStabilityColor, ratio);

        float baseIntensity =
            Mathf.Lerp(maxIntensity, minIntensity, ratio);

        float pulseMultiplier = 1f - ratio;

        float pulse =
            Mathf.Sin(Time.time * pulseSpeed) *
            pulseStrength *
            pulseMultiplier;

        vignette.intensity.value =
            Mathf.Clamp(baseIntensity + pulse, 0f, 1f);
    }

    private void FindStability()
    {
        stability = G.stability;

        if (stability == null)
            stability = FindFirstObjectByType<PlayerStabilitySystem>();
    }

    private void ResetVignette()
    {
        if (vignette == null)
            return;

        vignette.color.value = defaultColor;
        vignette.intensity.value = defaultIntensity;
    }

    private void OnDisable()
    {
        ResetVignette();
    }

    private void OnDestroy()
    {
        ResetVignette();
    }
}