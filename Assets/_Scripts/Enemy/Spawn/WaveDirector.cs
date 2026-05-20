using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [SerializeField] private WaveConfig wave;
    [SerializeField] private SpawnArea area;

    private float spawnTimer;
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
            foreach (EnemyWaveEntry e in wave.enemies)
            {
                if (e != null && e.prefab != null)
                    alive[e.prefab] = 0;
            }
        }

        spawnTimer = GetNextTime();
    }

    private void Update()
    {
        if (wave == null)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawn();
            spawnTimer = GetNextTime();
        }
    }

    private void SpawnBoss()
    {
        if (wave == null || wave.bossPrefab == null)
            return;

        Vector2 pos = area != null
            ? area.GetRandomPointOutside()
            : GetRandomPointOutsideCamera();

        GameObject boss = Instantiate(wave.bossPrefab, pos, Quaternion.identity);

        EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();
        if (bossHealth != null)
        {
            bossHealth.SetBoss(true);
            bossHealth.SetWavePrefab(wave.bossPrefab);
        }
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
            0 => new Vector2(center.x - horizontalSize - offset, UnityEngine.Random.Range(center.y - verticalSize, center.y + verticalSize)),
            1 => new Vector2(center.x + horizontalSize + offset, UnityEngine.Random.Range(center.y - verticalSize, center.y + verticalSize)),
            2 => new Vector2(UnityEngine.Random.Range(center.x - horizontalSize, center.x + horizontalSize), center.y + verticalSize + offset),
            _ => new Vector2(UnityEngine.Random.Range(center.x - horizontalSize, center.x + horizontalSize), center.y - verticalSize - offset),
        };
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

        return UnityEngine.Random.Range(
            wave.spawnInterval.x,
            wave.spawnInterval.y
        );
    }

    private void TrySpawn()
    {
        EnemyWaveEntry entry = GetWeightedEnemy();

        if (entry == null || entry.prefab == null)
            return;

        if (!alive.ContainsKey(entry.prefab))
            alive[entry.prefab] = 0;

        if (alive[entry.prefab] >= entry.maxAlive)
            return;

        Spawn(entry);
    }

    private EnemyWaveEntry GetWeightedEnemy()
    {
        if (wave == null || wave.enemies == null || wave.enemies.Length == 0)
            return null;

        float total = 0f;

        foreach (EnemyWaveEntry e in wave.enemies)
        {
            if (e != null && e.prefab != null)
                total += e.spawnWeight;
        }

        if (total <= 0f)
            return null;

        float r = UnityEngine.Random.value * total;
        float sum = 0f;

        foreach (EnemyWaveEntry e in wave.enemies)
        {
            if (e == null || e.prefab == null)
                continue;

            sum += e.spawnWeight;
            if (r <= sum)
                return e;
        }

        return wave.enemies[0];
    }

    private void Spawn(EnemyWaveEntry e)
    {
        Vector2 pos =
            e.spawnType == SpawnType.InArea
                ? area.GetRandomPointInside()
                : area.GetRandomPointOutside();

        GameObject enemy = Instantiate(e.prefab, pos, Quaternion.identity);

        EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
        if (hp != null)
        {
            hp.SetXP(e.xp);
            hp.SetBoss(false);
            hp.SetWavePrefab(e.prefab);
        }

        alive[e.prefab]++;
    }

    public void NotifyDeath(GameObject prefab)
    {
        if (prefab == null)
            return;

        if (!alive.ContainsKey(prefab))
            return;

        alive[prefab] = Mathf.Max(0, alive[prefab] - 1);
    }
}
