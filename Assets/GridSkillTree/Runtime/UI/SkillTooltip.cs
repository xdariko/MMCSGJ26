using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        [Header("Cost Display")]
        [Tooltip("Container where currency cost entries are spawned (Horizontal/VerticalLayoutGroup recommended).")]
        [SerializeField] private Transform costEntriesParent;
        [Tooltip("Prefab with Image (icon) + TMP_Text (amount).")]
        [SerializeField] private GameObject costEntryPrefab;
        [Tooltip("All known currencies — used to resolve icons.")]
        [SerializeField] private CurrencyData[] currencyDatabase;

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
            SkillNodeVisualState state = runtime.GetVisualState(node);

            root.SetActive(true);

            titleText.text = node.title;
            descriptionText.text = node.description;

            levelText.text = $"Level: {level}/{node.maxLevel}";

            BuildCostEntries(node, runtime, level >= node.maxLevel);

            if (costText != null)
                costText.text = level >= node.maxLevel ? "MAX" : "Cost:";

            statusText.text = $"Status: {state}";

            MoveAboveTarget(target);

            PlayAppearAnimation();
        }

        public void Hide()
        {
            currentTween?.Kill();

            ClearCostEntries();
            root.SetActive(false);
        }

        private readonly List<GameObject> spawnedCostEntries = new();

        private void BuildCostEntries(SkillNodeData node, SkillTreeRuntime runtime, bool maxed)
        {
            ClearCostEntries();

            if (maxed || costEntriesParent == null || costEntryPrefab == null) return;

            foreach (var (currency, amount) in runtime.GetCosts(node))
            {
                GameObject go = Instantiate(costEntryPrefab, costEntriesParent);
                go.SetActive(true);
                spawnedCostEntries.Add(go);

                CurrencyData data = FindCurrency(currency);

                Image icon = go.GetComponentInChildren<Image>();
                if (icon != null && data != null)
                {
                    icon.sprite = data.icon;
                    icon.color = data.color;
                }

                TMP_Text label = go.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = amount.ToString();
            }
        }

        private void ClearCostEntries()
        {
            foreach (GameObject go in spawnedCostEntries)
                if (go != null) Destroy(go);
            spawnedCostEntries.Clear();
        }

        private CurrencyData FindCurrency(CurrencyType type)
        {
            if (currencyDatabase == null) return null;
            foreach (CurrencyData cd in currencyDatabase)
                if (cd != null && cd.type == type) return cd;
            return null;
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