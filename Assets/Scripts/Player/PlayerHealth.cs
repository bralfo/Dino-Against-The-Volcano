using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField, Min(1)] private int maxHealth = 3;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float fallLimitY = -10f;
    [SerializeField, Min(0f)] private float damageCooldown = 0.5f;

    [Header("Damage Feedback")]
    [SerializeField, Min(0f)] private float damageLift = 0.75f;
    [SerializeField, Min(0f)] private float blinkDuration = 0.15f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;
    private SpriteRenderer[] spriteRenderers;
    private Coroutine blinkCoroutine;

    public event Action<int, int> HealthChanged;
    public event Action Died;
    public event Action Respawned;
    public event Action LifeLost;

    private Rigidbody2D rb;
    private Vector3 initialPosition;
    private float nextDamageTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

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
            HandleFall();
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead || Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time + damageCooldown;

        if (!ApplyDamage(amount))
            return;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }

        transform.position += Vector3.up * damageLift;
        StartBlink();
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

    private bool ApplyDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        NotifyHealthChanged();

        LifeLost?.Invoke();

        if (IsDead)
        {
            Died?.Invoke();
            return false;
        }

        return true;
    }

    private void HandleFall()
    {
        if (!ApplyDamage(1))
            return;

        Respawn();
    }

    private void StartBlink()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
        }

        blinkCoroutine = StartCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        SetSpritesVisible(false);

        yield return new WaitForSeconds(blinkDuration);

        SetSpritesVisible(true);
        blinkCoroutine = null;
    }

    private void SetSpritesVisible(bool isVisible)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = isVisible;
        }
    }

    private void Respawn()
    {
        nextDamageTime = Time.time + damageCooldown;

        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        SetSpritesVisible(true);

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
