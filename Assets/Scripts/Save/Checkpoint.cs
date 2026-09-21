using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointId;
    [SerializeField] private Transform respawnPoint;

    public string Id => checkpointId;
    public Transform RespawnPoint => respawnPoint != null ? respawnPoint : transform;

    public void Configure(string id, Transform newRespawnPoint)
    {
        checkpointId = id;
        respawnPoint = newRespawnPoint;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerHealth>() == null)
            return;

        CheckpointManager manager = FindAnyObjectByType<CheckpointManager>();

        if (manager == null)
            return;

        manager.ActivateCheckpoint(this);
    }
}
