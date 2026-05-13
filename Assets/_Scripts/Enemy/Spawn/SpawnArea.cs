using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private Vector2 size = new(10, 6);

    public Vector2 Min => (Vector2)transform.position - size * 0.5f;
    public Vector2 Max => (Vector2)transform.position + size * 0.5f;

    public Vector2 Center => transform.position;

    public Vector2 GetRandomPointInside()
    {
        return new Vector2(
            Random.Range(Min.x, Max.x),
            Random.Range(Min.y, Max.y)
        );
    }

    public Vector2 GetRandomPointOutside()
    {
        float side = Random.Range(0, 4);

        return side switch
        {
            < 1 => new Vector2(Min.x - 1, Random.Range(Min.y, Max.y)),
            < 2 => new Vector2(Max.x + 1, Random.Range(Min.y, Max.y)),
            < 3 => new Vector2(Random.Range(Min.x, Max.x), Max.y + 1),
            _ => new Vector2(Random.Range(Min.x, Max.x), Min.y - 1),
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, size);
    }
}