// MainMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using CoreGameplay.Audio;

namespace Meta
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject settingsPanel;
        public GameObject garagePanel;
        public GameObject leaderboardPanel;
        public LeaderboardUI leaderboardUI;
        
        [Header("Text")]
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI softMoneyText;
        public TextMeshProUGUI hardMoneyText;
        public TextMeshProUGUI conversionStatusText;
        
        [Header("Buttons")]
        public Button playButton;
        public Button garageButton;
        public Button leaderboardButton;
        public Button settingsButton;
        public Button authorButton;
        public Button buyHardCurrencyButton;

        [Header("Settings Panel")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle musicMuteToggle;
        [SerializeField] private Toggle sfxMuteToggle;
        [SerializeField] private Button closeSettingsButton;
        
        void Start()
        {
            // Назначаем обработчики кнопок
            playButton.onClick.AddListener(StartGame);
            garageButton.onClick.AddListener(OpenGarage);
            leaderboardButton.onClick.AddListener(OpenLeaderboard);
            settingsButton.onClick.AddListener(OpenSettings);
            authorButton.onClick.AddListener(ShowAuthorInfo);
            buyHardCurrencyButton.onClick.AddListener(BuyHardCurrency);
            
            // Обновляем отображаемые данные
            UpdateUI();
            
            // Загружаем данные игрока
            LoadPlayerData();

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
            if (musicMuteToggle != null)
            {
                musicMuteToggle.isOn = AudioManager.Instance.IsMusicMuted();
                musicMuteToggle.onValueChanged.AddListener(OnMusicMuteToggled);
            }
            if (sfxMuteToggle != null)
            {
                sfxMuteToggle.isOn = AudioManager.Instance.IsSFXMuted();
                sfxMuteToggle.onValueChanged.AddListener(OnSFXMuteToggled);
            }
            if (closeSettingsButton != null)
                closeSettingsButton.onClick.AddListener(BackToMain);
        }
        
        void LoadPlayerData()
        {
            // Загружаем данные с PlayFab
            DataBase.UserData.GetData(() => {
                UpdateUI();        
                if (leaderboardUI != null)
                    leaderboardUI.UpdatePlayerInfo();
            });
        }
        
        void UpdateUI()
        {
            if (playerNameText != null)
                playerNameText.text = DataBase.UserData.NickName;
            
            if (softMoneyText != null)
                softMoneyText.text = DataBase.UserData.SoftMoney.ToString();
            
            if (hardMoneyText != null)
                hardMoneyText.text = DataBase.UserData.HardMoney.ToString();

            BackToMain();
        }
        
        void StartGame()
        {
            DataBase.UserData.UpdateData();
            SceneManager.LoadScene(2);
        }
        
        public void OpenGarage()
        {
            Debug.Log("[MainMenuUI] Открывается гараж...");
            mainPanel.SetActive(false);
            garagePanel.SetActive(true);
        }
        
        void OpenLeaderboard()
        {
            mainPanel.SetActive(false);
            leaderboardPanel.SetActive(true);

            if (leaderboardUI != null)
                leaderboardUI.RefreshLeaderboard();
        }
        
        void ShowAuthorInfo()
        {
            Debug.Log("Автор: Имя Фамилия Группа");
        }
        
        public void BackToMain()
        {
            mainPanel.SetActive(true);
            settingsPanel.SetActive(false);
            garagePanel.SetActive(false);
            leaderboardPanel.SetActive(false);
        }

        private void BuyHardCurrency()
        {
            int amount = MenuCurrencyUtils.Instance != null ? 
                         MenuCurrencyUtils.Instance.defaultHardAmount : 10;
            
            MenuCurrencyUtils.Instance?.ConvertSoftToHard(amount, (success) =>
            {
                if (success)
                {
                    ShowConversionMessage($"Куплено {amount} кристаллов!");
                }
                else
                {
                    ShowConversionMessage($"Недостаточно монет для покупки {amount} кристаллов.");
                }
            });
        }

        private void ShowConversionMessage(string message)
        {
            if (conversionStatusText != null)
            {
                conversionStatusText.text = message;
                // Скрыть через 2 секунды
                Invoke(nameof(ClearConversionMessage), 2f);
            }
            else
            {
                Debug.Log(message);
            }
        }

        private void ClearConversionMessage()
        {
            if (conversionStatusText != null)
                conversionStatusText.text = "";
        }

        private void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(StartGame);
            
            if (garageButton != null)
                garageButton.onClick.RemoveListener(OpenGarage);
            
            if (leaderboardButton != null)
                leaderboardButton.onClick.RemoveListener(OpenLeaderboard);
            
            if (settingsButton != null)
                settingsButton.onClick.RemoveListener(OpenSettings);
            
            if (authorButton != null)
                authorButton.onClick.RemoveListener(ShowAuthorInfo);

            if(buyHardCurrencyButton != null)
                buyHardCurrencyButton.onClick.RemoveListener(BuyHardCurrency);
        }

        private void OnMusicVolumeChanged(float volume) => AudioManager.Instance.SetMusicVolume(volume);
        private void OnSFXVolumeChanged(float volume) => AudioManager.Instance.SetSFXVolume(volume);
        private void OnMusicMuteToggled(bool isMuted) => AudioManager.Instance.ToggleMusicMute(isMuted);
        private void OnSFXMuteToggled(bool isMuted) => AudioManager.Instance.ToggleSFXMute(isMuted);

        // Обновите метод OpenSettings() – он должен показывать панель и обновлять текущие значения
        public void OpenSettings()
        {
            mainPanel.SetActive(false);
            settingsPanel.SetActive(true);
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                // Обновить UI текущими значениями из AudioManager
                if (musicVolumeSlider != null) musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
                if (sfxVolumeSlider != null) sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
                if (musicMuteToggle != null) musicMuteToggle.isOn = AudioManager.Instance.IsMusicMuted();
                if (sfxMuteToggle != null) sfxMuteToggle.isOn = AudioManager.Instance.IsSFXMuted();
            }
        }
    }
}