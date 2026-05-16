using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 10f;

    [Header("XP")]
    [SerializeField] private int xpReward;
    [SerializeField] private bool isBoss;

    [Header("Hit FX")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private float hitSoundVolume = 0.8f;

    private float currentHealth;
    private EnemyDrop drop;

    public bool IsBoss => isBoss;

    public void SetXP(int xp) => xpReward = xp;
    public void SetBoss(bool boss) => isBoss = boss;

    public event Action<float> OnDamaged;

    private void Awake()
    {
        currentHealth = maxHealth;
        drop = GetComponent<EnemyDrop>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        ShowDamagePopup(damage);
        PlayHitSound();
        OnDamaged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void ShowDamagePopup(float damage)
    {
        if (G.damagePopupPrefab == null) return;

        Vector3 pos = transform.position + new Vector3(
            UnityEngine.Random.Range(-0.3f, 0.3f),
            0.5f,
            0f
        );

        GameObject go = UnityEngine.Object.Instantiate(G.damagePopupPrefab, pos, Quaternion.identity);
        DamagePopup popup = go.GetComponent<DamagePopup>();
        if (popup != null)
            popup.Setup(damage, false);
    }

    private void PlayHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0) return;

        SoundManagerSO.PlaySoundFXClip(hitSounds, transform.position, hitSoundVolume);
    }

    private void Die()
    {
        if (drop != null)
            drop.DropLoot();

        if (G.waveDirector != null)
            G.waveDirector.NotifyDeath(gameObject);

        if (isBoss)
            BossProgress.NotifyBossKilled();
        else
            BossProgress.AddXP(xpReward);

        Destroy(gameObject);
    }
}