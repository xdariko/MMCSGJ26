using System;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class BossCurrencyReward : MonoBehaviour
{
    [Serializable]
    private class CurrencyReward
    {
        public CurrencyType currency = CurrencyType.Basic;
        public int amount = 1;

        [Tooltip("Если true, награда выдастся только если эта валюта уже открыта.")]
        public bool requireUnlocked = true;

        [Tooltip("Если true, бонусы к выпадению валюты из дерева навыков будут влиять на эту награду.")]
        public bool useDropMultiplier = true;
    }

    [SerializeField] private CurrencyReward[] rewards;

    private EnemyHealth health;
    private bool rewarded;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        rewarded = false;

        if (health != null)
            health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDied;
    }

    private void HandleDied(EnemyHealth deadHealth)
    {
        if (rewarded)
            return;

        if (deadHealth == null || !deadHealth.IsBoss)
            return;

        rewarded = true;
        GiveRewards();
    }

    private void GiveRewards()
    {
        if (rewards == null)
            return;

        foreach (CurrencyReward reward in rewards)
        {
            if (reward == null)
                continue;

            if (reward.currency == CurrencyType.None)
                continue;

            if (reward.amount <= 0)
                continue;

            if (reward.requireUnlocked && !CurrencyManager.IsUnlocked(reward.currency))
                continue;

            int finalAmount = reward.amount;

            if (reward.useDropMultiplier)
            {
                finalAmount = PlayerStats.ApplyCurrencyDropMultiplier(
                    reward.currency,
                    reward.amount
                );
            }

            CurrencyManager.Add(reward.currency, finalAmount);
        }
    }
}