using UnityEngine;

public class EnemyOrbDrop : MonoBehaviour
{
    [SerializeField] private GameObject stableOrb;
    [SerializeField] private GameObject unstableOrb;
    [SerializeField] private GameObject currencyOrb;

    public void Drop(Vector2 position)
    {
        float roll = Random.value;

        GameObject prefab;

        if (roll < 0.4f)
            prefab = stableOrb;
        else if (roll < 0.8f)
            prefab = unstableOrb;
        else
            prefab = currencyOrb;

        Instantiate(prefab, position, Quaternion.identity);
    }
}