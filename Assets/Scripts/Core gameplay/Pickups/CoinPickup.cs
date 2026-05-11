using Currency;
using UnityEngine;

namespace CoreGameplay.Pickups
{
    public class CoinPickup : Pickup
    {
        [Header("Coin")]
        [SerializeField] private int coinAmount = 10;

        public override void ApplyEffect(GameObject player)
        {
            PlayFabCurrency.AddCurrency("SC", coinAmount);
            Debug.Log($"+{coinAmount} coins");
        }
    }
}