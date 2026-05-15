using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class PlayerPickupCollector : MonoBehaviour
{
    private CircleCollider2D pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        PlayerStats.BasePickupRadius = pickupCollider.radius;
    }

    private void Update()
    {
        pickupCollider.radius = PlayerStats.PickupRadius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OrbPickup orb =
            other.GetComponent<OrbPickup>();

        if (orb == null)
            return;

        orb.StartFollowing(transform.root);
    }
}