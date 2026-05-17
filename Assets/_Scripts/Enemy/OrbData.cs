using UnityEngine;

[System.Serializable]
public class OrbData
{
    [Header("Effect")]
    public OrbEffectType effectType;

    public float value = 10f;

    [Tooltip("Only used when effectType = Currency")]
    public CurrencyType currencyType = CurrencyType.Basic;

    [Header("Movement")]
    public float attractSpeed = 8f;

    [Header("Visual")]
    public Color color = Color.white;
}