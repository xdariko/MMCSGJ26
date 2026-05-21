using DG.Tweening;
using TMPro;
using UnityEngine;

public class StabilityChangePopupUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float moveY = 45f;
    [SerializeField] private float duration = 0.65f;
    [SerializeField] private float scalePunch = 0.25f;

    private RectTransform rectTransform;
    private Sequence sequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Setup(string value, Color color)
    {
        if (text != null)
        {
            text.text = value;
            text.color = color;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        PlayAnimation();
    }

    private void PlayAnimation()
    {
        if (rectTransform == null)
            return;

        sequence?.Kill();

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, moveY);

        sequence = DOTween.Sequence()
            .SetUpdate(true);

        sequence.Join(
            rectTransform
                .DOAnchorPos(endPos, duration)
                .SetEase(Ease.OutCubic));

        sequence.Join(
            rectTransform
                .DOPunchScale(Vector3.one * scalePunch, 0.25f, 8, 0.8f));

        if (canvasGroup != null)
        {
            sequence.Join(
                canvasGroup
                    .DOFade(0f, duration)
                    .SetEase(Ease.InCubic));
        }

        sequence.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    private void OnDestroy()
    {
        sequence?.Kill();
    }
}