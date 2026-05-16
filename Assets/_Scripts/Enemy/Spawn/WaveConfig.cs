using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Wave Config")]
public class WaveConfig : ScriptableObject
{
    public Vector2 spawnInterval = new(0.5f, 2f);

    public EnemyWaveEntry[] enemies;

    [Header("Boss")]
    public GameObject bossPrefab;
    public int xpToSpawnBoss = 20;
}