using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [System.Serializable]
    private class Drop
    {
        [Tooltip("Orb prefab to spawn. Should have OrbPickup with matching currency type.")]
        public GameObject prefab;
        public int amount = 1;
        [Tooltip("Skip this drop if currency type is locked. Use None to always drop.")]
        public CurrencyType requiresUnlockedCurrency = CurrencyType.None;
    }

    [SerializeField] private Drop[] drops;

    public void DropLoot()
    {
        if (drops == null) return;

        foreach (var drop in drops)
        {
            if (drop.prefab == null) continue;

            if (drop.requiresUnlockedCurrency != CurrencyType.None
                && !CurrencyManager.IsUnlocked(drop.requiresUnlockedCurrency))
                continue;

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
}