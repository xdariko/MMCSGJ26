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

            RebuildAllBonusesFromProgress();
        }

        public int GetLevel(string nodeId)
        {
            return Progress.GetLevel(nodeId);
        }

        public List<(CurrencyType currency, int amount)> GetCosts(SkillNodeData node)
        {
            List<(CurrencyType, int)> result = new();

            if (node == null)
                return result;

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
            if (node == null)
                return false;

            if (node.previousNodeIds == null || node.previousNodeIds.Count == 0)
                return true;

            foreach (string previousNodeId in node.previousNodeIds)
            {
                if (GetLevel(previousNodeId) <= 0)
                    return false;
            }

            return true;
        }

        public bool ShouldShowNode(SkillNodeData node)
        {
            if (node == null)
                return false;

            if (node.previousNodeIds == null || node.previousNodeIds.Count == 0)
                return true;

            if (GetLevel(node.id) > 0)
                return true;

            return IsUnlocked(node);
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

            RebuildAllBonusesFromProgress();

            if (saveProgressOnBuy)
                SaveProgress();

            OnTreeChanged?.Invoke();
            OnNodeUpgraded?.Invoke(node);

            return true;
        }

        public SkillNodeVisualState GetVisualState(SkillNodeData node)
        {
            if (node == null)
                return SkillNodeVisualState.Locked;

            int level = GetLevel(node.id);

            if (level >= node.maxLevel)
                return SkillNodeVisualState.Maxed;

            if (CanBuy(node))
                return SkillNodeVisualState.Available;

            return SkillNodeVisualState.Locked;
        }

        public string GetStatusText(SkillNodeData node)
        {
            SkillNodeVisualState state = GetVisualState(node);

            return state switch
            {
                SkillNodeVisualState.Locked => "Недоступно",
                SkillNodeVisualState.Available => "Можно улучшить",
                SkillNodeVisualState.Maxed => "Улучшено полностью",
                _ => ""
            };
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

            RebuildAllBonusesFromProgress();

            OnTreeChanged?.Invoke();
        }

        private void RebuildAllBonusesFromProgress()
        {
            PlayerStats.ResetBonuses();

            if (treeData == null || treeData.nodes == null || Progress == null)
                return;

            foreach (SkillNodeData node in treeData.nodes)
            {
                if (node == null)
                    continue;

                int level = Progress.GetLevel(node.id);

                if (level <= 0)
                    continue;

                ApplyFullEffect(node, level);
            }
        }

        private void ApplyFullEffect(SkillNodeData node, int level)
        {
            float value = node.GetValue(level);

            switch (node.effectType)
            {
                case SkillEffectType.MoveSpeedPercent:
                    PlayerStats.BonusMoveSpeedPercent += value;
                    break;

                case SkillEffectType.DamageFlat:
                    PlayerStats.BonusDamageFlat += value;
                    break;

                case SkillEffectType.BeamCount:
                    PlayerStats.BonusBeamCount += Mathf.RoundToInt(value);
                    break;

                case SkillEffectType.PickupRadius:
                    PlayerStats.BonusPickupRadius += value;
                    break;

                case SkillEffectType.CritChance:
                    PlayerStats.BonusCritChance += value;
                    break;

                case SkillEffectType.CritMultiplier:
                    PlayerStats.BonusCritMultiplier += value;
                    break;

                case SkillEffectType.StabilityDecayReduction:
                    PlayerStats.BonusStabilityDecayReduction += value;
                    break;

                case SkillEffectType.UnlockCurrency:
                    CurrencyManager.Unlock(node.unlockCurrencyType);
                    break;

                case SkillEffectType.CurrencyDropPercent:
                    PlayerStats.AddCurrencyDropBonus(node.currencyDropType, value);
                    break;

                case SkillEffectType.PassiveCurrency:
                    PlayerStats.AddPassiveCurrencyReward(
                        node.passiveCurrencyType,
                        Mathf.RoundToInt(value),
                        node.passiveCurrencyIntervalSeconds
                    );
                    break;

                case SkillEffectType.InvincibilityDuration:
                    PlayerStats.BonusInvincibilityDuration += value;
                    break;

                case SkillEffectType.UnlockBombs:
                    PlayerStats.UnlockBombs();
                    break;

                case SkillEffectType.BombExplosionRadius:
                    PlayerStats.AddBombExplosionRadius(value);
                    break;

                case SkillEffectType.BombDamage:
                    PlayerStats.AddBombDamage(value);
                    break;

                case SkillEffectType.BombSpawnIntervalReduction:
                    PlayerStats.AddBombSpawnIntervalReduction(value);
                    break;

                case SkillEffectType.UnlockSprint:
                    PlayerStats.UnlockSprint();
                    break;
            }
        }
    }
}