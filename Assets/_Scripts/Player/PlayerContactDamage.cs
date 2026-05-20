using System.Collections;
using System.Collections.Generic;
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

    private readonly HashSet<Collider2D> touchingEnemies = new();

    private void Awake()
    {
        stability = GetComponent<PlayerStabilitySystem>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsEnemyCollider(other))
            return;

        touchingEnemies.Add(other);
        TryTakeContactDamage();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsEnemyCollider(other))
            return;

        touchingEnemies.Remove(other);
    }

    private bool IsEnemyCollider(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            return true;

        return other.GetComponentInParent<EnemyHealth>() != null;
    }

    private void TryTakeContactDamage()
    {
        if (isShielded)
            return;

        CleanupDestroyedEnemies();

        if (touchingEnemies.Count == 0)
            return;

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

        CleanupDestroyedEnemies();
        TryTakeContactDamage();
    }

    private void CleanupDestroyedEnemies()
    {
        touchingEnemies.RemoveWhere(enemy => enemy == null);
    }
}