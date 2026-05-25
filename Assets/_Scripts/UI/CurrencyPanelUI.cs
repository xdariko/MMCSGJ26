using System.Collections.Generic;
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
        CurrencyManager.OnCurrencyUnlocked += HandleCurrencyUnlocked;

        Rebuild();
        RefreshAll();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
        CurrencyManager.OnCurrencyUnlocked -= HandleCurrencyUnlocked;
    }

    private void Rebuild()
    {
        if (viewPrefab == null || container == null)
            return;

        ClearViews();

        foreach (CurrencyType type in CurrencyManager.AllUnlocked())
        {
            if (type == CurrencyType.None)
                continue;

            CurrencyData data = FindCurrencyData(type);

            if (data == null)
            {
                Debug.LogWarning($"CurrencyPanelUI: CurrencyData не найден для валюты {type}. Добавь CurrencyData в массив Currency Data.");
                continue;
            }

            CurrencyViewUI view = Instantiate(viewPrefab, container);

            view.Setup(
                data,
                CurrencyManager.GetTotal(type)
            );

            views[type] = view;
        }
    }

    private void ClearViews()
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
    }

    private void RefreshAll()
    {
        foreach (var pair in views)
            pair.Value.SetAmount(CurrencyManager.GetTotal(pair.Key));
    }

    private void HandleCurrencyUnlocked(CurrencyType type)
    {
        Rebuild();
        RefreshAll();
    }

    private void HandleCurrencyChanged(CurrencyType type, int amount)
    {
        if (views.TryGetValue(type, out CurrencyViewUI view))
        {
            view.SetAmount(amount);
            return;
        }

        if (CurrencyManager.IsUnlocked(type))
        {
            Rebuild();
            RefreshAll();
        }
    }

    private CurrencyData FindCurrencyData(CurrencyType type)
    {
        if (currencyData == null)
            return null;

        foreach (CurrencyData data in currencyData)
        {
            if (data != null && data.type == type)
                return data;
        }

        return null;
    }
}