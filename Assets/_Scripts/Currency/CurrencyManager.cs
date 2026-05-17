using System;
using System.Collections.Generic;

public static class CurrencyManager
{
    // Persistent across runs (total wallet)
    private static readonly Dictionary<CurrencyType, int> totalBalance = new();

    // Reset every run, used by death panel
    private static readonly Dictionary<CurrencyType, int> runCollected = new();

    // Which currency types are unlocked (can be picked up / dropped)
    private static readonly HashSet<CurrencyType> unlocked = new() { CurrencyType.Basic };

    public static event Action<CurrencyType, int> OnCurrencyChanged;

    public static bool IsUnlocked(CurrencyType type)
    {
        return type != CurrencyType.None && unlocked.Contains(type);
    }

    public static void Unlock(CurrencyType type)
    {
        if (type == CurrencyType.None) return;
        unlocked.Add(type);
    }

    public static int GetTotal(CurrencyType type)
    {
        return totalBalance.TryGetValue(type, out int v) ? v : 0;
    }

    public static int GetRunCollected(CurrencyType type)
    {
        return runCollected.TryGetValue(type, out int v) ? v : 0;
    }

    public static IEnumerable<CurrencyType> AllUnlocked()
    {
        return unlocked;
    }

    public static void Add(CurrencyType type, int amount)
    {
        if (!IsUnlocked(type) || amount <= 0) return;

        totalBalance.TryGetValue(type, out int total);
        totalBalance[type] = total + amount;

        runCollected.TryGetValue(type, out int run);
        runCollected[type] = run + amount;

        OnCurrencyChanged?.Invoke(type, totalBalance[type]);
    }

    public static bool Spend(CurrencyType type, int amount)
    {
        if (GetTotal(type) < amount) return false;

        totalBalance[type] -= amount;
        OnCurrencyChanged?.Invoke(type, totalBalance[type]);
        return true;
    }

    public static void ResetRunCollected()
    {
        runCollected.Clear();
    }
}
