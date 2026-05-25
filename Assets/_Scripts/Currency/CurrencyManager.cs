using System;
using System.Collections.Generic;

public static class CurrencyManager
{
    private static readonly Dictionary<CurrencyType, int> totalBalance = new();
    private static readonly Dictionary<CurrencyType, int> runCollected = new();

    private static readonly HashSet<CurrencyType> unlocked = new()
    {
        CurrencyType.Basic
    };

    public static event Action<CurrencyType, int> OnCurrencyChanged;
    public static event Action<CurrencyType> OnCurrencyUnlocked;

    public static bool IsUnlocked(CurrencyType type)
    {
        return type != CurrencyType.None && unlocked.Contains(type);
    }

    public static void Unlock(CurrencyType type)
    {
        if (type == CurrencyType.None)
            return;

        bool wasAdded = unlocked.Add(type);

        if (!totalBalance.ContainsKey(type))
            totalBalance[type] = 0;

        if (wasAdded)
            OnCurrencyUnlocked?.Invoke(type);

        OnCurrencyChanged?.Invoke(type, GetTotal(type));
    }

    public static int GetTotal(CurrencyType type)
    {
        return totalBalance.TryGetValue(type, out int value) ? value : 0;
    }

    public static int GetRunCollected(CurrencyType type)
    {
        return runCollected.TryGetValue(type, out int value) ? value : 0;
    }

    public static IReadOnlyCollection<CurrencyType> AllUnlocked()
    {
        return unlocked;
    }

    public static IReadOnlyDictionary<CurrencyType, int> GetRunCollectedSnapshot()
    {
        return runCollected;
    }

    public static void Add(CurrencyType type, int amount)
    {
        if (type == CurrencyType.None)
            return;

        if (amount <= 0)
            return;

        if (!IsUnlocked(type))
            return;

        totalBalance.TryGetValue(type, out int total);
        totalBalance[type] = total + amount;

        runCollected.TryGetValue(type, out int run);
        runCollected[type] = run + amount;

        OnCurrencyChanged?.Invoke(type, totalBalance[type]);
    }

    public static bool Spend(CurrencyType type, int amount)
    {
        if (type == CurrencyType.None)
            return false;

        if (amount <= 0)
            return true;

        if (GetTotal(type) < amount)
            return false;

        totalBalance[type] -= amount;

        OnCurrencyChanged?.Invoke(type, totalBalance[type]);

        return true;
    }

    public static void ResetRunCollected()
    {
        runCollected.Clear();

        foreach (CurrencyType type in unlocked)
            OnCurrencyChanged?.Invoke(type, GetTotal(type));
    }

    public static void ResetAll()
    {
        totalBalance.Clear();
        runCollected.Clear();

        unlocked.Clear();
        unlocked.Add(CurrencyType.Basic);

        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            if (type == CurrencyType.None)
                continue;

            OnCurrencyChanged?.Invoke(type, 0);
        }

        OnCurrencyUnlocked?.Invoke(CurrencyType.Basic);
    }
}