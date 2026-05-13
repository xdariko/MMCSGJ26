using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [System.Serializable]
    private class Drop
    {
        public GameObject prefab;
        public int amount = 1;
    }

    [SerializeField] private Drop drop;

    public void DropLoot()
    {
        if (drop.prefab == null)
            return;

        for (int i = 0; i < drop.amount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 0.3f;

            Instantiate(
                drop.prefab,
                (Vector2)transform.position + offset,
                Quaternion.identity
            );
        }
    }
}