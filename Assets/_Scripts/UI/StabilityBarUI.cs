using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StabilityBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform shakeTarget;

    [Header("Fill Gradient")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient fillGradient;

    [Header("Popup")]
    [SerializeField] private StabilityChangePopupUI popupPrefab;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Vector2 popupOffset = new(0f, 35f);

    [Header("Popup Colors")]
    [SerializeField] private Color damageColor = new(0.35f, 0.65f, 1f);
    [SerializeField] private Color gainColor = new(1f, 0.75f, 0.2f);

    [Header("Hit Shake")]
    [SerializeField] private float hitShakeDuration = 0.18f;
    [SerializeField] private float hitShakeStrength = 10f;
    [SerializeField] private int hitShakeVibrato = 18;

    [Header("Danger Shake")]
    [SerializeField] private float lowDangerPercent = 0.3f;
    [SerializeField] private float highDangerPercent = 0.7f;
    [SerializeField] private float dangerShakeDuration = 0.25f;
    [SerializeField] private float dangerShakeStrength = 4f;
    [SerializeField] private int dangerShakeVibrato = 10;

    private PlayerStabilitySystem stability;

    private Tween hitShakeTween;
    private Tween dangerShakeTween;

    private Vector2 startAnchoredPosition;
    private bool isHitShaking;

    private void Awake()
    {
        if (shakeTarget == null && slider != null)
            shakeTarget = slider.GetComponent<RectTransform>();

        if (popupRoot == null && shakeTarget != null)
            popupRoot = shakeTarget;

        if (shakeTarget != null)
            startAnchoredPosition = shakeTarget.anchoredPosition;

        if (fillImage == null && slider != null && slider.fillRect != null)
            fillImage = slider.fillRect.GetComponent<Image>();
    }

    private void OnEnable()
    {
        BindToCurrentPlayer();
    }

    private void Update()
    {
        if (stability != G.stability)
            BindToCurrentPlayer();

        UpdateDangerShake();
    }

    private void OnDisable()
    {
        Unbind();
        KillTweens();
    }

    private void OnDestroy()
    {
        Unbind();
        KillTweens();
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

            UpdateFillColor(0f);
            StopDangerShake();
            return;
        }

        slider.maxValue = stability.Max;
        slider.value = stability.Current;

        UpdateFillColor(stability.Current / stability.Max);

        stability.OnStabilityChanged += HandleStabilityChanged;
        stability.OnStabilityDeltaChanged += HandleStabilityDeltaChanged;
    }

    private void Unbind()
    {
        if (stability != null)
        {
            stability.OnStabilityChanged -= HandleStabilityChanged;
            stability.OnStabilityDeltaChanged -= HandleStabilityDeltaChanged;
        }

        stability = null;
    }

    private void HandleStabilityChanged(float current, float max)
    {
        if (slider == null)
            return;

        slider.maxValue = max;
        slider.value = current;

        float ratio = max > 0f ? current / max : 0f;
        UpdateFillColor(ratio);
    }

    private void UpdateFillColor(float ratio)
    {
        if (fillImage == null)
            return;

        ratio = Mathf.Clamp01(ratio);
        fillImage.color = fillGradient.Evaluate(ratio);
    }

    private void HandleStabilityDeltaChanged(
        float current,
        float max,
        float delta,
        StabilityChangeType changeType)
    {
        if (changeType == StabilityChangeType.Decay)
            return;

        if (Mathf.Approximately(delta, 0f))
            return;

        PlayHitShake();
        SpawnPopup(delta, changeType);
    }

    private void PlayHitShake()
    {
        if (shakeTarget == null)
            return;

        isHitShaking = true;

        dangerShakeTween?.Kill();
        dangerShakeTween = null;

        hitShakeTween?.Kill();
        shakeTarget.anchoredPosition = startAnchoredPosition;

        hitShakeTween = shakeTarget
            .DOShakeAnchorPos(
                hitShakeDuration,
                hitShakeStrength,
                hitShakeVibrato,
                90f,
                snapping: false,
                fadeOut: true)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                isHitShaking = false;

                if (shakeTarget != null)
                    shakeTarget.anchoredPosition = startAnchoredPosition;
            });
    }

    private void UpdateDangerShake()
    {
        if (stability == null || shakeTarget == null || isHitShaking)
            return;

        float ratio = stability.Max > 0f
            ? stability.Current / stability.Max
            : 0f;

        bool shouldShake =
            ratio <= lowDangerPercent ||
            ratio >= highDangerPercent;

        if (shouldShake)
            StartDangerShake();
        else
            StopDangerShake();
    }

    private void StartDangerShake()
    {
        if (dangerShakeTween != null && dangerShakeTween.IsActive())
            return;

        shakeTarget.anchoredPosition = startAnchoredPosition;

        dangerShakeTween = shakeTarget
            .DOShakeAnchorPos(
                dangerShakeDuration,
                dangerShakeStrength,
                dangerShakeVibrato,
                90f,
                snapping: false,
                fadeOut: false)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(true);
    }

    private void StopDangerShake()
    {
        if (dangerShakeTween != null)
        {
            dangerShakeTween.Kill();
            dangerShakeTween = null;
        }

        if (!isHitShaking && shakeTarget != null)
            shakeTarget.anchoredPosition = startAnchoredPosition;
    }

    private void SpawnPopup(float delta, StabilityChangeType changeType)
    {
        if (popupPrefab == null || popupRoot == null)
            return;

        StabilityChangePopupUI popup =
            Instantiate(popupPrefab, popupRoot);

        RectTransform popupRect = popup.GetComponent<RectTransform>();

        if (popupRect != null)
        {
            popupRect.anchoredPosition = popupOffset;
            popupRect.localScale = Vector3.one;
        }

        string sign = delta > 0f ? "+" : "";
        string text = $"{sign}{Mathf.RoundToInt(delta)}";

        Color color = changeType == StabilityChangeType.Damage
            ? damageColor
            : gainColor;

        popup.Setup(text, color);
    }

    private void KillTweens()
    {
        hitShakeTween?.Kill();
        dangerShakeTween?.Kill();

        hitShakeTween = null;
        dangerShakeTween = null;

        isHitShaking = false;

        if (shakeTarget != null)
            shakeTarget.anchoredPosition = startAnchoredPosition;
    }
}