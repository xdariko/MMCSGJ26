using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GridSkillTree
{
    public class SkillNodeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject purchasedMark;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;

        private SkillNodeData node;
        private SkillTreeRuntime runtime;
        private SkillTreeUIConfig config;
        private SkillTooltip tooltip;

        public SkillNodeData Node => node;
        public string NodeId => node != null ? node.id : string.Empty;

        public void Init(
            SkillNodeData node,
            SkillTreeRuntime runtime,
            SkillTreeUIConfig config,
            SkillTooltip tooltip)
        {
            this.node = node;
            this.runtime = runtime;
            this.config = config;
            this.tooltip = tooltip;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClick);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (node == null || runtime == null)
                return;

            if (iconImage != null)
                iconImage.sprite = node.icon;

            int level = runtime.GetLevel(node.id);

            if (levelText != null)
                levelText.text = $"{level}/{node.maxLevel}";

            if (costText != null)
            {
                if (level >= node.maxLevel)
                {
                    costText.text = "MAX";
                }
                else
                {
                    var costs = runtime.GetCosts(node);

                    if (costs.Count == 0)
                    {
                        costText.text = "";
                    }
                    else if (costs.Count == 1)
                    {
                        costText.text = costs[0].amount.ToString();
                    }
                    else
                    {
                        string[] parts = new string[costs.Count];

                        for (int i = 0; i < costs.Count; i++)
                            parts[i] = costs[i].amount.ToString();

                        costText.text = string.Join(" + ", parts);
                    }
                }
            }

            SkillNodeVisualState state = runtime.GetVisualState(node);
            ApplyState(state);
        }

        private void ApplyState(SkillNodeVisualState state)
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = state switch
                {
                    SkillNodeVisualState.Locked => config.lockedColor,
                    SkillNodeVisualState.Available => config.availableColor,
                    SkillNodeVisualState.Maxed => config.maxedColor,
                    _ => Color.white
                };
            }

            if (lockIcon != null)
                lockIcon.SetActive(state == SkillNodeVisualState.Locked);

            if (purchasedMark != null)
                purchasedMark.SetActive(state == SkillNodeVisualState.Maxed);

            if (button != null)
                button.interactable = state == SkillNodeVisualState.Available;
        }

        private void OnClick()
        {
            runtime.Buy(node);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltip != null && node != null && runtime != null)
                tooltip.Show(node, runtime, GetComponent<RectTransform>());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void OnDisable()
        {
            HideTooltip();
        }

        private void HideTooltip()
        {
            if (tooltip != null)
                tooltip.Hide();
        }
    }
}