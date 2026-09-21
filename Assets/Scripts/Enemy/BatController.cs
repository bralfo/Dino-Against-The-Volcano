using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
public class BatController : MonoBehaviour
{
    private const int AdditionalBatCount = 3;

    [Header("Flight")]
    [SerializeField] private Vector2 horizontalSpeedRange = new Vector2(2.5f, 4.5f);
    [SerializeField] private Vector2 verticalSpeedRange = new Vector2(1.25f, 2.5f);
    [SerializeField] private Vector2 altitudeChangeInterval = new Vector2(1.5f, 4f);
    [SerializeField, Min(0f)] private float horizontalBoundaryMargin = 1.5f;
    [SerializeField, Min(0f)] private float verticalBoundaryMargin = 1.5f;

    private static bool additionalBatsCreated;

    private Rigidbody2D body;
    private Collider2D batCollider;
    private SpriteRenderer spriteRenderer;
    private float leftBoundary;
    private float rightBoundary;
    private float bottomBoundary;
    private float topBoundary;
    private float horizontalSpeed;
    private float verticalSpeed;
    private float horizontalDirection;
    private float targetAltitude;
    private float nextAltitudeChangeTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        additionalBatsCreated = false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        batCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer >= 0)
            gameObject.layer = enemyLayer;

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // O morcego atravessa cenário e outros inimigos. O trigger ainda detecta o Player.
        batCollider.isTrigger = true;

        CalculateMapBoundaries();
        RandomizeFlight();
    }

    private void Start()
    {
        if (additionalBatsCreated)
            return;

        additionalBatsCreated = true;
        CreateAdditionalBats();
    }

    private void FixedUpdate()
    {
        Vector2 position = body.position;

        if (position.x <= leftBoundary)
        {
            position.x = leftBoundary;
            horizontalDirection = 1f;
        }
        else if (position.x >= rightBoundary)
        {
            position.x = rightBoundary;
            horizontalDirection = -1f;
        }

        if (Time.time >= nextAltitudeChangeTime
            || Mathf.Abs(position.y - targetAltitude) < 0.25f)
        {
            ChooseNewAltitude();
        }

        float verticalVelocity = Mathf.Clamp(
            (targetAltitude - position.y) * 1.5f,
            -verticalSpeed,
            verticalSpeed);

        position.y = Mathf.Clamp(position.y, bottomBoundary, topBoundary);
        body.position = position;
        body.linearVelocity = new Vector2(
            horizontalDirection * horizontalSpeed,
            verticalVelocity);

        spriteRenderer.flipX = horizontalDirection > 0f;
    }

    private void CalculateMapBoundaries()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        float minimumX = float.PositiveInfinity;
        float maximumX = float.NegativeInfinity;

        foreach (Collider2D collider in FindObjectsByType<Collider2D>())
        {
            if (!collider.enabled
                || collider.isTrigger
                || collider.gameObject.layer != groundLayer
                || collider.GetComponentInParent<BatController>() != null)
            {
                continue;
            }

            minimumX = Mathf.Min(minimumX, collider.bounds.min.x);
            maximumX = Mathf.Max(maximumX, collider.bounds.max.x);
        }

        Camera mainCamera = Camera.main;

        if (float.IsInfinity(minimumX) || float.IsInfinity(maximumX))
        {
            float cameraHalfWidth = mainCamera != null
                ? mainCamera.orthographicSize * mainCamera.aspect
                : 10f;

            minimumX = transform.position.x - cameraHalfWidth;
            maximumX = transform.position.x + cameraHalfWidth;
        }

        leftBoundary = minimumX + horizontalBoundaryMargin;
        rightBoundary = maximumX - horizontalBoundaryMargin;

        float cameraY = mainCamera != null ? mainCamera.transform.position.y : transform.position.y;
        float cameraHalfHeight = mainCamera != null ? mainCamera.orthographicSize : 6f;

        bottomBoundary = cameraY - cameraHalfHeight + verticalBoundaryMargin;
        topBoundary = cameraY + cameraHalfHeight - verticalBoundaryMargin;

        if (topBoundary <= bottomBoundary)
        {
            bottomBoundary = transform.position.y - 2f;
            topBoundary = transform.position.y + 2f;
        }
    }

    private void RandomizeFlight()
    {
        horizontalSpeed = Random.Range(horizontalSpeedRange.x, horizontalSpeedRange.y);
        verticalSpeed = Random.Range(verticalSpeedRange.x, verticalSpeedRange.y);
        horizontalDirection = Random.value < 0.5f ? -1f : 1f;
        ChooseNewAltitude();
    }

    private void ChooseNewAltitude()
    {
        targetAltitude = Random.Range(bottomBoundary, topBoundary);
        nextAltitudeChangeTime = Time.time
            + Random.Range(altitudeChangeInterval.x, altitudeChangeInterval.y);
    }

    private void CreateAdditionalBats()
    {
        float availableWidth = Mathf.Max(1f, rightBoundary - leftBoundary);

        for (int index = 0; index < AdditionalBatCount; index++)
        {
            GameObject extraBat = Instantiate(gameObject, transform.parent);
            extraBat.name = $"Bat Extra ({index + 1})";

            float horizontalFraction = (index + 1f) / (AdditionalBatCount + 1f);
            float spawnX = leftBoundary + availableWidth * horizontalFraction;
            float spawnY = Random.Range(bottomBoundary, topBoundary);

            extraBat.transform.position = new Vector3(
                spawnX,
                spawnY,
                transform.position.z);
        }
    }
}
