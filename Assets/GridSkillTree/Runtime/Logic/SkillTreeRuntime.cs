using System;
using UnityEngine;

namespace GridSkillTree
{
    public class SkillTreeRuntime : MonoBehaviour
    {
        [SerializeField] private SkillTreeData treeData;
        [SerializeField] private int startingSkillPoints = 10;

        public SkillTreeData TreeData => treeData;
        public SkillTreeProgress Progress { get; private set; }

        public event Action OnTreeChanged;

        private void Awake()
        {
            Progress = new SkillTreeProgress
            {
                skillPoints = startingSkillPoints
            };
        }

        public int GetLevel(string nodeId)
        {
            return Progress.GetLevel(nodeId);
        }

        public int GetCost(SkillNodeData node)
        {
            int currentLevel = GetLevel(node.id);
            return node.GetCost(currentLevel);
        }

        public bool IsUnlocked(SkillNodeData node)
        {
            if (node.previousNodeIds == null || node.previousNodeIds.Count == 0)
                return true;

            foreach (string previousNodeId in node.previousNodeIds)
            {
                if (GetLevel(previousNodeId) <= 0)
                    return false;
            }

            return true;
        }

        public bool CanBuy(SkillNodeData node)
        {
            if (node == null)
                return false;

            int currentLevel = GetLevel(node.id);

            if (currentLevel >= node.maxLevel)
                return false;

            if (!IsUnlocked(node))
                return false;

            int cost = GetCost(node);

            return Progress.skillPoints >= cost;
        }

        public bool Buy(SkillNodeData node)
        {
            if (!CanBuy(node))
                return false;

            int currentLevel = GetLevel(node.id);
            int cost = GetCost(node);

            Progress.skillPoints -= cost;
            Progress.SetLevel(node.id, currentLevel + 1);

            ApplyEffect(node, currentLevel + 1);

            OnTreeChanged?.Invoke();
            return true;
        }

        public SkillNodeVisualState GetVisualState(SkillNodeData node)
        {
            int level = GetLevel(node.id);

            if (level >= node.maxLevel)
                return SkillNodeVisualState.Maxed;

            if (level > 0)
                return SkillNodeVisualState.Purchased;

            if (CanBuy(node))
                return SkillNodeVisualState.Available;

            return SkillNodeVisualState.Locked;
        }

        private void ApplyEffect(SkillNodeData node, int newLevel)
        {
            float value = node.GetValue(newLevel);
            float prevValue = newLevel > 1 ? node.GetValue(newLevel - 1) : 0f;
            float delta = value - prevValue;

            switch (node.effectType)
            {
                case SkillEffectType.MoveSpeedPercent:
                    PlayerStats.BonusMoveSpeedPercent += delta;
                    break;
                case SkillEffectType.DamageFlat:
                    PlayerStats.BonusDamageFlat += delta;
                    break;
                case SkillEffectType.BeamCount:
                    PlayerStats.BonusBeamCount += Mathf.RoundToInt(delta);
                    break;
                case SkillEffectType.PickupRadius:
                    PlayerStats.BonusPickupRadius += delta;
                    break;
            }
        }
    }
}