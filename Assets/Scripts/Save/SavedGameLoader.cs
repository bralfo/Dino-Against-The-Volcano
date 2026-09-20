using UnityEngine;

public class SavedGameLoader : MonoBehaviour
{
    void Start()
    {
        SaveData data = GameSession.PendingSave;

        if (data == null)
            return;

        PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>();

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint.Id != data.checkpointId)
                continue;

            player.transform.position = checkpoint.transform.position;
            player.SetRespawnPoint(checkpoint.transform);
            break;
        }

        player.RestoreHealth(data.health);
        GameSession.PendingSave = null;
    }
}
