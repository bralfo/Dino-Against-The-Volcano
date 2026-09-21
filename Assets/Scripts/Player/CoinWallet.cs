using System;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class CoinWallet : MonoBehaviour
{
    [SerializeField, Min(1)] private int coinsPerHeart = 10;
    [SerializeField, Min(0)] private int coinsLostOnDamage = 5;

    public int CurrentCoins { get; private set; }
    public event Action<int> CoinsChanged;
    public event Action HeartEarned;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.LifeLost += HandleLifeLost;
    }

    private void Start()
    {
        NotifyCoinsChanged();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.LifeLost -= HandleLifeLost;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
            return;

        CurrentCoins += amount;
        ConvertCoinsToHealth();
        NotifyCoinsChanged();
    }

    public void RestoreCoins(int amount)
    {
        CurrentCoins = Mathf.Max(0, amount);
        NotifyCoinsChanged();
    }

    private void HandleLifeLost()
    {
        CurrentCoins = Mathf.Max(0, CurrentCoins - coinsLostOnDamage);
        NotifyCoinsChanged();
    }

    private void ConvertCoinsToHealth()
    {
        while (CurrentCoins >= coinsPerHeart
                && playerHealth.CurrentHealth < playerHealth.MaxHealth
                && !playerHealth.IsDead)
        {
            CurrentCoins -= coinsPerHeart;
            playerHealth.Heal(1);
            HeartEarned?.Invoke();
        }
    }

    private void NotifyCoinsChanged()
    {
        CoinsChanged?.Invoke(CurrentCoins);
    }
}
