using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class UI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private GameObject skillTreePanel;
    [SerializeField] private DeathPanelUI deathPanel;

    [Header("Comic Cutscenes")]
    [SerializeField] private ComicCutsceneUI introCutscene;
    [SerializeField] private ComicCutsceneUI finalCutscene;
    [SerializeField] private bool playIntroCutsceneOnSceneStart = true;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button continueButton;

    [FormerlySerializedAs("fullRestartButton")]
    [SerializeField] private Button newGameButton;

    [SerializeField] private Button exitButton;

    [Header("Final Panel Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button finalExitButton;

    private bool introCutsceneWasPlayed;

    private void Awake()
    {
        G.ui = this;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (finalPanel != null)
            finalPanel.SetActive(false);

        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);
    }

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);

        if (finalExitButton != null)
            finalExitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);

        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(OnNewGameClicked);

        if (exitButton != null)
            exitButton.onClick.RemoveListener(OnExitClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartClicked);

        if (finalExitButton != null)
            finalExitButton.onClick.RemoveListener(OnExitClicked);
    }

    public bool ShouldPlayIntroCutscene()
    {
        return playIntroCutsceneOnSceneStart
            && !introCutsceneWasPlayed
            && introCutscene != null;
    }

    public void PlayIntroCutscene(Action onComplete)
    {
        if (introCutscene == null)
        {
            onComplete?.Invoke();
            return;
        }

        introCutsceneWasPlayed = true;

        introCutscene.Play(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void PlayFinalCutscene()
    {
        HideSkillTreePanel();
        HideRunResultPanel();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (finalPanel != null)
            finalPanel.SetActive(false);

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMenuMusic();

        if (finalCutscene != null)
        {
            finalCutscene.Play();
            return;
        }

        SetFinalPanel(true);
    }

    private void OnContinueClicked()
    {
        if (G.main != null)
            G.main.ResumeGame();
    }

    private void OnRestartClicked()
    {
        OnNewGameClicked();
    }

    private void OnNewGameClicked()
    {
        if (G.main != null)
            G.main.ResetGameToInitialState();
        else
            GameResetUtility.ResetAllProgressAndReload();
    }

    private void OnExitClicked()
    {
        Application.Quit();
    }

    internal void SetPausePanel(bool active)
    {
        if (pausePanel == null)
            return;

        pausePanel.SetActive(active);

        if (active)
            pausePanel.transform.SetAsLastSibling();
    }

    public void SetFinalPanel(bool active)
    {
        if (finalPanel != null)
        {
            finalPanel.SetActive(active);

            if (active)
                finalPanel.transform.SetAsLastSibling();
        }

        if (active)
        {
            G.IsPaused = true;
            Time.timeScale = 0f;

            if (MusicManager.Instance != null)
                MusicManager.Instance.PlayMenuMusic();
        }
    }

    public bool IsFinalPanelOpen()
    {
        return finalPanel != null && finalPanel.activeInHierarchy;
    }

    public void ShowSkillTreePanel()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(true);

        G.IsMenuOpen = true;

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayMenuMusic();
    }

    public void HideSkillTreePanel()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);

        G.IsMenuOpen = false;
    }

    public bool IsSkillTreePanelOpen()
    {
        return skillTreePanel != null && skillTreePanel.activeInHierarchy;
    }

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.ShowDeath();
    }

    public void ShowVictoryPanel()
    {
        if (deathPanel != null)
            deathPanel.ShowVictory();
    }

    public void HideRunResultPanel()
    {
        if (deathPanel != null)
            deathPanel.Hide();
    }

    public bool IsDeathPanelOpen()
    {
        return deathPanel != null && deathPanel.IsDeathOpen();
    }

    public bool IsRunResultPanelOpen()
    {
        return deathPanel != null && deathPanel.IsOpen();
    }
}