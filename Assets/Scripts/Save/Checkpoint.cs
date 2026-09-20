using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointId;

    public string Id => checkpointId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            checkpointId = checkpointId,
            health = health.CurrentHealth,
            coins = 0
        };

        SaveSystem.Save(data);
    }
}
