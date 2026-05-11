using UnityEngine;

namespace CoreGameplay.Pickups
{
    public class SpeedBoostPickup : Pickup
    {
        [Header("Speed Boost")]
        [SerializeField] private float boostMultiplier = 1.5f;
        [SerializeField] private float boostDuration = 3f;

        public override void ApplyEffect(GameObject player)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.ApplySpeedMultiplier(boostMultiplier, boostDuration);
        }
    }
}