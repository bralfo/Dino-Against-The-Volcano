using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
                playerHealth.Respawned += SnapToPlayer;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.Respawned -= SnapToPlayer;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        if (player.position.x <= transform.position.x)
            return;

        SetCameraX(player.position.x);
    }

    private void SnapToPlayer()
    {
        if (player != null)
            SetCameraX(player.position.x);
    }

    private void SetCameraX(float positionX)
    {
        float targetX = Mathf.Clamp(positionX, minX, maxX);

        transform.position = new Vector3(targetX, 0f, transform.position.z);
    }
}
