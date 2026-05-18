using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridSkillTree
{
    public class SkillTreeRuntime : MonoBehaviour
    {
        private const string SaveKeyPrefix = "GridSkillTree.Progress.";

        [SerializeField] private SkillTreeData treeData;
        [SerializeField] private int startingSkillPoints = 10;
        [SerializeField] private bool loadSavedProgress = true;
        [SerializeField] private bool saveProgressOnBuy = true;

        public event Action<SkillNodeData> OnNodeUpgraded;
        public SkillTreeData TreeData => treeData;
        public SkillTreeProgress Progress { get; private set; }

        public event Action OnTreeChanged;

        private string SaveKey
        {
            get
            {
                string treeName = treeData != null ? treeData.name : "Default";
                return SaveKeyPrefix + treeName;
            }
        }

        private void Awake()
        {
            Progress = new SkillTreeProgress
            {
                skillPoints = startingSkillPoints
            };

            if (loadSavedProgress)
                LoadProgress();

            PlayerStats.ResetBonuses();
            ApplyAllPurchasedEffects();
        }

        public int GetLevel(string nodeId)
        {
            return Progress.GetLevel(nodeId);
        }

        public List<(CurrencyType currency, int amount)> GetCosts(SkillNodeData node)
        {
            List<(CurrencyType, int)> result = new();
            if (node == null) return result;

            int currentLevel = GetLevel(node.id);

            if (node.costs == null)
                return result;

            foreach (SkillCost c in node.costs)
            {
                if (c == null || c.currency == CurrencyType.None)
                    continue;

                int amount = c.GetAmount(currentLevel);
                if (amount > 0)
                    result.Add((c.currency, amount));
            }

            return result;
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

            foreach (var (currency, amount) in GetCosts(node))
            {
                if (CurrencyManager.GetTotal(currency) < amount)
                    return false;
            }

            return true;
        }

        public bool Buy(SkillNodeData node)
        {
            if (!CanBuy(node))
                return false;

            int currentLevel = GetLevel(node.id);

            foreach (var (currency, amount) in GetCosts(node))
                CurrencyManager.Spend(currency, amount);

            Progress.SetLevel(node.id, currentLevel + 1);

            ApplyEffect(node, currentLevel + 1);

            if (saveProgressOnBuy)
                SaveProgress();

            OnTreeChanged?.Invoke();
            OnNodeUpgraded?.Invoke(node);

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

        public void SaveProgress()
        {
            if (Progress == null)
                return;

            string json = JsonUtility.ToJson(Progress);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
                return;

            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return;

            SkillTreeProgress loaded = JsonUtility.FromJson<SkillTreeProgress>(json);
            if (loaded == null)
                return;

            if (loaded.nodeProgress == null)
                loaded.nodeProgress = new List<SkillNodeProgress>();

            Progress = loaded;
        }

        public void ResetSavedProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();

            Progress = new SkillTreeProgress
            {
                skillPoints = startingSkillPoints
            };

            PlayerStats.ResetBonuses();
            OnTreeChanged?.Invoke();
        }

        private void ApplyAllPurchasedEffects()
        {
            if (treeData == null || treeData.nodes == null || Progress == null)
                return;

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node == null)
                    continue;

                int level = Progress.GetLevel(node.id);
                if (level <= 0)
                    continue;

                ApplyEffect(node, level);
            }
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
                case SkillEffectType.CritChance:
                    PlayerStats.BonusCritChance += delta;
                    break;
                case SkillEffectType.CritMultiplier:
                    PlayerStats.BonusCritMultiplier += delta;
                    break;
                case SkillEffectType.StabilityDecayReduction:
                    PlayerStats.BonusStabilityDecayReduction += delta;
                    break;
                case SkillEffectType.UnlockCurrency:
                    CurrencyManager.Unlock(node.unlockCurrencyType);
                    break;
                case SkillEffectType.CurrencyDropPercent:
                    PlayerStats.AddCurrencyDropBonus(node.currencyDropType, delta);
                    break;
                case SkillEffectType.PassiveCurrency:
                    PlayerStats.AddPassiveCurrencyReward(
                        node.passiveCurrencyType,
                        Mathf.RoundToInt(delta),
                        node.passiveCurrencyIntervalSeconds);
                    break;
                case SkillEffectType.InvincibilityDuration:
                    PlayerStats.BonusInvincibilityDuration += delta;
                    break;
            }
        }
    }
}
