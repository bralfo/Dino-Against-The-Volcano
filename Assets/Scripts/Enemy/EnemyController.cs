using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    [Header("Patrol")]
    [SerializeField] private LayerMask groundLayer = 1 << 6;
    [SerializeField, Min(0.01f)] private float groundCheckDepth = 0.5f;
    [SerializeField, Min(0f)] private float groundAheadDistance = 0.05f;
    [SerializeField, Min(0.01f)] private float groundCheckWidth = 0.12f;
    [SerializeField, Min(0.01f)] private float obstacleCheckDistance = 0.08f;
    [SerializeField, Range(0f, 0.25f)] private float screenEdgeMargin = 0.02f;
    [SerializeField, Min(0f)] private float turnCooldown = 0.15f;

    [Header("Player interaction")]
    [SerializeField] private float stompBounceForce = 15f;
    [SerializeField, Min(0f)] private float stompTolerance = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField, Range(0f, 1f)] private float deathSoundVolume = 0.2f;

    [Header("Death animation")]
    [SerializeField] private string deathAnimationState;
    [SerializeField, Min(0f)] private float deathAnimationDuration = 0.85f;
 
    private bool isFacingRight = false;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private Animator animator;
    private bool isDead;
    private PlayerHealth playerHealth;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool initialFacingRight;
    private Camera mainCamera;
    private float nextTurnTime;
    private Collider2D ignoredPlayerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main;

        FitColliderToSprite();

        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer >= 0)
            Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialFacingRight = isFacingRight;
    }

    private void FitColliderToSprite()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null || spriteRenderer.sprite == null || enemyCollider == null)
            return;

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        Vector2 fittedSize = spriteBounds.size;
        fittedSize.x *= 0.85f;
        fittedSize.y *= 0.85f;

        if (enemyCollider is BoxCollider2D boxCollider)
        {
            boxCollider.offset = spriteBounds.center;
            boxCollider.size = fittedSize;
        }
        else if (enemyCollider is CapsuleCollider2D capsuleCollider)
        {
            capsuleCollider.offset = spriteBounds.center;
            capsuleCollider.size = fittedSize;
            capsuleCollider.direction = CapsuleDirection2D.Horizontal;
        }
    }

    private void Start()
    {
        Move();

        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
            return;

        playerHealth = playerObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.LifeLost += RespawnEnemy;
    }

    private void OnDestroy()
    {
        RestorePlayerCollision();

        if (playerHealth != null)
            playerHealth.LifeLost -= RespawnEnemy;
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (ShouldTurnAround())
            TryFlip();

        Move();
    }

    private bool ShouldTurnAround()
    {
        if (Time.time < nextTurnTime || enemyCollider == null)
            return false;

        float direction = isFacingRight ? 1f : -1f;

        if (ReachedScreenEdge(direction))
            return true;

        if (HasObstacleAhead(direction))
            return true;

        bool isOnGround = IsOnGround();
        bool hasGroundAhead = HasGroundAhead(direction);

        if (isOnGround && !hasGroundAhead)
            return true;

        return false;
    }

    private bool HasObstacleAhead(float direction)
    {
        Bounds bounds = enemyCollider.bounds;
        float rayDistance = bounds.extents.x + obstacleCheckDistance;
        int obstacleLayers = groundLayer.value;
        int enemyWallLayer = LayerMask.NameToLayer("EnemyWall");

        if (enemyWallLayer >= 0)
            obstacleLayers |= 1 << enemyWallLayer;

        Vector2 lowerOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y + bounds.size.y * 0.45f
        );
        Vector2 upperOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y + bounds.size.y * 0.7f
        );
        Vector2 rayDirection = direction > 0f ? Vector2.right : Vector2.left;

        return Physics2D.Raycast(lowerOrigin, rayDirection, rayDistance, obstacleLayers)
            || Physics2D.Raycast(upperOrigin, rayDirection, rayDistance, obstacleLayers);
    }

    private bool ReachedScreenEdge(float direction)
    {
        if (mainCamera == null)
            return false;

        Bounds bounds = enemyCollider.bounds;
        float worldX = direction > 0f ? bounds.max.x : bounds.min.x;
        float viewportX = mainCamera.WorldToViewportPoint(
            new Vector3(worldX, bounds.center.y, transform.position.z)
        ).x;

        return direction > 0f
            ? viewportX >= 1f - screenEdgeMargin
            : viewportX <= screenEdgeMargin;
    }

    private bool IsOnGround()
    {
        Bounds bounds = enemyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.05f);

        return Physics2D.Raycast(origin, Vector2.down, groundCheckDepth, groundLayer);
    }

    private bool HasGroundAhead(float direction)
    {
        Bounds bounds = enemyCollider.bounds;
        Vector2 origin = new Vector2(
            bounds.center.x + direction * (bounds.extents.x + groundAheadDistance),
            bounds.min.y + 0.05f
        );

        Vector2 checkSize = new Vector2(groundCheckWidth, 0.05f);

        return Physics2D.BoxCast(
            origin,
            checkSize,
            0f,
            Vector2.down,
            groundCheckDepth,
            groundLayer
        );
    }

    private void Move()
    {
        if (rb == null)
            return;

        float direction = isFacingRight ? 1f : -1f;

        if (rb.IsSleeping())
            rb.WakeUp();

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void TryFlip()
    {
        if (Time.time < nextTurnTime)
            return;

        isFacingRight = !isFacingRight;
        nextTurnTime = Time.time + turnCooldown;

        transform.Rotate(0f, 180f, 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        EnemyController otherEnemy = collision.collider.GetComponentInParent<EnemyController>();

        if (otherEnemy != null && otherEnemy != this)
        {
            if (enemyCollider != null)
                Physics2D.IgnoreCollision(enemyCollider, collision.collider, true);

            return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyWall"))
        {
            TryFlip();
            return;
        }

        PlayerHealth playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (isDead)
            return;

        Rigidbody2D playerBody = playerHealth.GetComponent<Rigidbody2D>();

        if (WasStomped(collision, collision.collider, playerHealth))
        {
            isDead = true;
            playerHealth.RegisterSuccessfulStomp();

            if (playerBody != null)
            {
                playerBody.linearVelocity = new Vector2(
                    playerBody.linearVelocity.x,
                    stompBounceForce
                );
            }

            if (deathSound != null)
            {
                Vector3 soundPosition = Camera.main != null
                    ? Camera.main.transform.position
                    : transform.position;

                AudioSource.PlayClipAtPoint(deathSound, soundPosition, deathSoundVolume);
            }

            PlayDeathAnimationOrDisable();
            return;
        }

        playerHealth.QueueContactDamage(1, transform.position);
        StartIgnoringPlayerUntilSeparated(collision.collider);
    }

    private void StartIgnoringPlayerUntilSeparated(Collider2D playerCollider)
    {
        if (enemyCollider == null || playerCollider == null)
            return;

        if (ignoredPlayerCollider == playerCollider)
            return;

        RestorePlayerCollision();

        ignoredPlayerCollider = playerCollider;
        Physics2D.IgnoreCollision(enemyCollider, playerCollider, true);
        StartCoroutine(RestorePlayerCollisionWhenSeparated(playerCollider));
    }

    private IEnumerator RestorePlayerCollisionWhenSeparated(Collider2D playerCollider)
    {
        yield return new WaitForFixedUpdate();

        while (enemyCollider != null
               && playerCollider != null
               && enemyCollider.bounds.Intersects(playerCollider.bounds))
        {
            yield return new WaitForFixedUpdate();
        }

        if (ignoredPlayerCollider == playerCollider)
            RestorePlayerCollision();
    }

    private void RestorePlayerCollision()
    {
        if (enemyCollider != null && ignoredPlayerCollider != null)
            Physics2D.IgnoreCollision(enemyCollider, ignoredPlayerCollider, false);

        ignoredPlayerCollider = null;
    }

    private bool WasStomped(
        Collision2D collision,
        Collider2D playerCollider,
        PlayerHealth playerHealth)
    {
        if (enemyCollider == null || playerCollider == null || playerHealth == null)
            return false;

        if (!playerHealth.CanStompEnemy)
            return false;

        Bounds enemyBounds = enemyCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;
        float topBandDepth = Mathf.Max(stompTolerance, enemyBounds.size.y * 0.35f);
        float topBandMinimum = enemyBounds.max.y - topBandDepth;

        bool playerCenterIsAbove = playerBounds.center.y > enemyBounds.center.y;
        bool horizontalOverlap = playerBounds.max.x >= enemyBounds.min.x
            && playerBounds.min.x <= enemyBounds.max.x;
        bool playerFeetAreNearTop = playerBounds.min.y >= topBandMinimum;
        bool hasTopContact = false;

        for (int index = 0; index < collision.contactCount; index++)
        {
            Vector2 contactPoint = collision.GetContact(index).point;

            if (contactPoint.y >= topBandMinimum
                && contactPoint.x >= enemyBounds.min.x - 0.05f
                && contactPoint.x <= enemyBounds.max.x + 0.05f)
            {
                hasTopContact = true;
                break;
            }
        }

        return playerCenterIsAbove
            && horizontalOverlap
            && (playerFeetAreNearTop || hasTopContact);
    }

    private void PlayDeathAnimationOrDisable()
    {
        RestorePlayerCollision();

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (animator == null || string.IsNullOrWhiteSpace(deathAnimationState))
        {
            gameObject.SetActive(false);
            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        animator.Play(deathAnimationState, 0, 0f);
        StartCoroutine(DisableAfterDeathAnimation());
    }

    private IEnumerator DisableAfterDeathAnimation()
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        gameObject.SetActive(false);
    }

    private void RespawnEnemy()
    {
        RestorePlayerCollision();
        StopAllCoroutines();
        gameObject.SetActive(true);
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        isFacingRight = initialFacingRight;
        isDead = false;
        nextTurnTime = Time.time + turnCooldown;

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (enemyCollider != null)
            enemyCollider.enabled = true;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D currentCollider = enemyCollider != null
            ? enemyCollider
            : GetComponent<Collider2D>();

        if (currentCollider == null)
            return;

        float direction = isFacingRight ? 1f : -1f;
        Bounds bounds = currentCollider.bounds;

        Vector2 groundOrigin = new Vector2(
            bounds.center.x + direction * (bounds.extents.x + groundAheadDistance),
            bounds.min.y + 0.05f
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            groundOrigin + Vector2.down * groundCheckDepth,
            new Vector2(groundCheckWidth, 0.05f)
        );

        float rayDistance = bounds.extents.x + obstacleCheckDistance;
        Vector2 rayDirection = direction > 0f ? Vector2.right : Vector2.left;
        Vector2 lowerObstacleOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y + bounds.size.y * 0.45f
        );
        Vector2 upperObstacleOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y + bounds.size.y * 0.7f
        );

        Gizmos.color = Color.red;
        Gizmos.DrawLine(lowerObstacleOrigin, lowerObstacleOrigin + rayDirection * rayDistance);
        Gizmos.DrawLine(upperObstacleOrigin, upperObstacleOrigin + rayDirection * rayDistance);

    }
}
