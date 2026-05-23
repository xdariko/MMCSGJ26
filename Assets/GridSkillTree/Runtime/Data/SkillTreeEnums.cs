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
        UnlockCurrency,
        CurrencyDropPercent,
        PassiveCurrency,
        InvincibilityDuration,
        UnlockBombs,
        BombExplosionRadius,
        BombDamage,
        BombSpawnIntervalReduction,
        UnlockSprint
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
        Maxed
    }
}