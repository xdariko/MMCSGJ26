using System.Collections;
using UnityEngine;

public class PlayerContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float contactDamage = 10f;

    [Header("Shield (Invincibility Frames)")]
    [SerializeField] private float shieldDuration = 1.5f;
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private PlayerStabilitySystem stability;
    private bool isShielded;

    private void Awake()
    {
        stability = GetComponent<PlayerStabilitySystem>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isShielded) return;
        if (other.CompareTag("Enemy") == false) return;

        stability.RemoveStability(contactDamage);
        StartCoroutine(ShieldRoutine());
    }

    private IEnumerator ShieldRoutine()
    {
        isShielded = true;

        float elapsed = 0f;
        bool visible = true;

        float duration = PlayerStats.GetInvincibilityDuration(shieldDuration);

        while (elapsed < duration)
        {
            visible = !visible;
            if (spriteRenderer != null)
                spriteRenderer.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isShielded = false;
    }
}
