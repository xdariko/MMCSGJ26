using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverShake : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
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
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentTween?.Kill();

        SoundManagerSO.PlaySoundFXClip(
            hoverSounds,
            transform.position,
            soundVolume
        );

        Sequence seq = DOTween.Sequence();

        seq.Append(rectTransform.DORotate(new Vector3(0, 0, -rotateAngle), duration)
            .SetEase(Ease.OutQuad));

        seq.Append(rectTransform.DORotate(Vector3.zero, duration)
            .SetEase(Ease.OutBack));

        currentTween = seq;
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

        seq.Append(rectTransform.DORotate(new Vector3(0, 0, clickRotateAngle), 0.05f)
            .SetEase(Ease.OutQuad));

        seq.Append(rectTransform.DORotate(Vector3.zero, 0.1f)
            .SetEase(Ease.OutBack));

        currentTween = seq;
    }
}