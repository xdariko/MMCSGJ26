using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyViewUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    public void Setup(CurrencyData data, int amount)
    {
        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.color = data.color;
        }

        SetAmount(amount);
    }

    public void SetAmount(int amount)
    {
        if (amountText != null)
            amountText.text = amount.ToString();
    }
}