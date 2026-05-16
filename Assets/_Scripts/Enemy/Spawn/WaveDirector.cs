using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [SerializeField] private WaveConfig wave;
    [SerializeField] private SpawnArea area;

    private float spawnTimer;

    private Dictionary<GameObject, int> alive = new();

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

        BossProgress.Reset(newWave.xpToSpawnBoss);
        InitWave();
    }

    private void InitWave()
    {
        if (wave == null) return;

        foreach (var e in wave.enemies)
            alive[e.prefab] = 0;

        spawnTimer = GetNextTime();
    }

    private void Update()
    {
        if (wave == null) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawn();
            spawnTimer = GetNextTime();
        }
    }

    private void SpawnBoss()
    {
        if (wave == null || wave.bossPrefab == null || area == null) return;

        Vector2 pos = area.GetRandomPointOutside();
        GameObject boss = Instantiate(wave.bossPrefab, pos, Quaternion.identity);

        EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();
        if (bossHealth != null)
            bossHealth.SetBoss(true);
    }

    private void HandleBossDefeated()
    {
        enabled = false;
        OnWaveComplete?.Invoke();
    }

    private float GetNextTime()
    {
        return UnityEngine.Random.Range(
            wave.spawnInterval.x,
            wave.spawnInterval.y
        );
    }

    private void TrySpawn()
    {
        EnemyWaveEntry entry = GetWeightedEnemy();

        if (entry == null)
            return;

        if (alive[entry.prefab] >= entry.maxAlive)
            return;

        Spawn(entry);
    }

    private EnemyWaveEntry GetWeightedEnemy()
    {
        float total = 0f;

        foreach (var e in wave.enemies)
            total += e.spawnWeight;

        float r = UnityEngine.Random.value * total;

        float sum = 0f;

        foreach (var e in wave.enemies)
        {
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

        GameObject enemy =
            Instantiate(e.prefab, pos, Quaternion.identity);

        EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
        if (hp != null)
            hp.SetXP(e.xp);

        alive[e.prefab]++;
    }

    public void NotifyDeath(GameObject prefab)
    {
        if (!alive.ContainsKey(prefab))
            return;

        alive[prefab]--;
    }
}