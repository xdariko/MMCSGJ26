using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathPanelUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CurrencyData[] currencyDatabase;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform entriesParent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private GameObject emptyLabel;
    [SerializeField] private Button continueButton;

    private readonly List<GameObject> spawnedEntries = new();

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    public void Show()
    {
        if (panel == null) return;

        panel.SetActive(true);
        Populate();
    }

    public void Hide()
    {
        if (panel == null) return;

        panel.SetActive(false);
        ClearEntries();
    }

    private void Populate()
    {
        ClearEntries();

        bool anyCollected = false;

        foreach (CurrencyData data in currencyDatabase)
        {
            if (data == null) continue;

            int collected = CurrencyManager.GetRunCollected(data.type);
            if (collected <= 0) continue;

            anyCollected = true;
            SpawnEntry(data, collected);
        }

        if (emptyLabel != null)
            emptyLabel.SetActive(!anyCollected);
    }

    private void SpawnEntry(CurrencyData data, int amount)
    {
        if (entryPrefab == null || entriesParent == null) return;

        GameObject go = Instantiate(entryPrefab, entriesParent);
        go.SetActive(true);
        spawnedEntries.Add(go);

        Image icon = go.GetComponentInChildren<Image>();
        if (icon != null)
        {
            icon.sprite = data.icon;
            icon.color = data.color;
        }

        TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = amount.ToString();
    }

    private void ClearEntries()
    {
        foreach (GameObject go in spawnedEntries)
            if (go != null) Destroy(go);

        spawnedEntries.Clear();
    }

    private void OnContinueClicked()
    {
        Hide();

        if (G.ui != null)
            G.ui.ShowSkillTreePanel();
    }
}
