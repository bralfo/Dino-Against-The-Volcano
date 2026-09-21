using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour
{
    private const string GameplaySceneName = "GameScene";

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private CoinWallet coinWallet;
    [SerializeField] private Checkpoint initialCheckpoint;
    [SerializeField] private CheckpointNotificationUI notificationUI;

    public Checkpoint LastCheckpoint { get; private set; }
    public bool HasCheckpoint => LastCheckpoint != null;

    private bool runtimeInitialized;
    private float minimumGroundY;
    private float maximumGroundY;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GameplaySceneName)
            return;

        CheckpointManager manager = FindAnyObjectByType<CheckpointManager>();

        if (manager == null)
        {
            GameObject managerObject = new GameObject("CheckpointManager");
            manager = managerObject.AddComponent<CheckpointManager>();
        }

        manager.InitializeRuntimeSystem();
    }

    private void Awake()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (coinWallet == null && playerHealth != null)
            coinWallet = playerHealth.GetComponent<CoinWallet>();

        if (notificationUI == null)
            notificationUI = FindAnyObjectByType<CheckpointNotificationUI>();

        if (initialCheckpoint != null)
            SetCheckpoint(initialCheckpoint);
    }

    private void InitializeRuntimeSystem()
    {
        if (runtimeInitialized)
            return;

        runtimeInitialized = true;

        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (coinWallet == null)
            coinWallet = playerHealth.GetComponent<CoinWallet>();

        EnsureNotificationUI();
        CreateRuntimeCheckpoints();
    }

    public void ActivateCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null || checkpoint == LastCheckpoint)
            return;

        SetCheckpoint(checkpoint);
        SaveCurrentProgress(false);

        if (notificationUI != null)
            notificationUI.ShowCheckpoint();
    }

    public void RestoreCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint != null)
            SetCheckpoint(checkpoint);
    }

    public bool SaveAtLastCheckpoint()
    {
        return SaveCurrentProgress(true);
    }

    private void SetCheckpoint(Checkpoint checkpoint)
    {
        LastCheckpoint = checkpoint;

        if (playerHealth != null)
            playerHealth.SetRespawnPoint(checkpoint.RespawnPoint);
    }

    private bool SaveCurrentProgress(bool showConfirmation)
    {
        if (LastCheckpoint == null || playerHealth == null || coinWallet == null)
            return false;

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            checkpointId = LastCheckpoint.Id,
            health = playerHealth.CurrentHealth,
            coins = coinWallet.CurrentCoins
        };

        SaveSystem.Save(data);

        if (showConfirmation && notificationUI != null)
            notificationUI.ShowSaved();

        return true;
    }

    private void EnsureNotificationUI()
    {
        if (notificationUI != null)
            return;

        notificationUI = FindAnyObjectByType<CheckpointNotificationUI>();

        if (notificationUI != null)
            return;

        Canvas hud = null;

        foreach (Canvas canvas in FindObjectsByType<Canvas>())
        {
            if (canvas.name == "HUD")
            {
                hud = canvas;
                break;
            }
        }

        if (hud == null)
            return;

        GameObject notificationObject = new GameObject(
            "CheckpointNotification",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(TextMeshProUGUI));

        notificationObject.layer = LayerMask.NameToLayer("UI");
        notificationObject.transform.SetParent(hud.transform, false);

        RectTransform rect = notificationObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.72f);
        rect.anchorMax = new Vector2(0.5f, 0.72f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(720f, 100f);

        TextMeshProUGUI text = notificationObject.GetComponent<TextMeshProUGUI>();
        text.text = "CHECKPOINT";
        text.fontSize = 58f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.82f, 0.2f, 1f);
        text.outlineColor = new Color32(35, 5, 45, 255);
        text.outlineWidth = 0.25f;
        text.raycastTarget = false;

        TMP_Text existingText = FindAnyObjectByType<TMP_Text>();

        if (existingText != null && existingText.font != null)
            text.font = existingText.font;

        CanvasGroup canvasGroup = notificationObject.GetComponent<CanvasGroup>();
        notificationUI = notificationObject.AddComponent<CheckpointNotificationUI>();
        notificationUI.Initialize(canvasGroup, text);
    }

    private void CreateRuntimeCheckpoints()
    {
        foreach (Checkpoint existingCheckpoint in FindObjectsByType<Checkpoint>())
            existingCheckpoint.gameObject.SetActive(false);

        GameObject container = new GameObject("Checkpoints");
        Transform gameplayRoot = FindTransformByName("_Gameplay");

        if (gameplayRoot != null)
            container.transform.SetParent(gameplayRoot, true);

        Checkpoint start = CreateCheckpoint(
            container.transform,
            "Checkpoint_Start",
            "checkpoint_start",
            playerHealth.transform.position,
            false);

        initialCheckpoint = start;
        SetCheckpoint(start);

        Physics2D.SyncTransforms();

        if (!TryGetGroundRange(out float minimumX, out float maximumX))
            return;

        float[] fractions = { 0.25f, 0.5f, 0.75f };

        for (int index = 0; index < fractions.Length; index++)
        {
            float targetX = Mathf.Lerp(minimumX, maximumX, fractions[index]);

            if (!TryFindGroundPosition(targetX, out Vector2 position))
                continue;

            CreateCheckpoint(
                container.transform,
                $"Checkpoint_{index + 1}",
                $"checkpoint_{index + 1}",
                position,
                true);
        }
    }

    private Checkpoint CreateCheckpoint(
        Transform parent,
        string objectName,
        string id,
        Vector2 groundPosition,
        bool createMarker)
    {
        GameObject checkpointObject = new GameObject(objectName);
        checkpointObject.transform.SetParent(parent, true);
        checkpointObject.transform.position = groundPosition + Vector2.up * 2.5f;

        BoxCollider2D trigger = checkpointObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(2f, 5f);

        Transform respawn = new GameObject("RespawnPoint").transform;
        respawn.SetParent(checkpointObject.transform, false);
        respawn.localPosition = new Vector3(0f, -1f, 0f);

        Checkpoint checkpoint = checkpointObject.AddComponent<Checkpoint>();
        checkpoint.Configure(id, respawn);

        if (createMarker)
            CreateMarker(checkpointObject.transform);

        return checkpoint;
    }

    private static void CreateMarker(Transform checkpoint)
    {
        GameObject poleObject = new GameObject("Marker");
        poleObject.transform.SetParent(checkpoint, false);

        LineRenderer pole = poleObject.AddComponent<LineRenderer>();
        pole.useWorldSpace = false;
        pole.positionCount = 2;
        pole.SetPosition(0, new Vector3(0f, -2.5f, 0f));
        pole.SetPosition(1, new Vector3(0f, 2.5f, 0f));
        pole.startWidth = 0.12f;
        pole.endWidth = 0.12f;
        pole.startColor = new Color(1f, 0.75f, 0.15f, 1f);
        pole.endColor = pole.startColor;
        pole.sortingOrder = 20;

        Shader spriteShader = Shader.Find("Sprites/Default");

        if (spriteShader != null)
            pole.material = new Material(spriteShader);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshPro));
        labelObject.transform.SetParent(checkpoint, false);
        labelObject.transform.localPosition = new Vector3(0f, 2.8f, 0f);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(8f, 1.5f);

        TextMeshPro label = labelObject.GetComponent<TextMeshPro>();
        label.text = "CHECKPOINT";
        label.fontSize = 2.4f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.82f, 0.2f, 1f);
        label.outlineColor = new Color32(35, 5, 45, 255);
        label.outlineWidth = 0.2f;
        label.sortingOrder = 21;
    }

    private bool TryGetGroundRange(out float minimumX, out float maximumX)
    {
        minimumX = float.PositiveInfinity;
        maximumX = float.NegativeInfinity;
        minimumGroundY = float.PositiveInfinity;
        maximumGroundY = float.NegativeInfinity;
        int groundLayer = LayerMask.NameToLayer("Ground");

        foreach (Collider2D collider in FindObjectsByType<Collider2D>())
        {
            if (!collider.enabled || collider.isTrigger || collider.gameObject.layer != groundLayer)
                continue;

            minimumX = Mathf.Min(minimumX, collider.bounds.min.x);
            maximumX = Mathf.Max(maximumX, collider.bounds.max.x);
            minimumGroundY = Mathf.Min(minimumGroundY, collider.bounds.min.y);
            maximumGroundY = Mathf.Max(maximumGroundY, collider.bounds.max.y);
        }

        return !float.IsInfinity(minimumX)
            && !float.IsInfinity(maximumX)
            && !float.IsInfinity(minimumGroundY)
            && !float.IsInfinity(maximumGroundY)
            && maximumX - minimumX > 10f;
    }

    private bool TryFindGroundPosition(float targetX, out Vector2 position)
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        int groundMask = 1 << groundLayer;
        float rayStartY = maximumGroundY + 10f;
        float maximumDistance = Mathf.Max(40f, maximumGroundY - minimumGroundY + 20f);
        const float sampleSpacing = 2f;

        for (int sample = 0; sample <= 12; sample++)
        {
            float offset = sample * sampleSpacing;
            float[] candidateX = sample == 0
                ? new[] { targetX }
                : new[] { targetX + offset, targetX - offset };

            foreach (float x in candidateX)
            {
                if (TryFindBestGroundAtX(
                    x,
                    rayStartY,
                    maximumDistance,
                    groundMask,
                    out position))
                {
                    return true;
                }
            }
        }

        position = default;
        return false;
    }

    private bool TryFindBestGroundAtX(
        float x,
        float rayStartY,
        float maximumDistance,
        int groundMask,
        out Vector2 position)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(
            new Vector2(x, rayStartY),
            Vector2.down,
            maximumDistance,
            groundMask);

        float bestScore = float.PositiveInfinity;
        Vector2 bestPosition = default;
        bool foundPosition = false;
        float playerStartY = playerHealth.transform.position.y;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.normal.y < 0.5f)
                continue;

            Vector2 clearanceCenter = hit.point + Vector2.up * 1.6f;
            Collider2D obstruction = Physics2D.OverlapBox(
                clearanceCenter,
                new Vector2(1.2f, 2.4f),
                0f,
                groundMask);

            if (obstruction != null && obstruction != hit.collider)
                continue;

            float verticalDistance = Mathf.Abs(hit.point.y - playerStartY);
            float narrowPlatformPenalty = hit.collider.bounds.size.x < 4f ? 8f : 0f;
            float score = verticalDistance + narrowPlatformPenalty;

            if (score >= bestScore)
                continue;

            bestScore = score;
            bestPosition = hit.point;
            foundPosition = true;
        }

        position = bestPosition;
        return foundPosition;
    }

    private static Transform FindTransformByName(string objectName)
    {
        foreach (Transform transform in FindObjectsByType<Transform>())
        {
            if (transform.name == objectName)
                return transform;
        }

        return null;
    }
}
