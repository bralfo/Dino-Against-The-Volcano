using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isGameOver;

    private void Awake()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        playerHealth.Died += ShowGameOver;
    }

    private void OnDisable()
    {
        playerHealth.Died -= ShowGameOver;
    }

    private void ShowGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
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
