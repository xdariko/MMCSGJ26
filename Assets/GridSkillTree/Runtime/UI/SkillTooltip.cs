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

        [Header("Draw On Top")]
        [SerializeField] private bool moveToRootCanvas = true;
        [SerializeField] private int sortingOrder = 32767;

        [Header("Animation")]
        [SerializeField] private float popupScale = 1.08f;
        [SerializeField] private float appearDuration = 0.12f;
        [SerializeField] private float shakeStrength = 8f;
        [SerializeField] private int shakeVibrato = 12;
        [SerializeField] private float shakeDuration = 0.18f;

        private RectTransform tooltipRect;
        private Canvas rootCanvas;
        private Canvas tooltipCanvas;
        private Tween currentTween;

        private readonly List<GameObject> spawnedCostEntries = new();

        private void Awake()
        {
            if (root == null)
                root = gameObject;

            tooltipRect = root.GetComponent<RectTransform>();

            SetupTopLayer();

            HideImmediate();
        }

        private void SetupTopLayer()
        {
            rootCanvas = FindRootCanvas();

            if (moveToRootCanvas && rootCanvas != null && root.transform.parent != rootCanvas.transform)
            {
                root.transform.SetParent(rootCanvas.transform, false);
            }

            root.transform.SetAsLastSibling();

            tooltipCanvas = root.GetComponent<Canvas>();

            if (tooltipCanvas == null)
                tooltipCanvas = root.AddComponent<Canvas>();

            tooltipCanvas.overrideSorting = true;
            tooltipCanvas.sortingOrder = sortingOrder;

            if (rootCanvas != null)
                tooltipCanvas.sortingLayerID = rootCanvas.sortingLayerID;

            if (root.GetComponent<GraphicRaycaster>() == null)
                root.AddComponent<GraphicRaycaster>();
        }

        private Canvas FindRootCanvas()
        {
            Canvas[] canvases = GetComponentsInParent<Canvas>(true);

            Canvas result = null;

            foreach (Canvas canvas in canvases)
            {
                if (canvas == null)
                    continue;

                if (canvas.isRootCanvas)
                    result = canvas;
            }

            if (result != null)
                return result;

            if (canvases.Length > 0)
                return canvases[canvases.Length - 1];

            return null;
        }

        public void Show(SkillNodeData node, SkillTreeRuntime runtime, RectTransform target)
        {
            if (node == null || runtime == null || target == null || root == null)
                return;

            SetupTopLayer();

            int level = runtime.GetLevel(node.id);

            root.SetActive(true);
            root.transform.SetAsLastSibling();

            if (titleText != null)
                titleText.text = node.title;

            if (descriptionText != null)
                descriptionText.text = node.description;

            if (levelText != null)
                levelText.text = $"Level: {level}/{node.maxLevel}";

            BuildCostEntries(node, runtime, level >= node.maxLevel);

            if (costText != null)
                costText.text = level >= node.maxLevel ? "MAX" : "Cost:";

            if (statusText != null)
                statusText.text = $"Status: {runtime.GetStatusText(node)}";

            MoveAboveTarget(target);

            PlayAppearAnimation();
        }

        public void Hide()
        {
            currentTween?.Kill();

            ClearCostEntries();

            if (root != null)
                root.SetActive(false);
        }

        private void BuildCostEntries(SkillNodeData node, SkillTreeRuntime runtime, bool maxed)
        {
            ClearCostEntries();

            if (maxed || costEntriesParent == null || costEntryPrefab == null)
                return;

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
            {
                if (go != null)
                    Destroy(go);
            }

            spawnedCostEntries.Clear();
        }

        private CurrencyData FindCurrency(CurrencyType type)
        {
            if (currencyDatabase == null)
                return null;

            foreach (CurrencyData cd in currencyDatabase)
            {
                if (cd != null && cd.type == type)
                    return cd;
            }

            return null;
        }

        private void HideImmediate()
        {
            if (root != null)
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

            if (rootCanvas == null)
            {
                MoveAboveTargetFallback(target);
                return;
            }

            RectTransform canvasRect = rootCanvas.transform as RectTransform;

            if (canvasRect == null)
            {
                MoveAboveTargetFallback(target);
                return;
            }

            Canvas targetCanvas = target.GetComponentInParent<Canvas>();
            Camera targetCamera = GetCanvasCamera(targetCanvas);
            Camera rootCamera = GetCanvasCamera(rootCanvas);

            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Vector3 topCenterWorld = (corners[1] + corners[2]) * 0.5f;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(targetCamera, topCenterWorld);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPoint,
                    rootCamera,
                    out Vector2 localPoint))
            {
                MoveAboveTargetFallback(target);
                return;
            }

            float tooltipWidth = tooltipRect.rect.width;
            float tooltipHeight = tooltipRect.rect.height;

            float x = localPoint.x;
            float y = localPoint.y + verticalOffset + tooltipHeight * tooltipRect.pivot.y;

            float minX = canvasRect.rect.xMin + tooltipWidth * tooltipRect.pivot.x;
            float maxX = canvasRect.rect.xMax - tooltipWidth * (1f - tooltipRect.pivot.x);

            float minY = canvasRect.rect.yMin + tooltipHeight * tooltipRect.pivot.y;
            float maxY = canvasRect.rect.yMax - tooltipHeight * (1f - tooltipRect.pivot.y);

            if (minX <= maxX)
                x = Mathf.Clamp(x, minX, maxX);

            if (minY <= maxY)
                y = Mathf.Clamp(y, minY, maxY);

            tooltipRect.anchoredPosition = new Vector2(x, y);
        }

        private Camera GetCanvasCamera(Canvas canvas)
        {
            if (canvas == null)
                return null;

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            return canvas.worldCamera;
        }

        private void MoveAboveTargetFallback(RectTransform target)
        {
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
            if (tooltipRect == null)
                return;

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