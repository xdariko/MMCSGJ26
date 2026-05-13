using System.Collections.Generic;
using UnityEngine;

public class WaveDirector : MonoBehaviour
{
    [SerializeField] private WaveConfig wave;
    [SerializeField] private SpawnArea area;

    private float spawnTimer;

    private Dictionary<GameObject, int> alive = new();

    private void Start()
    {
        foreach (var e in wave.enemies)
            alive[e.prefab] = 0;

        spawnTimer = GetNextTime();
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawn();
            spawnTimer = GetNextTime();
        }
    }

    private float GetNextTime()
    {
        return Random.Range(
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

        float r = Random.value * total;

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

        alive[e.prefab]++;
    }

    public void NotifyDeath(GameObject prefab)
    {
        if (!alive.ContainsKey(prefab))
            return;

        alive[prefab]--;
    }
}