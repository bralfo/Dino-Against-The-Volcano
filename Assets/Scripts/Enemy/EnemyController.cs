using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    [Header("Player interaction")]
    [SerializeField] private float stompBounceForce = 15f;
    [SerializeField, Min(0f)] private float stompTolerance = 0.2f;

    private bool isFacingRight = false;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private bool isDead;
    private PlayerHealth playerHealth;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool initialFacingRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialFacingRight = isFacingRight;
    }

    private void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
            return;

        playerHealth = playerObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
            playerHealth.LifeLost += RespawnEnemy;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.LifeLost -= RespawnEnemy;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float direction = isFacingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;

        transform.Rotate(0f, 180f, 0f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyWall"))
        {
            Flip();
            return;
        }

        PlayerHealth playerHealth = collision.collider.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null || isDead)
            return;

        Rigidbody2D playerBody = playerHealth.GetComponent<Rigidbody2D>();

        if (WasStomped(collision.collider, playerBody))
        {
            isDead = true;

            if (playerBody != null)
            {
                playerBody.linearVelocity = new Vector2(
                    playerBody.linearVelocity.x,
                    stompBounceForce
                );
            }

            gameObject.SetActive(false);
            return;
        }

        playerHealth.TakeDamage(1);
    }

    private bool WasStomped(Collider2D playerCollider, Rigidbody2D playerBody)
    {
        if (enemyCollider == null || playerBody == null)
            return false;

        bool playerIsFalling = playerBody.linearVelocity.y <= 0f;
        bool playerIsAbove = playerCollider.bounds.min.y >=
                             enemyCollider.bounds.max.y - stompTolerance;

        return playerIsFalling && playerIsAbove;
    }

    private void RespawnEnemy()
    {
        transform.SetPositionAndRotation(initialPosition, initialRotation);

        isFacingRight = initialFacingRight;
        isDead = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        gameObject.SetActive(true);
    }
}
