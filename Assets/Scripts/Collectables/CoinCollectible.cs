using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0f, 1f)] private float collectSoundVolume = 0.2f;

    [SerializeField, Min(1)] private int value = 1;

    private bool wasCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasCollected)
            return;

        CoinWallet wallet = other.GetComponentInParent<CoinWallet>();

        if (wallet == null)
            return;

        wasCollected = true;

        AudioSource playerAudioSource = wallet.GetComponent<AudioSource>();

        if (playerAudioSource != null && collectSound != null)
            playerAudioSource.PlayOneShot(collectSound, collectSoundVolume);

        wallet.AddCoins(value);
        Destroy(gameObject);
    }
}
