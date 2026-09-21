using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private Button saveButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private InputAction pauseAction;
    private GameObject optionsPanel;
    private TMP_FontAsset menuFont;

    public bool IsPaused { get; private set; }

    private void Start()
    {
        EnsureSaveButton();
        EnsureOptionsMenu();
    }

    private void Awake()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (checkpointManager == null)
            checkpointManager = FindAnyObjectByType<CheckpointManager>();
    }

    private void OnEnable()
    {
        PlayerInput playerInput = FindAnyObjectByType<PlayerInput>();

        if (playerInput == null)
            return;

        pauseAction = playerInput.actions.FindAction("Pause");

        if (pauseAction != null)
            pauseAction.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnPausePerformed;
    }

    private void OnDestroy()
    {
        if (IsPaused)
            Time.timeScale = 1f;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (gameOverUI != null && gameOverUI.IsGameOver)
            return;

        SetPaused(!IsPaused);
    }

    public void ResumeGame()
    {
        SetPaused(false);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SaveGame()
    {
        if (checkpointManager == null)
            checkpointManager = FindAnyObjectByType<CheckpointManager>();

        if (checkpointManager != null)
            checkpointManager.SaveAtLastCheckpoint();
    }

    private void SetPaused(bool shouldPause)
    {
        if (checkpointManager == null)
            checkpointManager = FindAnyObjectByType<CheckpointManager>();

        IsPaused = shouldPause;

        if (pausePanel != null)
            pausePanel.SetActive(shouldPause);

        if (!shouldPause && optionsPanel != null)
            optionsPanel.SetActive(false);

        if (shouldPause && saveButton != null)
            saveButton.interactable = checkpointManager != null && checkpointManager.HasCheckpoint;

        Time.timeScale = shouldPause ? 0f : 1f;
    }

    private void EnsureSaveButton()
    {
        if (pausePanel == null)
            return;

        Transform existingSaveButton = pausePanel.transform.Find("SaveButton");

        if (existingSaveButton != null)
        {
            saveButton = existingSaveButton.GetComponent<Button>();
            return;
        }

        RectTransform resumeRect = FindChildRect("ResumeButton");
        RectTransform menuRect = FindChildRect("MenuButton");

        if (resumeRect != null)
            resumeRect.anchoredPosition = new Vector2(0f, 65f);

        if (menuRect != null)
            menuRect.anchoredPosition = new Vector2(0f, -135f);

        GameObject buttonObject = new GameObject(
            "SaveButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));

        buttonObject.layer = LayerMask.NameToLayer("UI");
        buttonObject.transform.SetParent(pausePanel.transform, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -35f);
        rect.sizeDelta = new Vector2(440f, 84f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.45f, 0.12f, 0.55f, 1f);

        saveButton = buttonObject.GetComponent<Button>();
        saveButton.targetGraphic = image;
        saveButton.onClick.AddListener(SaveGame);

        GameObject textObject = new GameObject(
            "Text",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        textObject.layer = LayerMask.NameToLayer("UI");
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = "SAVE GAME";
        text.fontSize = 34f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;

        TMP_Text template = resumeRect != null
            ? resumeRect.GetComponentInChildren<TMP_Text>(true)
            : null;

        if (template != null && template.font != null)
            text.font = template.font;
    }

    private void EnsureOptionsMenu()
    {
        if (pausePanel == null)
            return;

        RectTransform resumeRect = FindChildRect("ResumeButton");
        RectTransform saveRect = FindChildRect("SaveButton");
        RectTransform menuRect = FindChildRect("MenuButton");
        RectTransform pausedTextRect = FindChildRect("PausedText");

        if (resumeRect != null)
            resumeRect.anchoredPosition = new Vector2(0f, 125f);

        if (saveRect != null)
            saveRect.anchoredPosition = new Vector2(0f, 25f);

        if (menuRect != null)
            menuRect.anchoredPosition = new Vector2(0f, -175f);

        if (pausedTextRect != null)
            pausedTextRect.anchoredPosition = new Vector2(0f, 260f);

        TMP_Text template = resumeRect != null
            ? resumeRect.GetComponentInChildren<TMP_Text>(true)
            : null;

        if (template != null)
            menuFont = template.font;

        Transform existingOptionsButton = pausePanel.transform.Find("OptionsButton");

        if (existingOptionsButton == null)
        {
            Button optionsButton = CreateButton(
                pausePanel.transform,
                "OptionsButton",
                "OPTIONS",
                new Vector2(0f, -75f),
                new Vector2(440f, 84f));
            optionsButton.onClick.AddListener(ShowOptions);
        }

        CreateOptionsPanel();
    }

    private void CreateOptionsPanel()
    {
        Transform existingPanel = pausePanel.transform.Find("OptionsPanel");

        if (existingPanel != null)
        {
            optionsPanel = existingPanel.gameObject;
            optionsPanel.SetActive(false);
            return;
        }

        optionsPanel = new GameObject(
            "OptionsPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        optionsPanel.layer = LayerMask.NameToLayer("UI");
        optionsPanel.transform.SetParent(pausePanel.transform, false);

        RectTransform panelRect = optionsPanel.GetComponent<RectTransform>();
        Stretch(panelRect);

        Image panelImage = optionsPanel.GetComponent<Image>();
        panelImage.color = new Color(0.02f, 0.04f, 0.08f, 1f);

        CreateText(
            optionsPanel.transform,
            "Title",
            "OPTIONS",
            64f,
            new Vector2(0f, 310f),
            new Vector2(700f, 90f));

        CreateAudioSection(
            optionsPanel.transform,
            "Music",
            "MUSIC",
            180f,
            GameAudioSettings.MusicVolume,
            GameAudioSettings.MusicMuted,
            GameAudioSettings.SetMusicVolume,
            GameAudioSettings.SetMusicMuted);

        CreateAudioSection(
            optionsPanel.transform,
            "Effects",
            "EFFECTS",
            -35f,
            GameAudioSettings.EffectsVolume,
            GameAudioSettings.EffectsMuted,
            GameAudioSettings.SetEffectsVolume,
            GameAudioSettings.SetEffectsMuted);

        Button backButton = CreateButton(
            optionsPanel.transform,
            "BackButton",
            "BACK",
            new Vector2(0f, -300f),
            new Vector2(360f, 76f));
        backButton.onClick.AddListener(CloseOptions);

        optionsPanel.SetActive(false);
    }

    private void CreateAudioSection(
        Transform parent,
        string objectPrefix,
        string title,
        float verticalPosition,
        float volume,
        bool muted,
        Action<float> setVolume,
        Action<bool> setMuted)
    {
        CreateText(
            parent,
            $"{objectPrefix}Label",
            title,
            38f,
            new Vector2(0f, verticalPosition),
            new Vector2(500f, 55f));

        TextMeshProUGUI percentageText = CreateText(
            parent,
            $"{objectPrefix}Percentage",
            $"{Mathf.RoundToInt(volume * 100f)}%",
            28f,
            new Vector2(330f, verticalPosition - 65f),
            new Vector2(130f, 45f));

        Slider slider = CreateSlider(
            parent,
            $"{objectPrefix}Slider",
            new Vector2(-25f, verticalPosition - 65f),
            volume);
        slider.interactable = !muted;
        slider.onValueChanged.AddListener(value =>
        {
            percentageText.text = $"{Mathf.RoundToInt(value * 100f)}%";
            setVolume(value);
        });

        Toggle muteToggle = CreateToggle(
            parent,
            $"{objectPrefix}Mute",
            new Vector2(0f, verticalPosition - 125f),
            muted);
        muteToggle.onValueChanged.AddListener(value =>
        {
            slider.interactable = !value;
            setMuted(value);
        });
    }

    private Slider CreateSlider(
        Transform parent,
        string objectName,
        Vector2 position,
        float value)
    {
        GameObject sliderObject = CreateUIObject(objectName, parent);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        SetCenteredRect(sliderRect, position, new Vector2(560f, 42f));

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        GameObject backgroundObject = CreateUIObject("Background", sliderObject.transform);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        Stretch(backgroundRect);
        backgroundRect.offsetMin = new Vector2(0f, 14f);
        backgroundRect.offsetMax = new Vector2(0f, -14f);
        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        GameObject fillAreaObject = CreateUIObject("Fill Area", sliderObject.transform);
        RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
        Stretch(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(7f, 14f);
        fillAreaRect.offsetMax = new Vector2(-7f, -14f);

        GameObject fillObject = CreateUIObject("Fill", fillAreaObject.transform);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        Stretch(fillRect);
        Image fill = fillObject.AddComponent<Image>();
        fill.color = new Color(0.7f, 0.2f, 0.8f, 1f);

        GameObject handleAreaObject = CreateUIObject("Handle Slide Area", sliderObject.transform);
        RectTransform handleAreaRect = handleAreaObject.GetComponent<RectTransform>();
        Stretch(handleAreaRect);
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handleObject = CreateUIObject("Handle", handleAreaObject.transform);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(32f, 42f);
        Image handle = handleObject.AddComponent<Image>();
        handle.color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        slider.SetValueWithoutNotify(value);
        return slider;
    }

    private Toggle CreateToggle(
        Transform parent,
        string objectName,
        Vector2 position,
        bool value)
    {
        GameObject toggleObject = CreateUIObject(objectName, parent);
        RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
        SetCenteredRect(toggleRect, position, new Vector2(260f, 48f));

        Toggle toggle = toggleObject.AddComponent<Toggle>();

        GameObject backgroundObject = CreateUIObject("Background", toggleObject.transform);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0f, 0.5f);
        backgroundRect.pivot = new Vector2(0f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(40f, 40f);
        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0.16f, 0.18f, 0.24f, 1f);

        GameObject checkmarkObject = CreateUIObject("Checkmark", backgroundObject.transform);
        RectTransform checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
        Stretch(checkmarkRect);
        checkmarkRect.offsetMin = new Vector2(7f, 7f);
        checkmarkRect.offsetMax = new Vector2(-7f, -7f);
        Image checkmark = checkmarkObject.AddComponent<Image>();
        checkmark.color = new Color(0.7f, 0.2f, 0.8f, 1f);

        TextMeshProUGUI label = CreateText(
            toggleObject.transform,
            "Label",
            "MUTE",
            29f,
            new Vector2(70f, 0f),
            new Vector2(170f, 48f));
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        label.alignment = TextAlignmentOptions.Left;

        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.SetIsOnWithoutNotify(value);
        checkmark.canvasRenderer.SetAlpha(value ? 1f : 0f);
        return toggle;
    }

    private Button CreateButton(
        Transform parent,
        string objectName,
        string label,
        Vector2 position,
        Vector2 size)
    {
        GameObject buttonObject = CreateUIObject(objectName, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.45f, 0.12f, 0.55f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        TextMeshProUGUI text = CreateText(
            buttonObject.transform,
            "Text",
            label,
            34f,
            Vector2.zero,
            size);
        Stretch(text.rectTransform);
        return button;
    }

    private TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        string value,
        float fontSize,
        Vector2 position,
        Vector2 size)
    {
        GameObject textObject = CreateUIObject(objectName, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        SetCenteredRect(rect, position, size);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;

        if (menuFont != null)
            text.font = menuFont;

        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
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

    private static void SetCenteredRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void ShowOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    private void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    private RectTransform FindChildRect(string childName)
    {
        Transform child = pausePanel.transform.Find(childName);
        return child != null ? child.GetComponent<RectTransform>() : null;
    }
}
