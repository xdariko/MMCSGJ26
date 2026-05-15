using System;
using System.Collections.Generic;
using UnityEngine;

public static class G
{
    public static Main main;
    public static UI ui;
    public static GameObject player;
    public static PlayerStabilitySystem stability;
    public static WaveDirector waveDirector;
    public static SpawnArea spawnArea;

    public static bool IsPaused;
    public static bool IsMenuOpen;
    public static bool IsPlayerDead;

    public static GameObject damagePopupPrefab;

    public static int Currency;

    public static void AddCurrency(int amount)
    {
        Currency += amount;
        Debug.Log("Currency: " + G.Currency);
    }
}

public static class PlayerStats
{
    public static float BaseMoveSpeed;
    public static float BaseDamage;
    public static int BaseBeamCount;
    public static float BasePickupRadius;

    public static float BonusMoveSpeedPercent;
    public static float BonusDamageFlat;
    public static int BonusBeamCount;
    public static float BonusPickupRadius;

    public static float MoveSpeed => BaseMoveSpeed * (1f + BonusMoveSpeedPercent);
    public static float Damage => BaseDamage + BonusDamageFlat;
    public static int BeamCount => BaseBeamCount + BonusBeamCount;
    public static float PickupRadius => BasePickupRadius + BonusPickupRadius;

    public static void ResetBonuses()
    {
        BonusMoveSpeedPercent = 0f;
        BonusDamageFlat = 0f;
        BonusBeamCount = 0;
        BonusPickupRadius = 0f;
    }
}


public class ManagedBehaviour : MonoBehaviour
{
    void Update()
    {
        if (!G.IsPaused)
            PausableUpdate();
    }

    void FixedUpdate()
    {
        if (!G.IsPaused)
            PausableFixedUpdate();
    }

    protected virtual void PausableUpdate() { }
    protected virtual void PausableFixedUpdate() { }
}
