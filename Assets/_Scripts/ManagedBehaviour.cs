using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class G
{
    public static Main main;
    public static UI ui;
    public static MusicManager music;
    public static GameObject player;
    public static PlayerStabilitySystem stability;
    public static WaveDirector waveDirector;
    public static SpawnArea spawnArea;
    public static ScreenDustTransition transition;

    public static bool IsPaused;
    public static bool IsMenuOpen;
    public static bool IsPlayerDead;

    public static GameObject damagePopupPrefab;
    public static LevelDatabase levelDatabase;

    public static void ResetRuntimeFlags()
    {
        IsPaused = false;
        IsMenuOpen = false;
        IsPlayerDead = false;
    }

    public static void ClearSceneReferences()
    {
        main = null;
        ui = null;
        music = null;
        player = null;
        stability = null;
        waveDirector = null;
        spawnArea = null;
        damagePopupPrefab = null;
        levelDatabase = null;
        transition = null;
    }
}

public static class LevelProgress
{
    public static int UnlockedLevel;
    public static int SelectedLevel;

    public static void Reset()
    {
        UnlockedLevel = 0;
        SelectedLevel = 0;
    }

    public static void CompleteCurrentLevel()
    {
        if (SelectedLevel >= UnlockedLevel)
            UnlockedLevel = SelectedLevel + 1;
    }
}

public static class PlayerStats
{
    public static float BaseMoveSpeed;
    public static float BaseDamage;
    public static int BaseBeamCount;
    public static float BasePickupRadius;
    public static float BaseStabilityDecay;
    public static float BaseCritChance;
    public static float BaseCritMultiplier = 2f;

    public static float BonusMoveSpeedPercent;
    public static float BonusDamageFlat;
    public static int BonusBeamCount;
    public static float BonusPickupRadius;
    public static float BonusStabilityDecayReduction;
    public static float BonusCritChance;
    public static float BonusCritMultiplier;

    private static readonly Dictionary<CurrencyType, float> CurrencyDropBonusPercent = new();

    // Нужен, чтобы бонус +10%, +15%, +20% не терялся на орбах, которые дают по 1 ресурсу.
    private static readonly Dictionary<CurrencyType, float> CurrencyDropRoundingCarry = new();

    private static readonly Dictionary<CurrencyType, int> PassiveCurrencyAmounts = new();

    public static float PassiveCurrencyIntervalSeconds = 3f;
    public static float BonusInvincibilityDuration;

    public static bool BombsUnlocked;
    public static float BombSpawnInterval = 3f;
    public static float BombExplosionRadius = 2f;
    public static float BombDamage = 5f;

    public static bool SprintUnlocked;
    public static float SprintMoveSpeedMultiplier = 3f;

    public static float MoveSpeed => BaseMoveSpeed * (1f + BonusMoveSpeedPercent);
    public static float SprintMoveSpeed => MoveSpeed * SprintMoveSpeedMultiplier;
    public static float Damage => BaseDamage + BonusDamageFlat;
    public static int BeamCount => BaseBeamCount + BonusBeamCount;
    public static float PickupRadius => BasePickupRadius + BonusPickupRadius;
    public static float StabilityDecay => Mathf.Max(0f, BaseStabilityDecay - BonusStabilityDecayReduction);
    public static float CritChance => Mathf.Clamp01(BaseCritChance + BonusCritChance);
    public static float CritMultiplier => BaseCritMultiplier + BonusCritMultiplier;

    public static void AddCurrencyDropBonus(CurrencyType currencyType, float bonusPercent)
    {
        if (currencyType == CurrencyType.None)
            return;

        if (!CurrencyDropBonusPercent.ContainsKey(currencyType))
            CurrencyDropBonusPercent[currencyType] = 0f;

        CurrencyDropBonusPercent[currencyType] += bonusPercent;
    }

    public static float GetCurrencyDropMultiplier(CurrencyType currencyType)
    {
        if (currencyType == CurrencyType.None)
            return 1f;

        CurrencyDropBonusPercent.TryGetValue(currencyType, out float bonusPercent);

        return Mathf.Max(0f, 1f + bonusPercent);
    }

    public static int ApplyCurrencyDropMultiplier(CurrencyType currencyType, int baseAmount)
    {
        if (baseAmount <= 0)
            return 0;

        if (currencyType == CurrencyType.None)
            return baseAmount;

        float multiplier = GetCurrencyDropMultiplier(currencyType);

        if (multiplier <= 0f)
            return 0;

        CurrencyDropRoundingCarry.TryGetValue(currencyType, out float carry);

        float exactAmount = baseAmount * multiplier + carry;

        // Округляем накопленно, а не каждый орб отдельно.
        // Так бонус +15% не теряется на ресурсах по 1 штуке.
        int finalAmount = Mathf.FloorToInt(exactAmount + 0.5f);

        finalAmount = Mathf.Max(1, finalAmount);

        CurrencyDropRoundingCarry[currencyType] = exactAmount - finalAmount;

        return finalAmount;
    }

    public static void ResetCurrencyDropRoundingCarry()
    {
        CurrencyDropRoundingCarry.Clear();
    }

    public static void AddPassiveCurrencyReward(CurrencyType currencyType, int amount, float intervalSeconds)
    {
        if (currencyType == CurrencyType.None || amount <= 0)
            return;

        if (!PassiveCurrencyAmounts.ContainsKey(currencyType))
            PassiveCurrencyAmounts[currencyType] = 0;

        PassiveCurrencyAmounts[currencyType] += amount;

        if (intervalSeconds > 0f)
            PassiveCurrencyIntervalSeconds = Mathf.Max(0.1f, intervalSeconds);
    }

    public static bool HasPassiveCurrencyRewards()
    {
        return PassiveCurrencyAmounts.Count > 0;
    }

    public static IReadOnlyDictionary<CurrencyType, int> GetPassiveCurrencyRewards()
    {
        return PassiveCurrencyAmounts;
    }

    public static float GetInvincibilityDuration(float baseDuration)
    {
        return Mathf.Max(0f, baseDuration + BonusInvincibilityDuration);
    }

    public static void UnlockBombs()
    {
        BombsUnlocked = true;
    }

    public static void AddBombExplosionRadius(float amount)
    {
        BombExplosionRadius = Mathf.Max(0.1f, BombExplosionRadius + amount);
    }

    public static void AddBombDamage(float amount)
    {
        BombDamage = Mathf.Max(0f, BombDamage + amount);
    }

    public static void AddBombSpawnIntervalReduction(float amount)
    {
        BombSpawnInterval = Mathf.Max(0.25f, BombSpawnInterval - amount);
    }

    public static void ResetBombStats()
    {
        BombsUnlocked = false;
        BombSpawnInterval = 3f;
        BombExplosionRadius = 2f;
        BombDamage = 5f;
    }

    public static void UnlockSprint()
    {
        SprintUnlocked = true;
    }

    public static void ResetSprintStats()
    {
        SprintUnlocked = false;
        SprintMoveSpeedMultiplier = 2f;
    }

    public static void ResetBaseStats()
    {
        BaseMoveSpeed = 0f;
        BaseDamage = 0f;
        BaseBeamCount = 0;
        BasePickupRadius = 0f;
        BaseStabilityDecay = 0f;
        BaseCritChance = 0f;
        BaseCritMultiplier = 2f;
    }

    public static void ResetBonuses()
    {
        BonusMoveSpeedPercent = 0f;
        BonusDamageFlat = 0f;
        BonusBeamCount = 0;
        BonusPickupRadius = 0f;
        BonusStabilityDecayReduction = 0f;
        BonusCritChance = 0f;
        BonusCritMultiplier = 0f;
        BonusInvincibilityDuration = 0f;

        PassiveCurrencyIntervalSeconds = 3f;

        CurrencyDropBonusPercent.Clear();
        CurrencyDropRoundingCarry.Clear();
        PassiveCurrencyAmounts.Clear();

        ResetBombStats();
        ResetSprintStats();
    }
}

public static class GameResetUtility
{
    public static void ResetAllProgressAndReload()
    {
        ResetAllRuntimeState();

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void ResetAllRuntimeState()
    {
        G.ResetRuntimeFlags();
        LevelProgress.Reset();

        PlayerStats.ResetBonuses();
        PlayerStats.ResetBaseStats();

        CurrencyManager.ResetAll();
    }

    private static void TryResetCurrencyManager()
    {
        Type currencyManagerType = FindType("CurrencyManager");

        if (currencyManagerType == null)
            return;

        string[] resetMethodNames =
        {
            "ResetAll",
            "Reset",
            "Clear",
            "ClearAll",
            "ResetCurrencies",
            "ResetRunCollected"
        };

        foreach (string methodName in resetMethodNames)
        {
            MethodInfo method = currencyManagerType.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null
            );

            if (method == null)
                continue;

            method.Invoke(null, null);
        }
    }

    private static Type FindType(string typeName)
    {
        Type directType = Type.GetType(typeName);

        if (directType != null)
            return directType;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type foundType = assembly.GetType(typeName);

            if (foundType != null)
                return foundType;
        }

        return null;
    }
}

public class ManagedBehaviour : MonoBehaviour
{
    private void Update()
    {
        if (!G.IsPaused)
            PausableUpdate();
    }

    private void FixedUpdate()
    {
        if (!G.IsPaused)
            PausableFixedUpdate();
    }

    protected virtual void PausableUpdate() { }
    protected virtual void PausableFixedUpdate() { }
}