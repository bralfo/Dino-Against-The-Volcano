using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] public Transform player;
    [SerializeField] float minX, maxX; 
    void Awake()
    {
        player = GameObject.FindWithTag("Player").transform; 
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player.position.x >= transform.position.x)
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, minX, maxX), 0, transform.position.z);
    }
}
