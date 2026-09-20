using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 3;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float fallLimitY = -10f;
    [SerializeField, Min(0f)] private float damageCooldown = 0.5f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;
    public event Action Respawned;

    private Rigidbody2D rb;
    private Vector3 initialPosition;
    private float nextDamageTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    private void Update()
    {
        if (!IsDead && transform.position.y < fallLimitY)
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || Time.time < nextDamageTime)
            return;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        NotifyHealthChanged();

        if (IsDead)
        {
            Died?.Invoke();
            return;
        }

        Respawn();
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

    private void Respawn()
    {
        nextDamageTime = Time.time + damageCooldown;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.position = respawnPoint != null
            ? respawnPoint.position
            : initialPosition;

        Respawned?.Invoke();
    }
}
