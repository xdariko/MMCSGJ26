using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    private bool movementLocked;

    public bool CanMove => !movementLocked;

    public void LockMovement()
    {
        movementLocked = true;
    }

    public void UnlockMovement()
    {
        movementLocked = false;
    }
}