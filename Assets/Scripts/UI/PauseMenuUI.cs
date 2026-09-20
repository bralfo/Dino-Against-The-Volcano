using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private InputAction pauseAction;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerInput playerInput = FindFirstObjectByType<PlayerInput>();

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

    private void SetPaused(bool shouldPause)
    {
        IsPaused = shouldPause;

        if (pausePanel != null)
            pausePanel.SetActive(shouldPause);

        Time.timeScale = shouldPause ? 0f : 1f;
    }
}
