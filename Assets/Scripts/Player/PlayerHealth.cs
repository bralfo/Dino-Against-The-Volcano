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
    [SerializeField, Min(0f)] private float damageCooldown = 1f;

    [Header("Damage Feedback")]
    [SerializeField, Min(0f)] private float damageLift = 0.75f;
    [SerializeField, Min(0f)] private float blinkDuration = 0.15f;
    [SerializeField, Min(0f)] private float damageKnockbackHorizontal = 5f;
    [SerializeField, Min(0f)] private float damageKnockbackVertical = 2.5f;
    [SerializeField, Min(0f)] private float damageKnockbackDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField, Range(0f, 1f)] private float damageSoundVolume = 0.2f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;
    public bool CanStompEnemy => rb != null
        && (rb.linearVelocity.y <= 0.1f || Time.time <= stompChainGraceUntil);
    private SpriteRenderer[] spriteRenderers;
    private Coroutine blinkCoroutine;
    private AudioSource audioSource;

    public event Action<int, int> HealthChanged;
    public event Action Died;
    public event Action Respawned;
    public event Action LifeLost;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private Vector3 initialPosition;
    private float nextDamageTime;
    private float stompChainGraceUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        initialPosition = transform.position;
        CurrentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
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
        TryTakeDamage(amount, transform.position);
    }

    public bool TryTakeDamage(int amount, Vector2 damageSourcePosition)
    {
        if (amount <= 0 || IsDead || Time.time < nextDamageTime)
            return false;

        nextDamageTime = Time.time + damageCooldown;

        if (!ApplyDamage(amount))
            return false;

        ApplyDamageKnockback(damageSourcePosition);

        transform.position += Vector3.up * damageLift;
        StartBlink();
        return true;
    }

    public void RegisterSuccessfulStomp()
    {
        stompChainGraceUntil = Time.time + 0.1f;
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

        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound, damageSoundVolume);

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

    private void ApplyDamageKnockback(Vector2 damageSourcePosition)
    {
        if (rb == null)
            return;

        float horizontalDirection = Mathf.Sign(transform.position.x - damageSourcePosition.x);

        if (Mathf.Approximately(horizontalDirection, 0f))
            horizontalDirection = rb.linearVelocity.x >= 0f ? -1f : 1f;

        Vector2 knockbackVelocity = new Vector2(
            horizontalDirection * damageKnockbackHorizontal,
            damageKnockbackVertical);

        if (playerController != null)
            playerController.ApplyKnockback(knockbackVelocity, damageKnockbackDuration);
        else
            rb.linearVelocity = knockbackVelocity;
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

    public void RestoreHealth(int health)
    {
        CurrentHealth = Mathf.Clamp(health, 1, maxHealth);
        NotifyHealthChanged();
    }

    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
    }
}
