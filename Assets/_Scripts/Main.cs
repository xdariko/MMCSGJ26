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
    private bool gameStarted;

    private void Awake()
    {
        G.main = this;
        G.ResetRuntimeFlags();
        G.IsPlayerDead = false;
        G.damagePopupPrefab = damagePopupPrefab;
        G.levelDatabase = levelDatabase;

        LevelProgress.Load();
        ClampSavedLevelProgressToDatabase();
    }

    private void Start()
    {
        CurrencyManager.ResetRunCollected();

        TryPlayIntroCutsceneOrStartGame();
    }

    private void TryPlayIntroCutsceneOrStartGame()
    {
        if (G.ui != null && G.ui.ShouldPlayIntroCutscene())
        {
            G.ui.PlayIntroCutscene(() =>
            {
                StartGameAfterIntro();
            });

            return;
        }

        StartGameAfterIntro();
    }

    private void StartGameAfterIntro()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        Time.timeScale = 1f;
        G.IsPaused = false;
        G.IsPlayerDead = false;

        EnsurePlayerSpawnedAtScreenCenter();
        LoadSelectedLevel();

        PlayGameMusicAfterTransition();
    }

    private void PlayGameMusicAfterTransition()
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayNextGameMusicForRun(false);
    }

    private void LoadSelectedLevel()
    {
        if (levelDatabase == null || G.waveDirector == null)
            return;

        ClampSavedLevelProgressToDatabase();

        int idx = LevelProgress.SelectedLevel;

        if (idx < 0 || idx >= levelDatabase.levels.Length)
            return;

        G.waveDirector.OnWaveComplete -= OnLevelComplete;
        G.waveDirector.OnWaveComplete += OnLevelComplete;
        G.waveDirector.enabled = true;
        G.waveDirector.LoadWave(levelDatabase.levels[idx].wave);
    }

    private void ClampSavedLevelProgressToDatabase()
    {
        if (levelDatabase == null || levelDatabase.levels == null)
            return;

        LevelProgress.ClampToLevelCount(levelDatabase.levels.Length);
    }

    private void OnLevelComplete()
    {
        if (G.waveDirector != null)
            G.waveDirector.OnWaveComplete -= OnLevelComplete;

        LevelProgress.CompleteCurrentLevel();

        bool wasLastLevel = levelDatabase != null
            && levelDatabase.levels != null
            && LevelProgress.SelectedLevel >= levelDatabase.levels.Length - 1;

        EndCurrentRunCleanup(false);

        if (wasLastLevel)
        {
            if (G.ui != null)
                G.ui.PlayFinalCutscene();

            return;
        }

        LevelProgress.SelectLevel(LevelProgress.UnlockedLevel);

        if (G.ui != null)
            G.ui.ShowVictoryPanel();
        else
            ShowSkillTreeWithTransition();
    }

    private void ShowSkillTreeWithTransition()
    {
        if (G.transition != null)
        {
            G.transition.Play(() =>
            {
                ShowSkillTree();
            });
        }
        else
        {
            ShowSkillTree();
        }
    }

    private void Update()
    {
        HandlePauseInput();

        if (!gameStarted)
            return;

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
            CurrencyManager.Add(reward.Key, reward.Value * tickCount);
    }

    private void TogglePause()
    {
        if (G.ui != null && G.ui.IsFinalPanelOpen())
            return;

        bool menuOpen =
            G.IsMenuOpen ||
            (G.ui != null && G.ui.IsSkillTreePanelOpen()) ||
            (G.ui != null && G.ui.IsRunResultPanelOpen());

        if (G.IsPlayerDead && !menuOpen)
            return;

        SetPause(!G.IsPaused);
    }

    private void SetPause(bool paused)
    {
        G.IsPaused = paused;

        if (!G.IsMenuOpen)
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

        EndCurrentRunCleanup(true);

        if (G.ui != null)
            G.ui.ShowDeathPanel();
    }

    private void EndCurrentRunCleanup(bool stopMusicImmediately)
    {
        G.IsPlayerDead = true;
        G.IsPaused = false;
        Time.timeScale = 1f;

        if (stopMusicImmediately && MusicManager.Instance != null)
            MusicManager.Instance.StopMusicImmediate();

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
        if (G.transition != null)
        {
            G.IsPaused = true;
            Time.timeScale = 0f;

            G.transition.Play(
                () =>
                {
                    StartNewRunInternal(false);

                    G.IsPaused = true;
                    Time.timeScale = 0f;
                },
                () =>
                {
                    G.IsPaused = false;
                    Time.timeScale = 1f;

                    PlayGameMusicAfterTransition();
                }
            );
        }
        else
        {
            StartNewRunInternal(true);
        }
    }

    private void StartNewRunInternal(bool playMusicNow)
    {
        if (G.ui != null)
        {
            G.ui.HideSkillTreePanel();
            G.ui.HideRunResultPanel();
        }

        Time.timeScale = 1f;
        G.ResetRuntimeFlags();

        gameStarted = true;
        passiveCurrencyTimer = 0f;

        ClearEnemies();
        ClearObjectsWithTag("Projectile");
        ClearObjectsWithTag("Orb");
        DestroyPlayer();

        PlayerStats.ResetBaseStats();
        PlayerStats.ResetCurrencyDropRoundingCarry();
        CurrencyManager.ResetRunCollected();

        EnsurePlayerSpawnedAtScreenCenter();
        LoadSelectedLevel();

        if (playMusicNow)
            PlayGameMusicAfterTransition();
    }

    public void RestartCurrentRun()
    {
        ResetGameToInitialState();
    }

    public void ResetGameToInitialState()
    {
        Time.timeScale = 1f;
        G.ResetRuntimeFlags();

        if (MusicManager.Instance != null)
            MusicManager.Instance.StopMusicImmediate();

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
        gameStarted = false;

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
            Quaternion.identity
        );
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
            distanceToWorldPlane
        );

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