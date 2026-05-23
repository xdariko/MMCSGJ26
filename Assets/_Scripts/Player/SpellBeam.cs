using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SpellBeam : MonoBehaviour
{
    private LineRenderer line;

    private Transform startTarget;
    private Transform endTarget;

    private bool initialized;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        if (line != null)
        {
            line.positionCount = 2;
            line.enabled = false;

            Vector3 safePosition = transform.position;
            line.SetPosition(0, safePosition);
            line.SetPosition(1, safePosition);
        }
    }

    public void Initialize(Transform start, Transform end)
    {
        startTarget = start;
        endTarget = end;

        if (startTarget == null || endTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        initialized = true;

        UpdateBeamPosition();

        if (line != null)
            line.enabled = true;
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        if (startTarget == null || endTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdateBeamPosition();
    }

    private void UpdateBeamPosition()
    {
        if (line == null || startTarget == null || endTarget == null)
            return;

        line.SetPosition(0, startTarget.position);
        line.SetPosition(1, endTarget.position);
    }
}