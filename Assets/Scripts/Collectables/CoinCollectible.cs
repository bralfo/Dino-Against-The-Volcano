using UnityEngine;

public class CoinCollectible : MonoBehaviour
{
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
        wallet.AddCoins(value);
        Destroy(gameObject);
    }
}
