using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject continueDisabledOverlay;

    private void Awake()
    {
        bool hasSave = SaveSystem.HasSave();

        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave();

        if (continueDisabledOverlay != null)
            continueDisabledOverlay.SetActive(!hasSave);
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
