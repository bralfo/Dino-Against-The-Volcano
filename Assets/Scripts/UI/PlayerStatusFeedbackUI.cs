using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerStatusFeedbackUI : MonoBehaviour
{
    private const string GameplaySceneName = "GameScene";

    [SerializeField, Min(0.1f)] private float animationDuration = 1.2f;
    [SerializeField, Min(0f)] private float riseDistance = 110f;

    private Canvas hudCanvas;
    private RectTransform canvasRect;
    private PlayerHealth playerHealth;
    private CoinWallet coinWallet;
    private Sprite coinSprite;
    private Sprite heartSprite;
    private TMP_FontAsset font;

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

        Canvas hud = FindHud();
        PlayerHealth health = FindAnyObjectByType<PlayerHealth>();

        if (hud == null || health == null)
            return;

        PlayerStatusFeedbackUI feedback = hud.GetComponent<PlayerStatusFeedbackUI>();

        if (feedback == null)
            feedback = hud.gameObject.AddComponent<PlayerStatusFeedbackUI>();

        feedback.Initialize(hud, health);
    }

    private void Initialize(Canvas hud, PlayerHealth health)
    {
        Unsubscribe();

        hudCanvas = hud;
        canvasRect = hud.transform as RectTransform;
        playerHealth = health;
        coinWallet = health.GetComponent<CoinWallet>();

        FindVisualAssets();

        playerHealth.LifeLost += ShowDamageLoss;

        if (coinWallet != null)
            coinWallet.HeartEarned += ShowHeartEarned;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (playerHealth != null)
            playerHealth.LifeLost -= ShowDamageLoss;

        if (coinWallet != null)
            coinWallet.HeartEarned -= ShowHeartEarned;
    }

    private void ShowDamageLoss()
    {
        Vector2 origin = GetPlayerCanvasPosition();
        CreatePopup("-5", coinSprite, new Color(1f, 0.78f, 0.16f), origin + new Vector2(-75f, 0f));
        CreatePopup("-1", heartSprite, new Color(1f, 0.25f, 0.25f), origin + new Vector2(75f, 0f));
    }

    private void ShowHeartEarned()
    {
        CreatePopup(
            "+1",
            heartSprite,
            new Color(0.35f, 1f, 0.45f),
            GetPlayerCanvasPosition());
    }

    private void CreatePopup(string value, Sprite iconSprite, Color textColor, Vector2 position)
    {
        GameObject popupObject = new GameObject(
            $"StatusPopup_{value}",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(HorizontalLayoutGroup));

        popupObject.layer = LayerMask.NameToLayer("UI");
        popupObject.transform.SetParent(hudCanvas.transform, false);

        RectTransform popupRect = popupObject.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = position;
        popupRect.sizeDelta = new Vector2(145f, 64f);

        HorizontalLayoutGroup layout = popupObject.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        TextMeshProUGUI text = CreateText(popupObject.transform, value, textColor);
        Image icon = CreateIcon(popupObject.transform, iconSprite);

        text.rectTransform.sizeDelta = new Vector2(76f, 60f);
        icon.rectTransform.sizeDelta = new Vector2(52f, 52f);

        StartCoroutine(AnimatePopup(popupRect, popupObject.GetComponent<CanvasGroup>()));
    }

    private TextMeshProUGUI CreateText(Transform parent, string value, Color color)
    {
        GameObject textObject = new GameObject(
            "Value",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        textObject.layer = LayerMask.NameToLayer("UI");
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = 46f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.outlineColor = new Color32(35, 5, 45, 255);
        text.outlineWidth = 0.25f;
        text.raycastTarget = false;
        return text;
    }

    private static Image CreateIcon(Transform parent, Sprite sprite)
    {
        GameObject iconObject = new GameObject(
            "Icon",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        iconObject.layer = LayerMask.NameToLayer("UI");
        iconObject.transform.SetParent(parent, false);

        Image icon = iconObject.GetComponent<Image>();
        icon.sprite = sprite;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        return icon;
    }

    private IEnumerator AnimatePopup(RectTransform popup, CanvasGroup canvasGroup)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float delta = Time.unscaledDeltaTime;
            elapsed += delta;

            float progress = Mathf.Clamp01(elapsed / animationDuration);
            popup.anchoredPosition += Vector2.up * (riseDistance / animationDuration) * delta;
            canvasGroup.alpha = 1f - progress;

            yield return null;
        }

        Destroy(popup.gameObject);
    }

    private Vector2 GetPlayerCanvasPosition()
    {
        Camera camera = Camera.main;
        Vector3 worldPosition = playerHealth.transform.position + Vector3.up * 2f;
        Vector2 screenPosition = camera != null
            ? camera.WorldToScreenPoint(worldPosition)
            : worldPosition;

        Camera eventCamera = hudCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : hudCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            eventCamera,
            out Vector2 localPosition);

        return localPosition;
    }

    private void FindVisualAssets()
    {
        foreach (Image image in FindObjectsByType<Image>())
        {
            if (image.name == "CoinIcon")
                coinSprite = image.sprite;
            else if (image.name == "Heart1")
                heartSprite = image.sprite;
        }

        TMP_Text existingText = FindAnyObjectByType<TMP_Text>();

        if (existingText != null)
            font = existingText.font;
    }

    private static Canvas FindHud()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>())
        {
            if (canvas.name == "HUD")
                return canvas;
        }

        return null;
    }
}
