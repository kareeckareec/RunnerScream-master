using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Unity.Services.LevelPlay;
using System;

namespace Meta
{
    public class AdManager : MonoBehaviour
    {
        [Header("LevelPlay Configuration")]
        [SerializeField] private string StringAppId = "24583f3d5";
        [SerializeField] private string StringAdUnitId = "1mnkp5jrqnmrrjfd";
        [SerializeField] private string StringPlacementId = "F00807BEB7457EF";
        [SerializeField] private string StringRewardId = "27FE990FE2FC3E4E";
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private LevelPlayRewardedAd _rewardedAd;
        public static AdManager instance;
        
        // События для внешних подписчиков
        private static Action _onRewardSuccess;
        private static Action<string> _onRewardFailed;
        
        private bool isLoadingAd = false;
        private bool isShowingAd = false;
        private bool isInitialized = false;
        private bool isRewarded = false;
        
        public static AdManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AdManager>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject("AdManager");
                        instance = obj.AddComponent<AdManager>();
                        DontDestroyOnLoad(obj);
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            InitializeLevelPlay();
        }
        
        private void InitializeLevelPlay()
        {
            if (isInitialized) return;
            
            try
            {
                // Подписываемся на события инициализации ДО вызова Init
                LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
                LevelPlay.OnInitFailed += OnLevelPlayInitFailed;
                
                Log("Инициализация LevelPlay...");
                LevelPlay.Init(StringAppId);
            }
            catch (Exception e)
            {
                LogError($"Ошибка инициализации LevelPlay: {e.Message}");
            }
        }
        
        private void OnLevelPlayInitSuccess(LevelPlayConfiguration config)
        {
            Log("✅ LevelPlay инициализирован успешно!");
            isInitialized = true;
            
            // Создаём рекламный объект после инициализации
            CreateRewardedAd();
        }
        
        private void OnLevelPlayInitFailed(LevelPlayInitError error)
        {
            LogError($"❌ LevelPlay инициализация не удалась: {error.ErrorCode} - {error.ErrorMessage}");
            isInitialized = false;
        }
        
        private void CreateRewardedAd()
        {
            if (_rewardedAd != null)
            {
                Log("RewardedAd уже создан");
                return;
            }
            
            try
            {
                Log($"Создание RewardedAd с ID: {StringAdUnitId}");
                _rewardedAd = new LevelPlayRewardedAd(StringAdUnitId);
                RegisterRewardedAdEvents();
                LoadRewardedAd();
            }
            catch (Exception e)
            {
                LogError($"Ошибка создания RewardedAd: {e.Message}");
            }
        }
        
        private void RegisterRewardedAdEvents()
        {
            if (_rewardedAd == null) return;
            
            _rewardedAd.OnAdLoaded += OnAdLoaded;
            _rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
            _rewardedAd.OnAdDisplayed += OnAdDisplayed;
            _rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
            _rewardedAd.OnAdRewarded += OnAdRewarded;
            _rewardedAd.OnAdClosed += OnAdClosed;
            _rewardedAd.OnAdClicked += OnAdClicked;
            
            Log("События рекламы зарегистрированы");
        }
        
        /// <summary>
        /// Показать вознаграждаемую рекламу
        /// </summary>
        public void ShowRewardedAd(Action onSuccess = null, Action<string> onFailed = null)
        {
            _onRewardSuccess = onSuccess;
            _onRewardFailed = onFailed;
            isRewarded = false;
            
            // Проверяем инициализацию
            if (!isInitialized)
            {
                LogError("LevelPlay не инициализирован");
                _onRewardFailed?.Invoke("Реклама не инициализирована");
                ClearCallbacks();
                return;
            }
            
            // Проверяем создание рекламы
            if (_rewardedAd == null)
            {
                LogError("RewardedAd не создан, пробуем создать");
                CreateRewardedAd();
                _onRewardFailed?.Invoke("Реклама не создана");
                ClearCallbacks();
                return;
            }
            
            // Проверяем готовность рекламы
            if (_rewardedAd.IsAdReady())
            {
                Log("Реклама готова, показываем");
                ShowAd();
            }
            else
            {
                Log("Реклама не готова, загружаем...");
                LoadRewardedAd();
                
                // Ждём загрузки с таймаутом
                Invoke(nameof(OnLoadTimeout), 10f);
            }
        }
        
        private void OnLoadTimeout()
        {
            if (!isShowingAd && _onRewardSuccess != null)
            {
                LogError("Таймаут загрузки рекламы");
                _onRewardFailed?.Invoke("Таймаут загрузки рекламы");
                ClearCallbacks();
            }
        }
        
        private void LoadRewardedAd()
        {
            if (isLoadingAd)
            {
                Log("Реклама уже загружается");
                return;
            }
            
            if (_rewardedAd == null)
            {
                LogError("Нельзя загрузить рекламу: _rewardedAd = null");
                return;
            }
            
            if (_rewardedAd.IsAdReady())
            {
                Log("Реклама уже готова к показу");
                return;
            }
            
            isLoadingAd = true;
            Log("Загрузка рекламы...");
            _rewardedAd.LoadAd();
        }
        
        private void ShowAd()
        {
            if (_rewardedAd == null)
            {
                LogError("Нельзя показать рекламу: _rewardedAd = null");
                _onRewardFailed?.Invoke("Реклама не создана");
                ClearCallbacks();
                return;
            }
            
            if (!_rewardedAd.IsAdReady())
            {
                LogError("Нельзя показать рекламу: IsAdReady = false");
                _onRewardFailed?.Invoke("Реклама не готова");
                ClearCallbacks();
                return;
            }
            
            if (isShowingAd)
            {
                Log("Реклама уже показывается");
                return;
            }
            
            isShowingAd = true;
            Log("Показ рекламы...");
            _rewardedAd.ShowAd();
        }
        
        private void OnAdLoaded(LevelPlayAdInfo adInfo)
        {
            isLoadingAd = false;
            Log($"✅ Реклама загружена! AdUnitId: {adInfo.AdUnitId}");
            
            // Если есть ожидающий запрос на показ, показываем сразу
            if (!isShowingAd && _onRewardSuccess != null)
            {
                Log("Загрузка завершена, показываем рекламу");
                ShowAd();
            }
        }
        
        private void OnAdLoadFailed(LevelPlayAdError error)
        {
            isLoadingAd = false;
            LogError($"❌ Ошибка загрузки рекламы: Code={error.ErrorCode}, Message={error.ErrorMessage}");
            
            // Пробуем загрузить снова через 5 секунд
            Invoke(nameof(RetryLoadAd), 5f);
            
            _onRewardFailed?.Invoke($"Ошибка загрузки: {error.ErrorCode}");
            ClearCallbacks();
        }
        
        private void RetryLoadAd()
        {
            Log("Повторная попытка загрузки рекламы...");
            LoadRewardedAd();
        }
        
        private void OnAdDisplayed(LevelPlayAdInfo adInfo)
        {
            Log("📺 Реклама показана на экране");
            //ReportAdActivity(AdActivity.Opened);
        }
        
        private void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
        {
            isShowingAd = false;
            isLoadingAd = false;
            LogError($"❌ Ошибка показа рекламы: Code={error.ErrorCode}, Message={error.ErrorMessage}");
            
            _onRewardFailed?.Invoke($"Ошибка показа: {error.ErrorCode}");
            ClearCallbacks();
            
            // Пробуем загрузить снова
            Invoke(nameof(RetryLoadAd), 3f);
        }
        
        private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
        {
            isRewarded = true;
            Log($"🎉 Игрок получил награду! Amount: {reward.Amount}");
            //ReportAdActivity(AdActivity.End);
            
            // Выдаём награду через PlayFab
            RewardPlayer();
        }
        
        private void OnAdClicked(LevelPlayAdInfo adInfo)
        {
            Log("👆 Пользователь кликнул по рекламе");
        }
        
        private void OnAdClosed(LevelPlayAdInfo adInfo)
        {
            isShowingAd = false;
            isLoadingAd = false;
            Log("🔒 Реклама закрыта");
            //ReportAdActivity(AdActivity.Closed);
            
            // Проверяем, была ли получена награда
            if (isRewarded)
            {
                Log("Награда уже выдана через RewardPlayer");
            }
            else
            {
                LogWarning("Реклама закрыта без получения награды");
                _onRewardFailed?.Invoke("Реклама закрыта до получения награды");
                ClearCallbacks();
            }
            
            // Загружаем следующую рекламу
            LoadRewardedAd();
        }
        
        private void ReportAdActivity(AdActivity activity)
        {
            var request = new ReportAdActivityRequest
            {
                PlacementId = StringPlacementId,
                RewardId = StringRewardId,
                Activity = activity
            };
            
            PlayFabClientAPI.ReportAdActivity(request,
                result => Log($"Activity {activity} отправлена в PlayFab"),
                error => LogError($"Ошибка отчёта PlayFab: {error.GenerateErrorReport()}")
            );
        }
        
        private void RewardPlayer()
        {
            Log("Выдача награды за рекламу...");
            int rewardAmount = 10; // сколько кристаллов давать за рекламу
            
            Currency.PlayFabCurrency.AddCurrency("HC", rewardAmount,
                onSuccess: (newBalance) =>
                {
                    Log($"✅ Награда выдана: +{rewardAmount} кристаллов. Баланс: {newBalance}");
                    _onRewardSuccess?.Invoke();
                    ClearCallbacks();
                },
                onError: (error) =>
                {
                    LogError($"Ошибка выдачи награды: {error}");
                    _onRewardFailed?.Invoke("Ошибка выдачи награды");
                    ClearCallbacks();
                }
            );
        }
        
        /// <summary>
        /// Проверить, готова ли реклама к показу
        /// </summary>
        public bool IsAdReady()
        {
            return isInitialized && _rewardedAd != null && _rewardedAd.IsAdReady();
        }
        
        private void ClearCallbacks()
        {
            _onRewardSuccess = null;
            _onRewardFailed = null;
            CancelInvoke(nameof(OnLoadTimeout));
        }
        
        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[AdManager] {message}");
        }
        
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AdManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[AdManager] {message}");
        }
        
        private void OnDestroy()
        {
            if (_rewardedAd != null)
            {
                _rewardedAd.OnAdLoaded -= OnAdLoaded;
                _rewardedAd.OnAdLoadFailed -= OnAdLoadFailed;
                _rewardedAd.OnAdDisplayed -= OnAdDisplayed;
                _rewardedAd.OnAdDisplayFailed -= OnAdDisplayFailed;
                _rewardedAd.OnAdRewarded -= OnAdRewarded;
                _rewardedAd.OnAdClosed -= OnAdClosed;
                _rewardedAd.OnAdClicked -= OnAdClicked;
                _rewardedAd.Dispose();
            }
            
            LevelPlay.OnInitSuccess -= OnLevelPlayInitSuccess;
            LevelPlay.OnInitFailed -= OnLevelPlayInitFailed;
        }
    }
}