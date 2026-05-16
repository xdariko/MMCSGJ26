using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GridSkillTree
{
    public class SkillTooltip : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text statusText;

        [Header("Position")]
        [SerializeField] private float verticalOffset = 10f;

        [Header("Animation")]
        [SerializeField] private float popupScale = 1.08f;
        [SerializeField] private float appearDuration = 0.12f;
        [SerializeField] private float shakeStrength = 8f;
        [SerializeField] private int shakeVibrato = 12;
        [SerializeField] private float shakeDuration = 0.18f;

        private RectTransform tooltipRect;
        private Tween currentTween;

        private void Awake()
        {
            tooltipRect = root.GetComponent<RectTransform>();

            HideImmediate();
        }

        public void Show(SkillNodeData node, SkillTreeRuntime runtime, RectTransform target)
        {
            if (node == null || runtime == null || target == null)
                return;

            int level = runtime.GetLevel(node.id);
            int cost = runtime.GetCost(node);
            SkillNodeVisualState state = runtime.GetVisualState(node);

            root.SetActive(true);

            titleText.text = node.title;
            descriptionText.text = node.description;

            levelText.text = $"Level: {level}/{node.maxLevel}";

            costText.text = level >= node.maxLevel
                ? "Cost: MAX"
                : $"Cost: {cost}";

            statusText.text = $"Status: {state}";

            MoveAboveTarget(target);

            PlayAppearAnimation();
        }

        public void Hide()
        {
            currentTween?.Kill();

            root.SetActive(false);
        }

        private void HideImmediate()
        {
            root.SetActive(false);

            if (tooltipRect != null)
            {
                tooltipRect.localScale = Vector3.one;
                tooltipRect.localRotation = Quaternion.identity;
            }
        }

        private void MoveAboveTarget(RectTransform target)
        {
            if (tooltipRect == null)
                return;

            Canvas.ForceUpdateCanvases();

            Vector2 targetAnchoredPosition = target.anchoredPosition;

            float targetHeight = target.rect.height;
            float tooltipHeight = tooltipRect.rect.height;

            float x = targetAnchoredPosition.x;

            float y =
                targetAnchoredPosition.y +
                targetHeight / 2f +
                tooltipHeight / 2f +
                verticalOffset;

            tooltipRect.anchoredPosition = new Vector2(x, y);
        }

        private void PlayAppearAnimation()
        {
            currentTween?.Kill();

            tooltipRect.localScale = Vector3.zero;
            tooltipRect.localRotation = Quaternion.identity;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                tooltipRect.DOScale(
                    popupScale,
                    appearDuration
                ).SetEase(Ease.OutBack)
            );

            seq.Append(
                tooltipRect.DOScale(
                    1f,
                    0.08f
                )
            );

            seq.Join(
                tooltipRect.DOShakeRotation(
                    shakeDuration,
                    new Vector3(0, 0, shakeStrength),
                    shakeVibrato,
                    90,
                    false
                )
            );

            currentTween = seq;
        }
    }
}