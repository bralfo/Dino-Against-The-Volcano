using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private CoinWallet coinWallet;
    [SerializeField] private TMP_Text coinCountText;

    private void OnEnable()
    {
        if (coinWallet != null)
            coinWallet.CoinsChanged += UpdateCoinText;
    }

    private void Start()
    {
        if (coinWallet != null)
            UpdateCoinText(coinWallet.CurrentCoins);
    }

    private void OnDisable()
    {
        if (coinWallet != null)
            coinWallet.CoinsChanged -= UpdateCoinText;
    }

    private void UpdateCoinText(int amount)
    {
        if (coinCountText != null)
            coinCountText.text = amount.ToString();
    }
}
