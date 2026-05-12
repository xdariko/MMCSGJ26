using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpellBeam : MonoBehaviour
{
    private LineRenderer line;

    private Transform startTarget;
    private Transform endTarget;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void Initialize(
        Transform start,
        Transform end)
    {
        startTarget = start;
        endTarget = end;
    }

    private void Update()
    {
        if (startTarget == null || endTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        line.SetPosition(0, startTarget.position);
        line.SetPosition(1, endTarget.position);
    }
}