using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneUI : MonoBehaviour
{
    private const string EndingSceneName = "EndScene";
    private const string MainMenuSceneName = "MainMenu";
    private const float ReturnToMenuDelay = 15f;
    private const int AnimationColumns = 8;
    private const int AnimationRows = 7;
    private const int AnimationFrameCount = 51;
    private const float AnimationFramesPerSecond = 10f;

    private RawImage animatedImage;
    private int currentFrame = -1;
    private float animationStartTime;
    private float returnToMenuTime;
    private bool isReturningToMenu;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        SceneManager.sceneLoaded -= BuildEndingScene;
        SceneManager.sceneLoaded += BuildEndingScene;
    }

    private static void BuildEndingScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != EndingSceneName)
            return;

        SaveSystem.DeleteSave();
        GameSession.PendingSave = null;

        if (FindAnyObjectByType<EndingSceneUI>() != null)
            return;

        GameObject root = new GameObject("EndingScreen");
        EndingSceneUI ending = root.AddComponent<EndingSceneUI>();
        ending.CreateInterface();
    }

    private void Update()
    {
        if (!isReturningToMenu && Time.unscaledTime >= returnToMenuTime)
        {
            isReturningToMenu = true;
            SceneManager.LoadScene(MainMenuSceneName);
            return;
        }

        if (animatedImage == null || animatedImage.texture == null)
            return;

        int frame = Mathf.FloorToInt(
            (Time.unscaledTime - animationStartTime) * AnimationFramesPerSecond)
            % AnimationFrameCount;

        if (frame == currentFrame)
            return;

        currentFrame = frame;
        int column = frame % AnimationColumns;
        int row = frame / AnimationColumns;

        animatedImage.uvRect = new Rect(
            column / (float)AnimationColumns,
            1f - (row + 1f) / AnimationRows,
            1f / AnimationColumns,
            1f / AnimationRows);
    }

    private void CreateInterface()
    {
        animationStartTime = Time.unscaledTime;
        returnToMenuTime = animationStartTime + ReturnToMenuDelay;

        GameObject canvasObject = new GameObject(
            "EndingCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.layer = LayerMask.NameToLayer("UI");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateBackground(canvasObject.transform);

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>(
            "Fonts & Materials/LiberationSans SDF");

        CreateText(canvasObject.transform, "YOU WIN!", 86f,
            new Color(1f, 0.78f, 0.15f), new Vector2(0f, 430f), new Vector2(1200f, 110f), font);
        CreateText(canvasObject.transform, "CONGRATULATIONS!", 58f,
            Color.white, new Vector2(0f, 340f), new Vector2(1300f, 85f), font);
        CreateText(canvasObject.transform, "THANKS FOR PLAYING.", 42f,
            new Color(0.82f, 0.65f, 1f), new Vector2(0f, 270f), new Vector2(1200f, 70f), font);

        CreateAnimatedImage(canvasObject.transform);

        CreateText(
            canvasObject.transform,
            "made by natadias, annamarquez and badulante.",
            25f,
            new Color(0.8f, 0.8f, 0.85f),
            new Vector2(0f, -485f),
            new Vector2(1100f, 50f),
            font);
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject backgroundObject = CreateUIObject("Background", parent);
        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        Stretch(rect);

        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0.035f, 0.015f, 0.075f, 1f);
        background.raycastTarget = false;
    }

    private void CreateAnimatedImage(Transform parent)
    {
        GameObject imageObject = CreateUIObject("TheWayAnimation", parent);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -105f);
        rect.sizeDelta = new Vector2(420f, 465f);

        animatedImage = imageObject.AddComponent<RawImage>();
        animatedImage.texture = Resources.Load<Texture2D>("theway_sheet");
        animatedImage.raycastTarget = false;
        animatedImage.color = Color.white;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string value,
        float fontSize,
        Color color,
        Vector2 position,
        Vector2 size,
        TMP_FontAsset font)
    {
        GameObject textObject = CreateUIObject("Text", parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateUIObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        gameObject.layer = LayerMask.NameToLayer("UI");
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
