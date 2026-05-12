using System;
using System.Collections.Generic;
using UnityEngine;

public static class G
{
    public static Main main;
    public static UI ui;
    public static GameObject player;

    public static bool IsPaused;
    public static bool IsMenuOpen;

    public static int Currency;

    public static void AddCurrency(int amount)
    {
        Currency += amount;
        Debug.Log("Currency: " + G.Currency);
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
