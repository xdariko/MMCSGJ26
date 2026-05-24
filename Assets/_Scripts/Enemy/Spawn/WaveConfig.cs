using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Spawn Interval Before Boss")]
    [Tooltip("Интервал спавна в начале уровня.")]
    public Vector2 spawnInterval = new(0.9f, 1.6f);

    [Tooltip("Если включено, чем ближе прогресс к боссу, тем чаще будет спавн.")]
    public bool speedUpSpawnNearBoss = true;

    [Tooltip("Интервал спавна, когда игрок почти дошёл до босса.")]
    public Vector2 nearBossSpawnInterval = new(0.45f, 0.9f);

    [Header("Spawn During Boss")]
    [Tooltip("Если включено, обычные враги продолжают появляться во время босса.")]
    public bool keepSpawningDuringBoss = true;

    [Tooltip("Интервал спавна обычных врагов во время боя с боссом.")]
    public Vector2 bossFightSpawnInterval = new(0.9f, 1.4f);

    [Header("Alive Limits")]
    [Tooltip("Общий максимум обычных врагов до появления босса.")]
    public int maxTotalAliveBeforeBoss = 14;

    [Tooltip("Общий максимум обычных врагов во время босса.")]
    public int maxTotalAliveDuringBoss = 8;

    [Header("Enemies")]
    public EnemyWaveEntry[] enemies;

    [Header("Boss")]
    public GameObject bossPrefab;
    public int xpToSpawnBoss = 20;
}