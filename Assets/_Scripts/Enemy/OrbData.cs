using UnityEngine;

[System.Serializable]
public class OrbData
{
    [Header("Effects")]
    public OrbEffect[] effects;

    [Header("Movement")]
    public float attractSpeed = 8f;

    [Header("Visual")]
    public Color color = Color.white;
}

[System.Serializable]
public class OrbEffect
{
    public OrbEffectType effectType;

    public float value = 10f;

    [Tooltip("Only used when effectType = Currency")]
    public CurrencyType currencyType = CurrencyType.Basic;
}