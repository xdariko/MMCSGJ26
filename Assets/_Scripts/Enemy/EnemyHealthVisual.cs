using DG.Tweening;
using UnityEngine;

public class EnemyHealthVisual : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField] private Transform maskTransform;

    [Header("Mask Positions")]
    [SerializeField] private bool useManualPositions = true;

    [Tooltip("Позиция маски при полном HP")]
    [SerializeField] private Vector3 visibleLocalPosition;

    [Tooltip("Позиция маски при 0 HP")]
    [SerializeField] private Vector3 hiddenLocalPosition;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer bgRenderer;
    [SerializeField] private SpriteRenderer fillRenderer;

    [Header("Shake")]
    [SerializeField] private Transform shakeTarget;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeStrength = 0.1f;

    private EnemyHealth health;
    private Tween shakeTween;

    private static int orderCounter;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();

        if (maskTransform != null && !useManualPositions)
        {
            visibleLocalPosition = maskTransform.localPosition;
            hiddenLocalPosition = maskTransform.localPosition + new Vector3(-0.6f, -0.2f, 0f);
        }

        IsolateMask();
    }

    private void Start()
    {
        UpdateMask(1f);
    }

    private void IsolateMask()
    {
        int order = orderCounter;
        orderCounter += 2;

        if (bgRenderer != null)
            bgRenderer.sortingOrder = order;

        if (fillRenderer != null)
            fillRenderer.sortingOrder = order + 1;

        if (maskTransform != null)
        {
            SpriteMask mask = maskTransform.GetComponent<SpriteMask>();

            if (mask != null)
            {
                mask.isCustomRangeActive = true;
                mask.frontSortingOrder = order + 1;
                mask.backSortingOrder = order;
            }
        }
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDamaged += HandleDamaged;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDamaged -= HandleDamaged;
    }

    private void HandleDamaged(float hpRatio)
    {
        UpdateMask(hpRatio);
        PlayShake();
    }

    private void UpdateMask(float ratio)
    {
        if (maskTransform == null)
            return;

        ratio = Mathf.Clamp01(ratio);

        maskTransform.localPosition =
            Vector3.Lerp(hiddenLocalPosition, visibleLocalPosition, ratio);
    }

    private void PlayShake()
    {
        if (shakeTarget == null)
            return;

        shakeTween?.Kill(true);

        shakeTween = shakeTarget
            .DOShakePosition(
                shakeDuration,
                shakeStrength,
                vibrato: 20,
                fadeOut: true)
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
    }

#if UNITY_EDITOR
    [ContextMenu("Use Current Position As Visible")]
    private void UseCurrentAsVisible()
    {
        if (maskTransform != null)
        {
            visibleLocalPosition = maskTransform.localPosition;
            useManualPositions = true;
        }
    }

    [ContextMenu("Use Current Position As Hidden")]
    private void UseCurrentAsHidden()
    {
        if (maskTransform != null)
        {
            hiddenLocalPosition = maskTransform.localPosition;
            useManualPositions = true;
        }
    }
#endif
}