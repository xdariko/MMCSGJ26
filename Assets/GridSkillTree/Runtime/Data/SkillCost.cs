using System;
using UnityEngine;

namespace GridSkillTree
{
    [Serializable]
    public class SkillCost
    {
        public CurrencyType currency = CurrencyType.Basic;
        public int baseAmount = 1;
        public CostFormulaType formula = CostFormulaType.Constant;

        public int GetAmount(int currentLevel)
        {
            return formula switch
            {
                CostFormulaType.Constant => baseAmount,
                CostFormulaType.Linear => baseAmount * (currentLevel + 1),
                CostFormulaType.Exponential => Mathf.RoundToInt(baseAmount * Mathf.Pow(1.5f, currentLevel)),
                _ => baseAmount
            };
        }
    }
}
