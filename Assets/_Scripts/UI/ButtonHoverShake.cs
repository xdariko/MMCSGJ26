using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverShake : MonoBehaviour, IPointerEnterHandler
{
    [Header("Animation")]
    [SerializeField] private float rotateAngle = 8f;
    [SerializeField] private float duration = 0.08f;

    [Header("Sound")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private float hitSoundVolume = 1f;

    private Tween currentTween;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        currentTween?.Kill();

        rectTransform.rotation = Quaternion.identity;

        SoundManagerSO.PlaySoundFXClip(
            hitSounds,
            transform.position,
            hitSoundVolume
        );

        Sequence seq = DOTween.Sequence();

        seq.Append(
            rectTransform.DORotate(
                new Vector3(0, 0, -rotateAngle),
                duration
            ).SetEase(Ease.OutQuad)
        );

        seq.Append(
            rectTransform.DORotate(
                Vector3.zero,
                duration
            ).SetEase(Ease.OutBack)
        );

        currentTween = seq;
    }
}