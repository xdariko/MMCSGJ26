using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathPanelUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CurrencyData[] currencyDatabase;

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private string deathTitle = "Поражение";
    [SerializeField] private string victoryTitle = "Победа";
    [SerializeField] private Transform entriesParent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private GameObject emptyLabel;
    [SerializeField] private Button continueButton;

    [Header("Count Animation")]
    [SerializeField] private bool animateNumbers = true;
    [SerializeField] private float minCountDuration = 0.35f;
    [SerializeField] private float maxCountDuration = 1.1f;
    [SerializeField] private float amountDurationMultiplier = 0.015f;
    [SerializeField] private float entryStartDelay = 0.12f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Entry Pop Animation")]
    [SerializeField] private bool animateEntryAppear = true;
    [SerializeField] private float entryAppearDuration = 0.18f;
    [SerializeField] private float entryStartScale = 0.75f;
    [SerializeField] private float entryPunchScale = 0.15f;

    [Header("Tick Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickClip;

    [Range(0f, 1f)]
    [SerializeField] private float tickVolume = 0.6f;

    [SerializeField] private float minTickInterval = 0.035f;
    [SerializeField] private int tickEveryAmount = 1;

    [Header("Final Sound")]
    [SerializeField] private AudioClip finishClip;

    [Range(0f, 1f)]
    [SerializeField] private float finishVolume = 0.75f;

    private readonly List<GameObject> spawnedEntries = new();
    private readonly List<Tween> activeTweens = new();

    private float lastTickTime;
    private int activeCountAnimations;
    private bool currentPanelIsDeath;

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

        KillTweens();
    }

    public void ShowDeath()
    {
        currentPanelIsDeath = true;

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusicImmediate();

        Show(deathTitle);
    }

    public void ShowVictory()
    {
        currentPanelIsDeath = false;

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusicImmediate();

        Show(victoryTitle);
    }

    public void Show()
    {
        ShowDeath();
    }

    public bool IsOpen()
    {
        return panel != null && panel.activeInHierarchy;
    }

    public bool IsDeathOpen()
    {
        return IsOpen() && currentPanelIsDeath;
    }

    private void Show(string title)
    {
        if (panel == null)
            return;

        KillTweens();

        if (titleText != null)
            titleText.text = title;

        panel.SetActive(true);

        if (continueButton != null)
            continueButton.interactable = false;

        Populate();
    }

    public void Hide()
    {
        if (panel == null)
            return;

        KillTweens();

        panel.SetActive(false);
        currentPanelIsDeath = false;

        ClearEntries();
    }

    private void Populate()
    {
        ClearEntries();

        bool anyCollected = false;
        int visibleEntryIndex = 0;

        IReadOnlyDictionary<CurrencyType, int> collectedSnapshot =
            CurrencyManager.GetRunCollectedSnapshot();

        foreach (var pair in collectedSnapshot)
        {
            CurrencyType type = pair.Key;
            int collected = pair.Value;

            if (type == CurrencyType.None)
                continue;

            if (collected <= 0)
                continue;

            CurrencyData data = FindCurrencyData(type);

            if (data == null)
            {
                Debug.LogWarning($"DeathPanelUI: CurrencyData не найден для валюты {type}. Добавь CurrencyData в Currency Database.");
                continue;
            }

            anyCollected = true;

            SpawnEntry(data, collected, visibleEntryIndex);

            visibleEntryIndex++;
        }

        if (emptyLabel != null)
            emptyLabel.SetActive(!anyCollected);

        if (!anyCollected)
        {
            if (continueButton != null)
                continueButton.interactable = true;
        }
    }

    private CurrencyData FindCurrencyData(CurrencyType type)
    {
        if (currencyDatabase == null)
            return null;

        foreach (CurrencyData data in currencyDatabase)
        {
            if (data != null && data.type == type)
                return data;
        }

        return null;
    }

    private void SpawnEntry(CurrencyData data, int amount, int index)
    {
        if (entryPrefab == null || entriesParent == null)
            return;

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
        {
            if (animateNumbers)
                label.text = "0";
            else
                label.text = amount.ToString();
        }

        RectTransform rect = go.GetComponent<RectTransform>();

        if (animateEntryAppear && rect != null)
            PlayEntryAppear(rect, index);

        if (animateNumbers && label != null)
        {
            float delay = index * entryStartDelay;
            AnimateNumber(label, amount, delay);
        }
        else
        {
            if (continueButton != null)
                continueButton.interactable = true;
        }
    }

    private void PlayEntryAppear(RectTransform rect, int index)
    {
        rect.localScale = Vector3.one * entryStartScale;

        Tween tween = rect
            .DOScale(1f, entryAppearDuration)
            .SetDelay(index * entryStartDelay)
            .SetEase(Ease.OutBack)
            .SetUpdate(useUnscaledTime);

        activeTweens.Add(tween);
    }

    private void AnimateNumber(TextMeshProUGUI label, int targetAmount, float delay)
    {
        activeCountAnimations++;

        int currentValue = 0;
        int lastSoundValue = 0;

        float duration = Mathf.Clamp(
            targetAmount * amountDurationMultiplier,
            minCountDuration,
            maxCountDuration
        );

        Tween tween = DOTween
            .To(
                () => currentValue,
                value =>
                {
                    currentValue = value;
                    label.text = currentValue.ToString();

                    if (Mathf.Abs(currentValue - lastSoundValue) >= Mathf.Max(1, tickEveryAmount))
                    {
                        TryPlayTickSound();
                        lastSoundValue = currentValue;
                    }
                },
                targetAmount,
                duration
            )
            .SetDelay(delay)
            .SetEase(Ease.OutCubic)
            .SetUpdate(useUnscaledTime)
            .OnComplete(() =>
            {
                label.text = targetAmount.ToString();

                RectTransform labelRect = label.GetComponent<RectTransform>();

                if (labelRect != null && entryPunchScale > 0f)
                {
                    Tween punchTween = labelRect
                        .DOPunchScale(Vector3.one * entryPunchScale, 0.18f, 6, 0.7f)
                        .SetUpdate(useUnscaledTime);

                    activeTweens.Add(punchTween);
                }

                activeCountAnimations--;

                if (activeCountAnimations <= 0)
                    FinishCounting();
            });

        activeTweens.Add(tween);
    }

    private void FinishCounting()
    {
        activeCountAnimations = 0;

        PlayFinishSound();

        if (continueButton != null)
            continueButton.interactable = true;
    }

    private void TryPlayTickSound()
    {
        if (audioSource == null || tickClip == null)
            return;

        float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;

        if (currentTime - lastTickTime < minTickInterval)
            return;

        lastTickTime = currentTime;

        audioSource.PlayOneShot(tickClip, tickVolume);
    }

    private void PlayFinishSound()
    {
        if (audioSource == null || finishClip == null)
            return;

        audioSource.PlayOneShot(finishClip, finishVolume);
    }

    private void ClearEntries()
    {
        foreach (GameObject go in spawnedEntries)
        {
            if (go != null)
                Destroy(go);
        }

        spawnedEntries.Clear();
    }

    private void KillTweens()
    {
        foreach (Tween tween in activeTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }

        activeTweens.Clear();
        activeCountAnimations = 0;
    }

    private void OnContinueClicked()
    {
        Hide();

        if (G.transition != null)
        {
            G.transition.Play(() =>
            {
                if (G.ui != null)
                    G.ui.ShowSkillTreePanel();
            });
        }
        else
        {
            if (G.ui != null)
                G.ui.ShowSkillTreePanel();
        }
    }
}