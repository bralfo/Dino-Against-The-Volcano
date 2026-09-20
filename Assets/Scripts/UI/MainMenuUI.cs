using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        continueButton.interactable = SaveSystem.HasSave();
    }
    public void StartGame()
    {
        SaveSystem.DeleteSave();
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        SaveData data = SaveSystem.Load();

        if (data == null)
            return;

        GameSession.PendingSave = data;
        SceneManager.LoadScene(data.sceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
