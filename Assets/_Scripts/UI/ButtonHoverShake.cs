using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIButtonHoverShake : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private RectTransform visual;
    [SerializeField] private RectTransform stableHitArea;

    [Header("Animation")]
    [SerializeField] private float rotateAngle = 8f;
    [SerializeField] private float duration = 0.08f;

    [Header("Click Animation")]
    [SerializeField] private float clickRotateAngle = 5f;

    [Header("Sound")]
    [SerializeField] private AudioClip[] hoverSounds;
    [SerializeField] private AudioClip[] clickSounds;
    [SerializeField] private float soundVolume = 1f;

    private Tween currentTween;
    private bool isHovered;

    private void Awake()
    {
        if (visual == null)
            visual = transform.GetChild(0) as RectTransform;

        if (stableHitArea == null)
            stableHitArea = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!isHovered)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        bool pointerInside =
            RectTransformUtility.RectangleContainsScreenPoint(
                stableHitArea,
                mousePosition,
                null);

        if (!pointerInside)
            isHovered = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovered)
            return;

        isHovered = true;
        PlayHoverAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        currentTween?.Kill();
        visual.localRotation = Quaternion.identity;

        SoundManagerSO.PlaySoundFXClip(
            clickSounds,
            transform.position,
            soundVolume);

        currentTween = DOTween.Sequence()
            .Append(visual.DOLocalRotate(
                new Vector3(0, 0, clickRotateAngle),
                0.05f).SetEase(Ease.OutQuad))
            .Append(visual.DOLocalRotate(
                Vector3.zero,
                0.1f).SetEase(Ease.OutBack));
    }

    private void PlayHoverAnimation()
    {
        currentTween?.Kill();
        visual.localRotation = Quaternion.identity;

        SoundManagerSO.PlaySoundFXClip(
            hoverSounds,
            transform.position,
            soundVolume);

        currentTween = DOTween.Sequence()
            .Append(visual.DOLocalRotate(
                new Vector3(0, 0, -rotateAngle),
                duration).SetEase(Ease.OutQuad))
            .Append(visual.DOLocalRotate(
                Vector3.zero,
                duration).SetEase(Ease.OutBack));
    }
}