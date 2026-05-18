
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject damagePopupPrefab;

    [Header("Levels")]
    [SerializeField] private LevelDatabase levelDatabase;

    private float impTimer;
    private float passiveCurrencyTimer;

    private void Awake()
    {
        G.main = this;
        G.ResetRuntimeFlags();
        G.IsPlayerDead = false;
        G.damagePopupPrefab = damagePopupPrefab;
        G.levelDatabase = levelDatabase;
    }

    private void Start()
    {
        EnsurePlayerSpawnedAtScreenCenter();
        CurrencyManager.ResetRunCollected();
        LoadSelectedLevel();
    }

    private void LoadSelectedLevel()
    {
        if (levelDatabase == null || G.waveDirector == null)
            return;

        int idx = LevelProgress.SelectedLevel;
        if (idx < 0 || idx >= levelDatabase.levels.Length)
            return;

        G.waveDirector.OnWaveComplete -= OnLevelComplete;
        G.waveDirector.OnWaveComplete += OnLevelComplete;
        G.waveDirector.enabled = true;
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

        ProcessPassiveCurrencyRewards();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void ProcessPassiveCurrencyRewards()
    {
        if (G.IsPaused || !PlayerStats.HasPassiveCurrencyRewards())
            return;

        passiveCurrencyTimer += Time.deltaTime;

        float interval = Mathf.Max(0.1f, PlayerStats.PassiveCurrencyIntervalSeconds);
        if (passiveCurrencyTimer < interval)
            return;

        int tickCount = Mathf.FloorToInt(passiveCurrencyTimer / interval);
        passiveCurrencyTimer -= tickCount * interval;

        foreach (var reward in PlayerStats.GetPassiveCurrencyRewards())
        {
            CurrencyManager.Add(reward.Key, reward.Value * tickCount);
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
        DestroyPlayer();

        if (G.ui != null)
            G.ui.ShowDeathPanel();
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

        Time.timeScale = 1f;
        G.ResetRuntimeFlags();
        passiveCurrencyTimer = 0f;

        ClearEnemies();
        CurrencyManager.ResetRunCollected();
        EnsurePlayerSpawnedAtScreenCenter();
        LoadSelectedLevel();
    }

    public void RestartCurrentRun()
    {
        if (G.ui != null)
            G.ui.HideSkillTreePanel();

        Time.timeScale = 1f;
        G.IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ResetGameToInitialState()
    {
        GameResetUtility.ResetAllProgressAndReload();
    }

    private void EnsurePlayerSpawnedAtScreenCenter()
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            G.player = existingPlayer;
            existingPlayer.transform.position = GetScreenCenterWorldPosition();
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("Main: Player Prefab is not assigned.");
            return;
        }

        G.player = Instantiate(
            playerPrefab,
            GetScreenCenterWorldPosition(),
            Quaternion.identity);
    }

    private Vector3 GetScreenCenterWorldPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return Vector3.zero;

        float distanceToWorldPlane = Mathf.Abs(cam.transform.position.z);
        Vector3 screenCenter = new Vector3(
            Screen.width * 0.5f,
            Screen.height * 0.5f,
            distanceToWorldPlane);

        Vector3 worldPosition = cam.ScreenToWorldPoint(screenCenter);
        worldPosition.z = 0f;

        return worldPosition;
    }

    private void DestroyPlayer()
    {
        GameObject player = G.player;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            Destroy(player);

        G.player = null;
        G.stability = null;
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
