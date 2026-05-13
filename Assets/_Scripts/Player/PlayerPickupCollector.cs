using UnityEngine;

public class PlayerPickupCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        OrbPickup orb =
            other.GetComponent<OrbPickup>();

        if (orb == null)
            return;

        orb.StartFollowing(transform.root);
    }
}