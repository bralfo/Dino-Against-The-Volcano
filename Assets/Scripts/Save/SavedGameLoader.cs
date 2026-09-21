using UnityEngine;

public class SavedGameLoader : MonoBehaviour
{
    void Start()
    {
        SaveData data = GameSession.PendingSave;

        if (data == null)
            return;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        CheckpointManager checkpointManager = FindAnyObjectByType<CheckpointManager>();

        if (player == null)
            return;

        CoinWallet wallet = player.GetComponent<CoinWallet>();
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>();

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint.Id != data.checkpointId)
                continue;

            player.transform.position = checkpoint.RespawnPoint.position;

            if (checkpointManager != null)
                checkpointManager.RestoreCheckpoint(checkpoint);
            else
                player.SetRespawnPoint(checkpoint.RespawnPoint);

            break;
        }

        player.RestoreHealth(data.health);

        if (wallet != null)
            wallet.RestoreCoins(data.coins);
        
        GameSession.PendingSave = null;
    }
}
