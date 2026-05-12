using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 pos =
                Random.insideUnitCircle * 6f;

            Instantiate(
                enemyPrefab,
                pos,
                Quaternion.identity);
        }
    }
}