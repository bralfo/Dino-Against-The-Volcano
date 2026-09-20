using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointId;

    public string Id => checkpointId;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        CoinWallet wallet = other.GetComponentInParent<CoinWallet>();

        if (health == null || wallet == null)
            return;

        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            checkpointId = checkpointId,
            health = health.CurrentHealth,
            coins = wallet.CurrentCoins
        };

        SaveSystem.Save(data);
    }
}
