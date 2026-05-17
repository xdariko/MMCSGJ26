using UnityEngine;

[CreateAssetMenu(menuName = "Currency/Currency Data")]
public class CurrencyData : ScriptableObject
{
    public CurrencyType type;
    public string displayName;
    public Sprite icon;
    public Color color = Color.white;
}
