using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Animation")]
    [SerializeField] private float scaleMultiplier = 1.15f;
    [SerializeField] private float clickScaleMultiplier = 0.9f;
    [SerializeField] private float duration = 0.12f;

    [SerializeField] private Ease ease = Ease.OutQuad;

    [Header("Sound")]
    [SerializeField] private AudioClip[] hoverSounds;
    [SerializeField] private AudioClip[] clickSounds;
    [SerializeField] private float soundVolume = 1f;

    private RectTransform rectTransform;
    private Vector3 initialScale;

    private Tween currentTween;
    private bool isHovered;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        currentTween?.Kill();

        SoundManagerSO.PlaySoundFXClip(
            hoverSounds,
            transform.position,
            soundVolume
        );

        currentTween = rectTransform
            .DOScale(initialScale * scaleMultiplier, duration)
            .SetEase(ease);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        currentTween?.Kill();

        currentTween = rectTransform
            .DOScale(initialScale, duration)
            .SetEase(ease);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        currentTween?.Kill();

        SoundManagerSO.PlaySoundFXClip(
            clickSounds,
            transform.position,
            soundVolume
        );

        Sequence seq = DOTween.Sequence();

        seq.Append(rectTransform.DOScale(initialScale * clickScaleMultiplier, 0.06f)
            .SetEase(Ease.OutQuad));

        seq.Append(rectTransform.DOScale(
            isHovered ? initialScale * scaleMultiplier : initialScale,
            0.12f
        ).SetEase(Ease.OutBack));

        currentTween = seq;
    }
}