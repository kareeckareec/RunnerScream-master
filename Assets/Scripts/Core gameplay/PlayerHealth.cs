using System.Collections;
using UnityEngine;
using CoreGameplay;
using CoreGameplay.UI;
using CoreGameplay.Pickups;
using TMPro;
using CoreGameplay.Audio; // добавлено

namespace CoreGameplay
{
    public class PlayerHealth : MonoBehaviour
    {
        private static int maxHealth = 3;
        [SerializeField] private float immunityDuration = 5f;
        [Header("Visual Feedback")] 
        [SerializeField] private Renderer playerRenderer;
        [SerializeField] private TextMeshProUGUI newRecordMessage;
        [SerializeField] private Color hitColor = Color.red;
        [SerializeField] private float blinkSpeed = 0.15f;
        [SerializeField] private ObstacleSpawner obstacleSpawner;
        private static int currentHealth;
        private bool isImmune = false;
        private Color originalColor;
        private Coroutine immunityRoutine;
        public static int DeathsCount;
        public UIController uiControllerObj;

        void Start()
        {
            DeathsCount = 0;
            currentHealth = maxHealth;
            if (playerRenderer == null)
                playerRenderer = GetComponentInChildren<Renderer>();
            if (playerRenderer != null)
                originalColor = playerRenderer.material.color;

            UIController.SetHealth(currentHealth);
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Столкновение с: {other.name} ({other.tag})");
            if (other.CompareTag("Enemy"))
            {
                TakeDamage();
                var enemyReaction = other.GetComponent<EnemyReaction>();
                if (enemyReaction != null)
                {
                    enemyReaction.ReactToHit(Vector3.right, 1);
                }
            }
            else if (other.CompareTag("Pickup"))
            {
                other.gameObject.GetComponent<Pickup>().ApplyEffect(gameObject);
            }
        }

        void TakeDamage()
        {
            if (isImmune) return;

            // Воспроизвести звук столкновения
            AudioManager.Instance?.PlayCollision(transform.position);

            currentHealth--;
            Debug.Log($"Current health: {currentHealth}");

            if (immunityRoutine != null)
                StopCoroutine(immunityRoutine);
            immunityRoutine = StartCoroutine(ImmunityFlash());

            UIController.SetHealth(currentHealth);

            if (currentHealth <= 0)
            {
                int coins = AwardSoftCurrencyForRun();
                int finalScore = obstacleSpawner != null ? Mathf.FloorToInt(obstacleSpawner.Score) : 0;
                bool isNewRecord = LeaderboardManager.Instance.TryAddScoreAndCheckRecord(finalScore);
                uiControllerObj.OnGameOver(finalScore, isNewRecord, coins);
                DeathsCount++;
                return;
            }
        }

        IEnumerator ImmunityFlash()
        {
            isImmune = true;
            float elapsed = 0f;
            while (elapsed < immunityDuration)
            {
                elapsed += Time.deltaTime;
                if (playerRenderer != null)
                {
                    playerRenderer.material.color =
                        (Mathf.FloorToInt(elapsed / blinkSpeed) % 2 == 0) ? hitColor : originalColor;
                }
                yield return null;
            }
            if (playerRenderer != null)
                playerRenderer.material.color = originalColor;
            isImmune = false;
        }

        public static void Revive()
        {
            currentHealth = maxHealth;
            UIController.SetHealth(currentHealth);
        }

        private int AwardSoftCurrencyForRun()
        {
            if (obstacleSpawner == null) return 0;
            int score = Mathf.FloorToInt(obstacleSpawner.Score);
            int rewardCoins = score / 10;
            if (rewardCoins <= 0) return 0;
            Currency.PlayFabCurrency.AddCurrency("SC", rewardCoins,
                onSuccess: (newBalance) => Debug.Log($"Начислено {rewardCoins} монет. Новый баланс: {newBalance}"),
                onError: (error) => Debug.LogError($"Ошибка начисления валюты: {error}")
            );
            return rewardCoins;
        }
    }
}