using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        Time.timeScale = 1f;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.Died += ShowGameOver;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.Died -= ShowGameOver;
    }

    private void ShowGameOver()
    {
        if (IsGameOver)
            return;

        IsGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
