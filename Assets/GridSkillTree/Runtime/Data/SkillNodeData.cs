using System;
using System.Collections.Generic;
using UnityEngine;

namespace GridSkillTree
{
    [Serializable]
    public class SkillNodeData
    {
        [Header("Identity")]
        public string id;
        public string title;

        [TextArea(3, 8)]
        public string description;

        [Header("Grid")]
        public Vector2Int gridPosition;

        [Header("Visual")]
        public Sprite icon;

        [Header("Connections")]
        public List<string> previousNodeIds = new();

        [Header("Levels")]
        public int maxLevel = 1;

        [Header("Cost")]
        public List<SkillCost> costs = new();

        [Header("Effect")]
        public SkillEffectType effectType = SkillEffectType.None;
        public float baseValue = 1f;
        public GrowthFormulaType growthFormula = GrowthFormulaType.Constant;

        [Tooltip("Only used when effectType = UnlockCurrency.")]
        public CurrencyType unlockCurrencyType = CurrencyType.None;

        [Tooltip("Only used when effectType = CurrencyDropPercent. This bonus is applied globally to every orb of this currency, no matter which enemy dropped it.")]
        public CurrencyType currencyDropType = CurrencyType.Basic;

        [Tooltip("Only used when effectType = PassiveCurrency. Currency that is granted while the player is alive.")]
        public CurrencyType passiveCurrencyType = CurrencyType.Basic;

        [Tooltip("Only used when effectType = PassiveCurrency. Time in seconds between passive currency payouts.")]
        public float passiveCurrencyIntervalSeconds = 3f;

        public float GetValue(int level)
        {
            return growthFormula switch
            {
                GrowthFormulaType.Constant => baseValue,
                GrowthFormulaType.Linear => baseValue * level,
                GrowthFormulaType.PercentLinear => baseValue * level / 100f,
                GrowthFormulaType.Exponential => baseValue * Mathf.Pow(1.25f, level - 1),
                _ => baseValue
            };
        }
    }
}
