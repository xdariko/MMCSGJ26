using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyPanelUI : MonoBehaviour
{
    [SerializeField] private CurrencyViewUI viewPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private CurrencyData[] currencyData;

    private readonly Dictionary<CurrencyType, CurrencyViewUI> views = new();

    private void OnEnable()
    {
        CurrencyManager.OnCurrencyChanged += HandleCurrencyChanged;
        Rebuild();
        RefreshAll();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
    }

    private void Rebuild()
    {
        List<Transform> toRemove = new();

        foreach (Transform child in container)
        {
            if (child.GetComponent<CurrencyViewUI>() != null)
                toRemove.Add(child);
        }

        foreach (Transform child in toRemove)
            Destroy(child.gameObject);

        views.Clear();

        foreach (CurrencyData data in currencyData)
        {
            if (data == null)
                continue;

            if (!CurrencyManager.IsUnlocked(data.type))
                continue;

            CurrencyViewUI view = Instantiate(viewPrefab, container);

            view.Setup(
                data,
                CurrencyManager.GetTotal(data.type));

            views[data.type] = view;
        }
    }

    private void RefreshAll()
    {
        foreach (var pair in views)
            pair.Value.SetAmount(CurrencyManager.GetTotal(pair.Key));
    }

    private void HandleCurrencyChanged(CurrencyType type, int amount)
    {
        if (views.TryGetValue(type, out CurrencyViewUI view))
            view.SetAmount(amount);
        else
            Rebuild();
    }
}