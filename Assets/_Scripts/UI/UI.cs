using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private GameObject skillTreePanel;
    [SerializeField] private DeathPanelUI deathPanel;

    [Header("Pause Panel Buttons")]
    [SerializeField] private Button continueButton;

    [FormerlySerializedAs("fullRestartButton")]
    [SerializeField] private Button newGameButton;

    [SerializeField] private Button exitButton;

    [Header("Final Panel Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button finalExitButton;

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

    private void OnContinueClicked()
    {
        if (G.main != null)
            G.main.ResumeGame();
    }

    private void OnRestartClicked()
    {
        // Final panel restart is also a clean New Game.
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
        if (pausePanel != null)
            pausePanel.SetActive(active);
    }

    public void SetFinalPanel(bool active)
    {
        if (finalPanel != null)
            finalPanel.SetActive(active);

        if (active)
            Time.timeScale = 0f;
    }

    public bool IsFinalPanelOpen()
    {
        return finalPanel != null && finalPanel.activeInHierarchy;
    }

    public void ShowSkillTreePanel()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(true);
    }

    public void HideSkillTreePanel()
    {
        if (skillTreePanel != null)
            skillTreePanel.SetActive(false);
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
}
