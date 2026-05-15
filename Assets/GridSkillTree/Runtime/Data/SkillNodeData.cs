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
        public int baseCost = 1;
        public CostFormulaType costFormula = CostFormulaType.Constant;

        [Header("Effect")]
        public SkillEffectType effectType = SkillEffectType.None;
        public float baseValue = 1f;
        public GrowthFormulaType growthFormula = GrowthFormulaType.Constant;

        public int GetCost(int currentLevel)
        {
            return costFormula switch
            {
                CostFormulaType.Constant => baseCost,
                CostFormulaType.Linear => baseCost * (currentLevel + 1),
                CostFormulaType.Exponential => Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, currentLevel)),
                _ => baseCost
            };
        }

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