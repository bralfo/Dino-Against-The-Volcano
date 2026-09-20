using System;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class CoinWallet : MonoBehaviour
{
    [SerializeField, Min(1)] private int coinsPerHeart = 10;
    [SerializeField, Min(0)] private int coinsLostOnDamage = 5;

    public int CurrentCoins { get; private set; }
}
