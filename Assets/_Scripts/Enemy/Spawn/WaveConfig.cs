using UnityEngine;

[CreateAssetMenu(menuName = "Waves/Wave Config")]
public class WaveConfig : ScriptableObject
{
    public float duration = 20f;

    public Vector2 spawnInterval = new(0.5f, 2f);

    public EnemyWaveEntry[] enemies;
}