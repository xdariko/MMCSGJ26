using System;
using UnityEngine;

[Serializable]
public class EnemyWaveEntry
{
    public GameObject prefab;

    [Header("Spawn Rules")]
    public SpawnType spawnType;

    public int maxAlive = 5;

    [Range(0f, 1f)]
    public float spawnWeight = 1f;

    [Header("Reward")]
    public int xp = 1;
}