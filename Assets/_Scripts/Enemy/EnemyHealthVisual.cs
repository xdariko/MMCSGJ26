using DG.Tweening;
using UnityEngine;

public class EnemyHealthVisual : MonoBehaviour
{
    [Header("Mask")]
    [SerializeField] private Transform maskTransform;
    [SerializeField] private Vector3 maskHideOffset = new(-1.5f, -1.5f, 0f);

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer bgRenderer;
    [SerializeField] private SpriteRenderer fillRenderer;

    [Header("Shake")]
    [SerializeField] private Transform shakeTarget;
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeStrength = 0.1f;

    private EnemyHealth health;
    private Tween shakeTween;
    private Vector3 maskStartPos;

    private static int orderCounter;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();

        if (maskTransform != null)
            maskStartPos = maskTransform.localPosition;

        IsolateMask();
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
        if (maskTransform == null) return;

        float t = 1f - Mathf.Clamp01(ratio);
        maskTransform.localPosition = maskStartPos + maskHideOffset * t;
    }

    private void PlayShake()
    {
        if (shakeTarget == null) return;

        shakeTween?.Kill(true);
        shakeTween = shakeTarget
            .DOShakePosition(shakeDuration, shakeStrength, vibrato: 20, fadeOut: true)
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        shakeTween?.Kill();
    }
}
