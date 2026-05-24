using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [SerializeField] private WaveConfig wave;
    [SerializeField] private SpawnArea area;

    [Header("Debug")]
    [SerializeField] private bool logSpawnDebug;

    [Header("Safety")]
    [Tooltip("Если true, WaveDirector иногда пересчитывает живых врагов на сцене. Это спасает, если враг был уничтожен не через EnemyHealth.")]
    [SerializeField] private bool autoRecountAlive = true;

    [SerializeField] private float recountInterval = 2f;

    private float spawnTimer;
    private float recountTimer;

    private readonly Dictionary<GameObject, int> alive = new();

    public event Action OnWaveComplete;

    private void Awake()
    {
        G.waveDirector = this;
    }

    private void Start()
    {
        InitWave();
    }

    private void OnEnable()
    {
        BossProgress.OnBossReady += SpawnBoss;
        BossProgress.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        BossProgress.OnBossReady -= SpawnBoss;
        BossProgress.OnBossDefeated -= HandleBossDefeated;
    }

    public void LoadWave(WaveConfig newWave)
    {
        wave = newWave;
        alive.Clear();
        enabled = true;

        if (wave != null)
            BossProgress.Reset(wave.xpToSpawnBoss);

        InitWave();
    }

    private void InitWave()
    {
        if (wave == null)
            return;

        alive.Clear();

        if (wave.enemies != null)
        {
            foreach (EnemyWaveEntry entry in wave.enemies)
            {
                if (entry != null && entry.prefab != null)
                    alive[entry.prefab] = 0;
            }
        }

        spawnTimer = GetNextTime();
        recountTimer = recountInterval;
    }

    private void Update()
    {
        if (wave == null)
            return;

        if (G.IsPaused || G.IsPlayerDead)
            return;

        if (autoRecountAlive)
            UpdateAliveRecount();

        if (BossProgress.BossSpawned && !wave.keepSpawningDuringBoss)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        TrySpawn();

        spawnTimer = GetNextTime();
    }

    private void UpdateAliveRecount()
    {
        recountTimer -= Time.deltaTime;

        if (recountTimer > 0f)
            return;

        recountTimer = Mathf.Max(0.25f, recountInterval);

        RecountAliveEnemies();
    }

    private void TrySpawn()
    {
        if (wave == null || wave.enemies == null || wave.enemies.Length == 0)
            return;

        int totalAlive = GetTotalAlive();
        int maxTotalAlive = GetCurrentMaxTotalAlive();

        if (totalAlive >= maxTotalAlive)
        {
            if (logSpawnDebug)
            {
                Debug.Log(
                    $"WaveDirector: spawn skipped. Total alive limit reached: {totalAlive}/{maxTotalAlive}"
                );
            }

            return;
        }

        EnemyWaveEntry entry = GetWeightedAvailableEnemy();

        if (entry == null || entry.prefab == null)
        {
            if (logSpawnDebug)
                Debug.Log("WaveDirector: spawn skipped. No available enemy entry.");

            return;
        }

        Spawn(entry);
    }

    private EnemyWaveEntry GetWeightedAvailableEnemy()
    {
        if (wave == null || wave.enemies == null || wave.enemies.Length == 0)
            return null;

        float totalWeight = 0f;

        foreach (EnemyWaveEntry entry in wave.enemies)
        {
            if (!CanSpawnEntry(entry))
                continue;

            totalWeight += Mathf.Max(0f, entry.spawnWeight);
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = UnityEngine.Random.value * totalWeight;
        float currentWeight = 0f;

        foreach (EnemyWaveEntry entry in wave.enemies)
        {
            if (!CanSpawnEntry(entry))
                continue;

            currentWeight += Mathf.Max(0f, entry.spawnWeight);

            if (randomValue <= currentWeight)
                return entry;
        }

        return null;
    }

    private bool CanSpawnEntry(EnemyWaveEntry entry)
    {
        if (entry == null)
            return false;

        if (entry.prefab == null)
            return false;

        if (entry.spawnWeight <= 0f)
            return false;

        if (!alive.ContainsKey(entry.prefab))
            alive[entry.prefab] = 0;

        int currentAlive = alive[entry.prefab];
        int maxAliveForEntry = Mathf.Max(0, entry.maxAlive);

        if (currentAlive >= maxAliveForEntry)
            return false;

        return true;
    }

    private void Spawn(EnemyWaveEntry entry)
    {
        Vector2 pos = GetSpawnPosition(entry.spawnType);

        GameObject enemy = Instantiate(
            entry.prefab,
            pos,
            Quaternion.identity
        );

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();

        if (health != null)
        {
            health.SetXP(entry.xp);
            health.SetBoss(false);
            health.SetWavePrefab(entry.prefab);
        }

        if (!alive.ContainsKey(entry.prefab))
            alive[entry.prefab] = 0;

        alive[entry.prefab]++;

        if (logSpawnDebug)
        {
            Debug.Log(
                $"WaveDirector: spawned {entry.prefab.name}. Alive: {alive[entry.prefab]}/{entry.maxAlive}. Total: {GetTotalAlive()}/{GetCurrentMaxTotalAlive()}"
            );
        }
    }

    private Vector2 GetSpawnPosition(SpawnType spawnType)
    {
        if (area != null)
        {
            return spawnType == SpawnType.InArea
                ? area.GetRandomPointInside()
                : area.GetRandomPointOutside();
        }

        return spawnType == SpawnType.InArea
            ? GetRandomPointInsideCamera()
            : GetRandomPointOutsideCamera();
    }

    private void SpawnBoss()
    {
        if (wave == null || wave.bossPrefab == null)
            return;

        Vector2 pos = area != null
            ? area.GetRandomPointOutside()
            : GetRandomPointOutsideCamera();

        GameObject boss = Instantiate(
            wave.bossPrefab,
            pos,
            Quaternion.identity
        );

        EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();

        if (bossHealth != null)
        {
            bossHealth.SetBoss(true);
            bossHealth.SetWavePrefab(wave.bossPrefab);
        }

        spawnTimer = GetNextTime();

        if (logSpawnDebug)
            Debug.Log("WaveDirector: boss spawned.");
    }

    private void HandleBossDefeated()
    {
        enabled = false;
        OnWaveComplete?.Invoke();
    }

    private float GetNextTime()
    {
        if (wave == null)
            return 1f;

        Vector2 interval = GetCurrentSpawnInterval();

        float min = Mathf.Min(interval.x, interval.y);
        float max = Mathf.Max(interval.x, interval.y);

        min = Mathf.Max(0.05f, min);
        max = Mathf.Max(min, max);

        return UnityEngine.Random.Range(min, max);
    }

    private Vector2 GetCurrentSpawnInterval()
    {
        if (wave == null)
            return new Vector2(1f, 1f);

        if (BossProgress.BossSpawned)
            return wave.bossFightSpawnInterval;

        if (!wave.speedUpSpawnNearBoss)
            return wave.spawnInterval;

        float progress = GetBossProgress01();

        float min = Mathf.Lerp(
            wave.spawnInterval.x,
            wave.nearBossSpawnInterval.x,
            progress
        );

        float max = Mathf.Lerp(
            wave.spawnInterval.y,
            wave.nearBossSpawnInterval.y,
            progress
        );

        return new Vector2(min, max);
    }

    private float GetBossProgress01()
    {
        if (BossProgress.RequiredXP <= 0)
            return 0f;

        return Mathf.Clamp01(
            (float)BossProgress.CurrentXP / BossProgress.RequiredXP
        );
    }

    private int GetCurrentMaxTotalAlive()
    {
        if (wave == null)
            return 0;

        if (BossProgress.BossSpawned)
            return Mathf.Max(0, wave.maxTotalAliveDuringBoss);

        return Mathf.Max(0, wave.maxTotalAliveBeforeBoss);
    }

    private int GetTotalAlive()
    {
        int total = 0;

        foreach (int count in alive.Values)
            total += Mathf.Max(0, count);

        return total;
    }

    public void NotifyDeath(GameObject prefab)
    {
        if (prefab == null)
            return;

        if (!alive.ContainsKey(prefab))
            alive[prefab] = 0;

        alive[prefab] = Mathf.Max(0, alive[prefab] - 1);

        if (logSpawnDebug)
        {
            Debug.Log(
                $"WaveDirector: removed {prefab.name}. Alive now: {alive[prefab]}. Total: {GetTotalAlive()}/{GetCurrentMaxTotalAlive()}"
            );
        }
    }

    private void RecountAliveEnemies()
    {
        if (wave == null || wave.enemies == null)
            return;

        foreach (EnemyWaveEntry entry in wave.enemies)
        {
            if (entry != null && entry.prefab != null)
                alive[entry.prefab] = 0;
        }

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (enemy.IsDead)
                continue;

            if (enemy.IsBoss)
                continue;

            GameObject prefab = GetMatchingWavePrefab(enemy.gameObject);

            if (prefab == null)
                continue;

            if (!alive.ContainsKey(prefab))
                alive[prefab] = 0;

            alive[prefab]++;
        }

        if (logSpawnDebug)
        {
            Debug.Log(
                $"WaveDirector: recount complete. Total alive: {GetTotalAlive()}/{GetCurrentMaxTotalAlive()}"
            );
        }
    }

    private GameObject GetMatchingWavePrefab(GameObject enemyObject)
    {
        if (enemyObject == null || wave == null || wave.enemies == null)
            return null;

        string enemyName = enemyObject.name.Replace("(Clone)", "").Trim();

        foreach (EnemyWaveEntry entry in wave.enemies)
        {
            if (entry == null || entry.prefab == null)
                continue;

            if (entry.prefab.name == enemyName)
                return entry.prefab;
        }

        return null;
    }

    private Vector2 GetRandomPointInsideCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return Vector2.zero;

        float verticalSize = cam.orthographicSize;
        float horizontalSize = verticalSize * cam.aspect;

        Vector2 center = cam.transform.position;

        return new Vector2(
            UnityEngine.Random.Range(center.x - horizontalSize, center.x + horizontalSize),
            UnityEngine.Random.Range(center.y - verticalSize, center.y + verticalSize)
        );
    }

    private Vector2 GetRandomPointOutsideCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
            return Vector2.zero;

        float verticalSize = cam.orthographicSize;
        float horizontalSize = verticalSize * cam.aspect;
        Vector2 center = cam.transform.position;

        const float offset = 1.5f;
        int side = UnityEngine.Random.Range(0, 4);

        return side switch
        {
            0 => new Vector2(
                center.x - horizontalSize - offset,
                UnityEngine.Random.Range(center.y - verticalSize, center.y + verticalSize)
            ),

            1 => new Vector2(
                center.x + horizontalSize + offset,
                UnityEngine.Random.Range(center.y - verticalSize, center.y + verticalSize)
            ),

            2 => new Vector2(
                UnityEngine.Random.Range(center.x - horizontalSize, center.x + horizontalSize),
                center.y + verticalSize + offset
            ),

            _ => new Vector2(
                UnityEngine.Random.Range(center.x - horizontalSize, center.x + horizontalSize),
                center.y - verticalSize - offset
            )
        };
    }
}