using System;
using DG.Tweening;
using UnityEngine;

public class ScreenDustTransition : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform curtain;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float coverDuration = 0.55f;
    [SerializeField] private float coveredPause = 0.12f;
    [SerializeField] private float uncoverDuration = 0.55f;

    [Header("Settings")]
    [SerializeField] private bool useUnscaledTime = true;

    private Sequence sequence;
    private bool isPlaying;

    private void Awake()
    {
        G.transition = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideInstant();
    }

    public void Play(Action onCovered)
    {
        Play(onCovered, null);
    }

    public void Play(Action onCovered, Action onFinished)
    {
        if (isPlaying)
            return;

        if (curtain == null)
        {
            onCovered?.Invoke();
            onFinished?.Invoke();
            return;
        }

        isPlaying = true;

        gameObject.SetActive(true);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        float width = GetScreenWidth();

        curtain.anchoredPosition = new Vector2(width, 0f);

        sequence?.Kill();

        sequence = DOTween.Sequence();
        sequence.SetUpdate(useUnscaledTime);

        sequence.Append(
            curtain
                .DOAnchorPos(Vector2.zero, coverDuration)
                .SetEase(Ease.OutCubic)
        );

        sequence.AppendCallback(() =>
        {
            onCovered?.Invoke();
        });

        sequence.AppendInterval(coveredPause);

        sequence.Append(
            curtain
                .DOAnchorPos(new Vector2(-width, 0f), uncoverDuration)
                .SetEase(Ease.InCubic)
        );

        sequence.AppendCallback(() =>
        {
            HideInstant();
            isPlaying = false;
            onFinished?.Invoke();
        });
    }

    public void HideInstant()
    {
        sequence?.Kill();

        if (curtain != null)
            curtain.anchoredPosition = new Vector2(GetScreenWidth(), 0f);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
        isPlaying = false;
    }

    private float GetScreenWidth()
    {
        RectTransform parent = curtain != null ? curtain.parent as RectTransform : null;

        if (parent != null)
            return parent.rect.width;

        return Screen.width;
    }
}