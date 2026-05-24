using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 10f;

    [Header("State")]
    [SerializeField] private bool invulnerable;

    [Header("XP")]
    [SerializeField] private int xpReward;
    [SerializeField] private bool isBoss;

    [Header("Hit FX")]
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private float hitSoundVolume = 0.8f;

    private float currentHealth;
    private EnemyDrop drop;
    private GameObject wavePrefab;
    private bool dead;
    private bool removedFromWave;

    public bool IsBoss => isBoss;
    public bool IsDead => dead;
    public bool IsInvulnerable => invulnerable;

    public event Action<float> OnDamaged;
    public event Action<EnemyHealth> OnDied;

    public void SetXP(int xp) => xpReward = xp;
    public void SetBoss(bool boss) => isBoss = boss;
    public void SetWavePrefab(GameObject prefab) => wavePrefab = prefab;

    public void SetInvulnerable(bool value)
    {
        invulnerable = value;
    }

    private void Awake()
    {
        drop = GetComponent<EnemyDrop>();
        ResetHealthToMax();
    }

    private void OnEnable()
    {
        ResetHealthToMax();
    }

    private void ResetHealthToMax()
    {
        dead = false;
        removedFromWave = false;

        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;

        OnDamaged?.Invoke(GetHealthRatio());
    }

    public void SetMaxHealth(float value, bool refill = true)
    {
        maxHealth = Mathf.Max(1f, value);

        if (refill)
            currentHealth = maxHealth;
        else
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        OnDamaged?.Invoke(GetHealthRatio());
    }

    public void TakeDamage(float damage, bool isCrit = false)
    {
        if (dead)
            return;

        if (invulnerable)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        ShowDamagePopup(damage, isCrit);
        PlayHitSound();

        OnDamaged?.Invoke(GetHealthRatio());

        if (currentHealth <= 0f)
            Die();
    }

    public void DespawnWithoutReward()
    {
        if (dead)
            return;

        dead = true;

        NotifyWaveDirectorRemoved();

        Destroy(gameObject);
    }

    private float GetHealthRatio()
    {
        return maxHealth > 0f ? currentHealth / maxHealth : 0f;
    }

    private void ShowDamagePopup(float damage, bool isCrit)
    {
        if (G.damagePopupPrefab == null)
            return;

        Vector3 pos = transform.position + new Vector3(
            UnityEngine.Random.Range(-0.3f, 0.3f),
            0.5f,
            0f
        );

        GameObject go = Instantiate(G.damagePopupPrefab, pos, Quaternion.identity);

        DamagePopup popup = go.GetComponent<DamagePopup>();

        if (popup != null)
            popup.Setup(damage, isCrit);
    }

    private void PlayHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0)
            return;

        SoundManagerSO.PlaySoundFXClip(hitSounds, transform.position, hitSoundVolume);
    }

    private void Die()
    {
        if (dead)
            return;

        dead = true;

        OnDied?.Invoke(this);

        if (drop != null)
            drop.DropLoot();

        NotifyWaveDirectorRemoved();

        if (isBoss)
            BossProgress.NotifyBossKilled();
        else
            BossProgress.AddXP(xpReward);

        Destroy(gameObject);
    }

    private void NotifyWaveDirectorRemoved()
    {
        if (removedFromWave)
            return;

        removedFromWave = true;

        if (G.waveDirector == null)
            return;

        GameObject prefab = wavePrefab != null ? wavePrefab : gameObject;

        G.waveDirector.NotifyDeath(prefab);
    }

    private void OnDestroy()
    {
        if (removedFromWave)
            return;

        if (G.waveDirector == null)
            return;

        if (isBoss)
            return;

        GameObject prefab = wavePrefab != null ? wavePrefab : gameObject;

        G.waveDirector.NotifyDeath(prefab);

        removedFromWave = true;
    }
}