using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CoreGameplay;
using CoreGameplay.Spawner;
using CoreGameplay.Audio;
using DataBase;
using System.Collections;
using Meta;
using TMPro;

namespace CoreGameplay.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private ObstacleSpawner spawnerObstacles;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI coinsEarnedText;
        [SerializeField] private TextMeshProUGUI recordMessageText;
        public static GameObject PauseButton => Instance.pauseButton;

        [SerializeField] private RectTransform healthBar;
        [SerializeField] private float healthBarElementSize = 20f;

        private static UIController Instance { get; set; }

        [Header("Revive Settings")]
        [SerializeField] private Text costText;
        [SerializeField] private Text moneyText;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button reviveByAdButton;  // новая кнопка

        [Header("Final Results Panel")]
        [SerializeField] private GameObject finalGameOverPanel;
        [SerializeField] private GameObject newRecordText;
        [SerializeField] private GameObject lastRecordText;
        [SerializeField] private Text lastRecordAmount;

        [Header("Restart Buttons")]
        [SerializeField] private Button restartButtonInGameOver;     // кнопка в меню поражения
        [SerializeField] private Button restartButtonInFinal;        // кнопка в финальном меню
        [SerializeField] private Button garageButton;               // кнопка выхода в гараж/меню

        [Header("Pause Settings")]
        [SerializeField] private Slider pauseMusicSlider;
        [SerializeField] private Slider pauseSFXSlider;
        [SerializeField] private Toggle pauseMusicMuteToggle;
        [SerializeField] private Toggle pauseSFXMuteToggle;

        private float cost;
        private float currentScore;
        private bool isNewRecord = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Назначаем обработчики кнопок
            if (restartButtonInGameOver != null)
                restartButtonInGameOver.onClick.AddListener(ShowFinalResults);
            
            if (restartButtonInFinal != null)
                restartButtonInFinal.onClick.AddListener(RestartGame);
            
            if (garageButton != null)
                garageButton.onClick.AddListener(LoadGarage);
            
            if (reviveByAdButton != null)
                reviveByAdButton.onClick.AddListener(Continue);

            if (pauseMusicSlider != null)
            {
                pauseMusicSlider.value = AudioManager.Instance.GetMusicVolume();
                pauseMusicSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetMusicVolume(v));
            }
            if (pauseSFXSlider != null)
            {
                pauseSFXSlider.value = AudioManager.Instance.GetSFXVolume();
                pauseSFXSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetSFXVolume(v));
            }
            if (pauseMusicMuteToggle != null)
            {
                pauseMusicMuteToggle.isOn = AudioManager.Instance.IsMusicMuted();
                pauseMusicMuteToggle.onValueChanged.AddListener(v => AudioManager.Instance.ToggleMusicMute(v));
            }
            if (pauseSFXMuteToggle != null)
            {
                pauseSFXMuteToggle.isOn = AudioManager.Instance.IsSFXMuted();
                pauseSFXMuteToggle.onValueChanged.AddListener(v => AudioManager.Instance.ToggleSFXMute(v));
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static void SetHealth(int health)
        {
            var size = Instance.healthBar.sizeDelta;
            size.x = Instance.healthBarElementSize * health;
            Instance.healthBar.sizeDelta = size;
        }

        public void OnGameOver(int finalScore, bool isNewRecord, int coinsEarned)
        {
            Time.timeScale = 0f;
            pauseButton.SetActive(false);
            gameOverPanel.SetActive(true);
            scoreText.text = $"Ваш счёт: {finalScore}";
            coinsEarnedText.text = $"Coins: {coinsEarned}";

            currentScore = spawnerObstacles.Score;
            cost = 1 << PlayerHealth.DeathsCount;  // 2^DeathsCount

            costText.text = cost.ToString();
            
            int hardMoneyBalance = UserData.HardMoney;
            moneyText.text = hardMoneyBalance.ToString();
            
            continueButton.interactable = hardMoneyBalance >= cost;
            
            if (isNewRecord)
            {
                recordMessageText.text = "НОВЫЙ РЕКОРД!";
                recordMessageText.color = Color.yellow;
            }
            else
            {
                recordMessageText.text = $"Личный рекорд: {DataBase.UserData.Record}";
                recordMessageText.color = Color.white;
            }

            if (reviveByAdButton != null)
                reviveByAdButton.interactable = Application.internetReachability != NetworkReachability.NotReachable;
        }

        public void Continue()
        {
            int costInt = Mathf.FloorToInt(cost);
            
            if (UserData.HardMoney >= costInt)
            {
                UserData.HardMoney -= costInt;
                UserData.UpdateData();  // Сохраняем в PlayFab
                
                PlayerHealth.Revive();
                Time.timeScale = 1f;
                pauseButton.SetActive(true);
                gameOverPanel.SetActive(false);
            }
        }

        public void ContinueByAd()
        {
            if (AdManager.Instance != null)
            {
                // Блокируем кнопку на время показа рекламы
                if (reviveByAdButton != null)
                    reviveByAdButton.interactable = false;
                
                AdManager.Instance.ShowRewardedAd(
                    onSuccess: () => 
                    {
                        // Успешный просмотр рекламы
                        PlayerHealth.Revive();
                        Time.timeScale = 1f;
                        pauseButton.SetActive(true);
                        gameOverPanel.SetActive(false);
                        Debug.Log("Воскрешение за рекламу успешно!");
                        
                        // Разблокируем кнопку (на случай, если понадобится снова)
                        if (reviveByAdButton != null)
                            reviveByAdButton.interactable = true;
                    },
                    onFailed: (error) => 
                    {
                        // Ошибка или закрытие без награды
                        Debug.LogWarning($"Воскрешение за рекламу не удалось: {error}");
                        
                        // Разблокируем кнопку, чтобы игрок мог попробовать снова
                        if (reviveByAdButton != null)
                            reviveByAdButton.interactable = true;
                        
                        // Можно показать сообщение игроку
                    }
                );
            }
            else
            {
                Debug.LogError("AdManager.Instance не найден!");
            }
        }

        public void ShowFinalResults()
        {
            gameOverPanel.SetActive(false);
            finalGameOverPanel.SetActive(true);
            
            int intScore = Mathf.FloorToInt(currentScore);

            // Проверка рекорда через UserData
            int currentRecord = UserData.Record;
            
            if (currentRecord < intScore)
            {
                // Новый рекорд!
                UserData.Record = intScore;
                UserData.UpdateData();
                isNewRecord = true;
                newRecordText.SetActive(true);
                lastRecordText.SetActive(false);
            }
            else
            {
                // Рекорд не побит
                isNewRecord = false;
                newRecordText.SetActive(false);
                lastRecordText.SetActive(true);
                lastRecordAmount.text = currentRecord.ToString();
            }
        }

        // Вызывается после показа результатов, когда игрок нажимает "Продолжить" или "Заново"
        public void OnResultsAcknowledged()
        {
            if (isNewRecord)
            {
                // Отправляем рекорд в таблицу лидеров
                LeaderboardManager.Instance.AddScore(Mathf.FloorToInt(currentScore), UserData.NickName);
            }
        }

        public void LoadGarage()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(1);
        }

        public void SetPaused(bool value)
        {
            pausePanel.SetActive(value);
            Time.timeScale = value ? 0f : 1f;
            
            if (value) // при открытии паузы обновляем слайдеры
            {
                if (pauseMusicSlider != null) pauseMusicSlider.value = AudioManager.Instance.GetMusicVolume();
                if (pauseSFXSlider != null) pauseSFXSlider.value = AudioManager.Instance.GetSFXVolume();
                if (pauseMusicMuteToggle != null) pauseMusicMuteToggle.isOn = AudioManager.Instance.IsMusicMuted();
                if (pauseSFXMuteToggle != null) pauseSFXMuteToggle.isOn = AudioManager.Instance.IsSFXMuted();
            }
        }

        private void OnDestroy()
        {
            if (restartButtonInGameOver != null)
                restartButtonInGameOver.onClick.RemoveListener(ShowFinalResults);
            
            if (restartButtonInFinal != null)
                restartButtonInFinal.onClick.RemoveListener(RestartGame);
            
            if (garageButton != null)
                garageButton.onClick.RemoveListener(LoadGarage);
            
            if (reviveByAdButton != null)
                reviveByAdButton.onClick.RemoveListener(ContinueByAd);
        }
    }
}