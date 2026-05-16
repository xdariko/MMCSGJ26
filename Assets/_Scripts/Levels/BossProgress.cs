using System;
using UnityEngine;

public static class BossProgress
{
    public static int CurrentXP;
    public static int RequiredXP;
    public static bool BossSpawned;
    public static bool BossKilled;

    /// <summary> Fires when XP changes. Args: current, required. </summary>
    public static event Action<int, int> OnXPChanged;

    /// <summary> Fires when XP reaches threshold and boss should spawn. </summary>
    public static event Action OnBossReady;

    /// <summary> Fires when boss is killed. </summary>
    public static event Action OnBossDefeated;

    public static void Reset(int requiredXP)
    {
        CurrentXP = 0;
        RequiredXP = requiredXP;
        BossSpawned = false;
        BossKilled = false;
        OnXPChanged?.Invoke(0, requiredXP);
    }

    public static void AddXP(int amount)
    {
        if (BossSpawned) return;

        CurrentXP += amount;
        CurrentXP = Mathf.Min(CurrentXP, RequiredXP);
        OnXPChanged?.Invoke(CurrentXP, RequiredXP);

        if (CurrentXP >= RequiredXP && !BossSpawned)
        {
            BossSpawned = true;
            OnBossReady?.Invoke();
        }
    }

    public static void NotifyBossKilled()
    {
        BossKilled = true;
        OnBossDefeated?.Invoke();
    }
}
