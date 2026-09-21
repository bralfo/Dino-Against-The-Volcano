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

    public bool IsPaused { get; private set; }

    private void Start()
    {
        EnsureSaveButton();
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

    private RectTransform FindChildRect(string childName)
    {
        Transform child = pausePanel.transform.Find(childName);
        return child != null ? child.GetComponent<RectTransform>() : null;
    }
}
