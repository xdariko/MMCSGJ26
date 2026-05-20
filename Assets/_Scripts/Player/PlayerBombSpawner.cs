using UnityEngine;

public class PlayerBombSpawner : MonoBehaviour
{
    [Header("Bombs")]
    [SerializeField] private BombPickup bombPrefab;

    [Header("Spawn")]
    [SerializeField] private float spawnRadius = 2f;

    private float timer;

    private void Update()
    {
        if (G.IsPaused || G.IsPlayerDead)
            return;

        if (!PlayerStats.BombsUnlocked)
            return;

        if (bombPrefab == null)
            return;

        timer += Time.deltaTime;

        float interval = Mathf.Max(
            0.25f,
            PlayerStats.BombSpawnInterval);

        if (timer < interval)
            return;

        timer -= interval;

        SpawnBomb();
    }

    private void SpawnBomb()
    {
        Vector2 randomOffset =
            Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPosition =
            transform.position +
            new Vector3(randomOffset.x, randomOffset.y, 0f);

        BombPickup bomb = Instantiate(
            bombPrefab,
            spawnPosition,
            Quaternion.identity);

        bomb.Setup(
            PlayerStats.BombExplosionRadius,
            PlayerStats.BombDamage);
    }

    public void ResetTimer()
    {
        timer = 0f;
    }
}