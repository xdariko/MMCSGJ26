namespace GridSkillTree
{
    public enum SkillEffectType
    {
        None,
        MoveSpeedPercent,
        DamageFlat,
        BeamCount,
        PickupRadius,
        CritChance,
        CritMultiplier,
        StabilityDecayReduction,
        UnlockCurrency
    }

    public enum CostFormulaType
    {
        Constant,
        Linear,
        Exponential
    }

    public enum GrowthFormulaType
    {
        Constant,
        Linear,
        PercentLinear,
        Exponential
    }

    public enum SkillNodeVisualState
    {
        Locked,
        Available,
        Purchased,
        Maxed
    }
}