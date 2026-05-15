using TMPro;
using UnityEngine;

namespace GridSkillTree
{
    public class SkillTooltip : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float verticalOffset = 10f;

        private void Awake()
        {
            Hide();
        }

        public void Show(SkillNodeData node, SkillTreeRuntime runtime, RectTransform target)
        {
            if (node == null || runtime == null || target == null)
                return;

            int level = runtime.GetLevel(node.id);
            int cost = runtime.GetCost(node);
            SkillNodeVisualState state = runtime.GetVisualState(node);

            if (root != null)
                root.SetActive(true);

            if (titleText != null)
                titleText.text = node.title;

            if (descriptionText != null)
                descriptionText.text = node.description;

            if (levelText != null)
                levelText.text = $"Level: {level}/{node.maxLevel}";

            if (costText != null)
                costText.text = level >= node.maxLevel ? "Cost: MAX" : $"Cost: {cost}";

            if (statusText != null)
                statusText.text = $"Status: {state}";

            MoveAboveTarget(target);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void MoveAboveTarget(RectTransform target)
        {
            RectTransform tooltipRect = root.GetComponent<RectTransform>();

            if (tooltipRect == null)
                return;

            Canvas.ForceUpdateCanvases();

            Vector2 targetAnchoredPosition = target.anchoredPosition;

            float targetHeight = target.rect.height;
            float tooltipHeight = tooltipRect.rect.height;

            float x = targetAnchoredPosition.x;
            float y = targetAnchoredPosition.y + targetHeight / 2f + tooltipHeight / 2f + verticalOffset;

            tooltipRect.anchoredPosition = new Vector2(x, y);
        }
    }
}