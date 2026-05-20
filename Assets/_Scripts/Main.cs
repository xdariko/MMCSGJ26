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

        bool wasLastLevel = levelDatabase != null
            && LevelProgress.SelectedLevel >= levelDatabase.levels.Length - 1;

        EndCurrentRunCleanup();

        if (wasLastLevel)
        {
            if (G.ui != null)
                G.ui.SetFinalPanel(true);

            return;
        }

        LevelProgress.SelectedLevel = LevelProgress.UnlockedLevel;

        if (G.ui != null)
            G.ui.ShowVictoryPanel();
        else
            ShowSkillTree();
    }

    private void Update()
    {
        HandlePauseInput();

        if (G.IsPlayerDead)
            return;

        ProcessPassiveCurrencyRewards();
    }

    private void HandlePauseInput()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            TogglePause();
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
        if (G.IsPlayerDead)
            return;

        EndCurrentRunCleanup();

        if (G.ui != null)
            G.ui.ShowDeathPanel();
    }

    private void EndCurrentRunCleanup()
    {
        G.IsPlayerDead = true;
        G.IsPaused = false;
        Time.timeScale = 1f;

        if (G.waveDirector != null)
            G.waveDirector.OnWaveComplete -= OnLevelComplete;

        StopSpawners();

        ClearEnemies();
        ClearObjectsWithTag("Projectile");
        ClearObjectsWithTag("Orb");

        DestroyPlayer();
    }

    private void ShowSkillTree()
    {
        if (G.ui != null)
            G.ui.ShowSkillTreePanel();
    }

    public void StartNewRun()
    {
        // New run after skill tree / death panel / victory panel.
        // Keeps long-term progress and skill tree purchases,
        // but cleans the scene and resets per-run state.
        if (G.ui != null)
        {
            G.ui.HideSkillTreePanel();
            G.ui.HideRunResultPanel();
        }

        Time.timeScale = 1f;
        G.ResetRuntimeFlags();
        passiveCurrencyTimer = 0f;

        ClearEnemies();
        ClearObjectsWithTag("Projectile");
        ClearObjectsWithTag("Orb");
        DestroyPlayer();

        PlayerStats.ResetBaseStats();
        CurrencyManager.ResetRunCollected();

        EnsurePlayerSpawnedAtScreenCenter();
        LoadSelectedLevel();
    }

    // This is the REAL New Game button logic.
    // It wipes all progress: currencies, level progress, skill tree PlayerPrefs,
    // runtime bonuses and base stat cache, then reloads the scene cleanly.
    public void RestartCurrentRun()
    {
        ResetGameToInitialState();
    }

    public void ResetGameToInitialState()
    {
        // REAL New Game: wipe absolutely everything and reload.
        Time.timeScale = 1f;
        G.ResetRuntimeFlags();

        if (G.ui != null)
        {
            G.ui.HideSkillTreePanel();
            G.ui.HideRunResultPanel();
        }

        StopSpawners();
        ClearEnemies();
        ClearObjectsWithTag("Projectile");
        ClearObjectsWithTag("Orb");
        DestroyPlayer();

        passiveCurrencyTimer = 0f;
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
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
            Destroy(enemy);
    }

    private void ClearObjectsWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objects)
            Destroy(obj);
    }
}
