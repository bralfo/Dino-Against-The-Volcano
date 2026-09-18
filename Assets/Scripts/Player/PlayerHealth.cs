using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        NotifyHealthChanged();

        if (IsDead)
            Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        NotifyHealthChanged();
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        NotifyHealthChanged();
    }

    private void NotifyHealthChanged()
    {
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
