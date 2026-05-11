using UnityEngine;
using CoreGameplay.Audio; // добавлено

namespace CoreGameplay.Pickups
{
    public abstract class Pickup : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] protected GameObject visualEffectPrefab;
        [SerializeField] protected AudioClip pickupSound;

        protected bool isCollected = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;
            if (other.CompareTag("Player"))
            {
                isCollected = true;
                ApplyEffect(other.gameObject);
                PlayPickupEffects();
                Destroy(gameObject);
            }
        }

        public abstract void ApplyEffect(GameObject player);

        protected virtual void PlayPickupEffects()
        {
            if (visualEffectPrefab != null)
                Instantiate(visualEffectPrefab, transform.position, Quaternion.identity);
            
            // Используем AudioManager для воспроизведения звука с учётом настроек громкости
            AudioManager.Instance?.PlayPickupActivated();
        }
    }
}