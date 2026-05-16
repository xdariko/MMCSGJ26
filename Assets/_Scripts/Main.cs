using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject damagePopupPrefab;

    [Header("Levels")]
    [SerializeField] private LevelDatabase levelDatabase;

    private float impTimer;

    private void Awake()
    {
        G.main = this;
        G.IsPlayerDead = false;
        G.damagePopupPrefab = damagePopupPrefab;
        G.levelDatabase = levelDatabase;
    }

    private void Start()
    {
        G.player = GameObject.FindGameObjectWithTag("Player");
        LoadSelectedLevel();
    }

    private void LoadSelectedLevel()
    {
        if (levelDatabase == null || G.waveDirector == null) return;

        int idx = LevelProgress.SelectedLevel;
        if (idx < 0 || idx >= levelDatabase.levels.Length) return;

        G.waveDirector.OnWaveComplete += OnLevelComplete;
        G.waveDirector.LoadWave(levelDatabase.levels[idx].wave);
    }

    private void OnLevelComplete()
    {
        if (G.waveDirector != null)
            G.waveDirector.OnWaveComplete -= OnLevelComplete;

        LevelProgress.CompleteCurrentLevel();
        ClearEnemies();

        bool wasLastLevel = levelDatabase != null
            && LevelProgress.SelectedLevel >= levelDatabase.levels.Length - 1;

        if (wasLastLevel)
        {
            if (G.ui != null)
                G.ui.SetFinalPanel(true);
        }
        else
        {
            LevelProgress.SelectedLevel = LevelProgress.UnlockedLevel;
            ShowSkillTree();
        }
    }

    private void Update()
    {
        if (G.IsPlayerDead) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        SetPause(!G.IsPaused);
    }

    private void SetPause(bool paused)
    {
        G.IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (G.ui != null)
            G.ui.SetPausePanel(paused);
    }

    public void ResumeGame()
    {
        SetPause(false);
    }

    public void OnPlayerDeath()
    {
        if (G.IsPlayerDead) return;
        G.IsPlayerDead = true;

        if (G.waveDirector != null)
            G.waveDirector.OnWaveComplete -= OnLevelComplete;

        StopSpawners();
        ClearEnemies();

        // Example: transitionAnimator.SetTrigger("DeathToSkillTree");
        // After animation completes, call ShowSkillTree(). For now we call it directly.

        ShowSkillTree();
    }

    private void ShowSkillTree()
    {
        if (G.ui != null)
            G.ui.ShowSkillTreePanel();
    }

    public void StartNewRun()
    {
        if (G.ui != null)
            G.ui.HideSkillTreePanel();

        // Example: transitionAnimator.SetTrigger("SkillTreeToGame");
        // After animation completes, reload scene. For now we reload directly.

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void StopSpawners()
    {
        if (G.waveDirector != null)
            G.waveDirector.enabled = false;
    }

    private void ClearEnemies()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
            Destroy(e);
    }
}