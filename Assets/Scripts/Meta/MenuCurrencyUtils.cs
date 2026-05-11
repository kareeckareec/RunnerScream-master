using UnityEngine;
using Currency;
using Unity.Services.LevelPlay;
using Meta;
using TMPro;
using System;

namespace Meta
{
    /// <summary>
    /// Утилита для работы с валютой в главном меню
    /// Поддерживает UI-обновление, анимации, проверки и уведомления
    /// </summary>
    public class MenuCurrencyUtils : MonoBehaviour
    {
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI softCurrencyText;
        [SerializeField] private TextMeshProUGUI hardCurrencyText;

        [Header("Currency Conversion")]
        [SerializeField] private int softToHardRate = 10; // 10 SC = 1 HC
        [SerializeField] public int defaultHardAmount = 10; // количество HC для быстрой покупки
        
        // События для других систем
        public static event Action<int> OnSoftCurrencyChanged;
        public static event Action<int> OnHardCurrencyChanged;
        
        private int currentSoftCurrency;
        private int currentHardCurrency;
        private bool isUpdating = false;
        
        // Singleton для лёгкого доступа
        public static MenuCurrencyUtils Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            // Загружаем текущий баланс
            RefreshCurrencyDisplay();
        }
        
        private void OnEnable()
        {
            // Подписываемся на события обновления валюты
            PlayFabCurrency.OnCurrencyUpdated += OnCurrencyUpdated;
        }
        
        private void OnDisable()
        {
            // Отписываемся при отключении
            PlayFabCurrency.OnCurrencyUpdated -= OnCurrencyUpdated;
        }
        
        /// <summary>
        /// Обновить отображение валюты
        /// </summary>
        public void RefreshCurrencyDisplay()
        {
            if (isUpdating) return;
            
            isUpdating = true;
            
            // Получаем текущий баланс с сервера
            PlayFabCurrency.GetCurrencyBalance(
                onSuccess: (softBalance, hardBalance) =>
                {
                    currentSoftCurrency = softBalance;
                    currentHardCurrency = hardBalance;
                    
                    UpdateUIText();
                    isUpdating = false;
                },
                onError: (error) =>
                {
                    Debug.LogError($"Ошибка загрузки валюты: {error}");
                    isUpdating = false;
                }
            );
        }
        
        /// <summary>
        /// Обновить текстовые поля UI
        /// </summary>
        private void UpdateUIText()
        {
            if (softCurrencyText != null)
                softCurrencyText.text = currentSoftCurrency.ToString();
            
            if (hardCurrencyText != null)
                hardCurrencyText.text = currentHardCurrency.ToString();
        }
        
        /// <summary>
        /// Добавить мягкую валюту
        /// </summary>
        public void AddSoftCurrency(int amountToAdd)
        {
            if (amountToAdd <= 0)
            {
                Debug.LogWarning("Нельзя добавить отрицательное или нулевое количество валюты");
                return;
            }
            
            PlayFabCurrency.AddCurrency("SC", amountToAdd,
                onSuccess: (newBalance) =>
                {
                    currentSoftCurrency = newBalance;
                    UpdateUIText();
                    OnSoftCurrencyChanged?.Invoke(newBalance);
                    
                    Debug.Log($"Добавлено {amountToAdd} мягкой валюты. Новый баланс: {newBalance}");
                },
                onError: (error) =>
                {
                    Debug.LogError($"Ошибка добавления мягкой валюты: {error}");
                }
            );
        }
        
        /// <summary>
        /// Добавить жёсткую валюту (через рекламу)
        /// </summary>
        public void AddHardCurrency(int amountToAdd)
        {
            if (amountToAdd <= 0)
            {
                Debug.LogWarning("Нельзя добавить отрицательное или нулевое количество валюты");
                return;
            }
            
            AdManager adManager = GetAdManager();
            if (adManager != null)
            {
                adManager.ShowRewardedAd(
                    onSuccess: () => 
                    {
                        // Реклама успешно просмотрена — выдаём награду
                        PlayFabCurrency.AddCurrency("HC", amountToAdd,
                            onSuccess: (newBalance) =>
                            {
                                currentHardCurrency = newBalance;
                                UpdateUIText();
                                OnHardCurrencyChanged?.Invoke(newBalance);
                                
                                Debug.Log($"Добавлено {amountToAdd} жёсткой валюты за рекламу. Новый баланс: {newBalance}");
                            },
                            onError: (error) =>
                            {
                                Debug.LogError($"Ошибка выдачи награды: {error}");
                            }
                        );
                    },
                    onFailed: (error) => 
                    {
                        Debug.Log($"Просмотр рекламы не завершён: {error}");
                    }
                );
            }
            else
            {
                Debug.LogError("AdManager не найден!");
            }
        }
        
        /// <summary>
        /// Списать мягкую валюту с проверкой достаточности
        /// </summary>
        /// <returns>Успешно ли списание</returns>
        public bool SubtractSoftCurrency(int amountToSubtract)
        {
            if (amountToSubtract <= 0)
            {
                Debug.LogWarning("Нельзя списать отрицательное или нулевое количество валюты");
                return false;
            }
            
            if (currentSoftCurrency < amountToSubtract)
            {
                return false;
            }
            
            PlayFabCurrency.SafeSubtractCurrency("SC", amountToSubtract,
                onSuccess: (newBalance) =>
                {
                    currentSoftCurrency = newBalance;
                    UpdateUIText();
                    OnSoftCurrencyChanged?.Invoke(newBalance);
                    
                    Debug.Log($"Списано {amountToSubtract} мягкой валюты. Новый баланс: {newBalance}");
                },
                onError: (error) =>
                {
                    Debug.LogError($"Ошибка списания мягкой валюты: {error}");
                }
            );
            
            return true;
        }
        
        /// <summary>
        /// Списать жёсткую валюту с проверкой достаточности
        /// </summary>
        /// <returns>Успешно ли списание</returns>
        public bool SubtractHardCurrency(int amountToSubtract)
        {
            if (amountToSubtract <= 0)
            {
                Debug.LogWarning("Нельзя списать отрицательное или нулевое количество валюты");
                return false;
            }
            
            if (currentHardCurrency < amountToSubtract)
            {
                return false;
            }
            
            PlayFabCurrency.SafeSubtractCurrency("HC", amountToSubtract,
                onSuccess: (newBalance) =>
                {
                    currentHardCurrency = newBalance;
                    UpdateUIText();
                    OnHardCurrencyChanged?.Invoke(newBalance);
                    
                    Debug.Log($"Списано {amountToSubtract} жёсткой валюты. Новый баланс: {newBalance}");
                },
                onError: (error) =>
                {
                    Debug.LogError($"Ошибка списания жёсткой валюты: {error}");
                }
            );
            
            return true;
        }
        
        /// <summary>
        /// Получить текущий баланс мягкой валюты
        /// </summary>
        public int GetSoftCurrencyBalance()
        {
            return currentSoftCurrency;
        }
        
        /// <summary>
        /// Получить текущий баланс жёсткой валюты
        /// </summary>
        public int GetHardCurrencyBalance()
        {
            return currentHardCurrency;
        }
        
        /// <summary>
        /// Проверить, достаточно ли мягкой валюты
        /// </summary>
        public bool HasEnoughSoftCurrency(int amount)
        {
            return currentSoftCurrency >= amount;
        }
        
        /// <summary>
        /// Проверить, достаточно ли жёсткой валюты
        /// </summary>
        public bool HasEnoughHardCurrency(int amount)
        {
            return currentHardCurrency >= amount;
        }
        
        /// <summary>
        /// Получить AdManager (с поиском если не назначен)
        /// </summary>
        private AdManager GetAdManager()
        {   
            return AdManager.instance;
        }
        
        /// <summary>
        /// Обработчик обновления валюты из PlayFabCurrency
        /// </summary>
        private void OnCurrencyUpdated(string currencyCode, int newBalance)
        {
            if (currencyCode == "SC")
            {
                currentSoftCurrency = newBalance;
                if (softCurrencyText != null)
                    softCurrencyText.text = currentSoftCurrency.ToString();
            }
            else if (currencyCode == "HC")
            {
                currentHardCurrency = newBalance;
                if (hardCurrencyText != null)
                    hardCurrencyText.text = currentHardCurrency.ToString();
            }
        }
        
        /// <summary>
        /// Принудительно синхронизировать валюту с сервером
        /// </summary>
        public void SyncCurrency()
        {
            RefreshCurrencyDisplay();
        }

        public void SubtractSoftCurrency(int amount, Action<bool> onComplete = null)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Нельзя списать отрицательное или нулевое количество валюты");
                onComplete?.Invoke(false);
                return;
            }

            if (currentSoftCurrency < amount)
            {
                Debug.Log($"Недостаточно мягкой валюты: {currentSoftCurrency} < {amount}");
                onComplete?.Invoke(false);
                return;
            }

            PlayFabCurrency.SafeSubtractCurrency("SC", amount,
                onSuccess: (newBalance) =>
                {
                    currentSoftCurrency = newBalance;
                    UpdateUIText();
                    OnSoftCurrencyChanged?.Invoke(newBalance);
                    Debug.Log($"Списано {amount} мягкой валюты. Новый баланс: {newBalance}");
                    onComplete?.Invoke(true);
                },
                onError: (error) =>
                {
                    Debug.LogError($"Ошибка списания мягкой валюты: {error}");
                    onComplete?.Invoke(false);
                }
            );
        }

        public void AddHardCurrency(int amount, Action<bool> onComplete = null)
        {
            if (amount <= 0)
            {
                Debug.LogWarning("Нельзя добавить отрицательное или нулевое количество валюты");
                onComplete?.Invoke(false);
                return;
            }

            PlayFabCurrency.AddCurrency("HC", amount,
                onSuccess: (newBalance) =>
                {
                    currentHardCurrency = newBalance;
                    UpdateUIText();
                    OnHardCurrencyChanged?.Invoke(newBalance);
                    Debug.Log($"Добавлено {amount} жёсткой валюты. Новый баланс: {newBalance}");
                    onComplete?.Invoke(true);
                },
                onError: (error) =>
                {
                    Debug.LogError($"Ошибка добавления жёсткой валюты: {error}");
                    onComplete?.Invoke(false);
                }
            );
        }

        // Новый метод конвертации
        public void ConvertSoftToHard(int hardAmount, Action<bool> onComplete = null)
        {
            int requiredSoft = hardAmount * softToHardRate;
            if (currentSoftCurrency < requiredSoft)
            {
                Debug.Log($"Недостаточно мягкой валюты для покупки {hardAmount} HC. Нужно: {requiredSoft}, есть: {currentSoftCurrency}");
                onComplete?.Invoke(false);
                return;
            }

            // Сначала списываем мягкую валюту
            SubtractSoftCurrency(requiredSoft, (softSuccess) =>
            {
                if (softSuccess)
                {
                    // Затем добавляем жёсткую
                    AddHardCurrency(hardAmount, (hardSuccess) =>
                    {
                        if (hardSuccess)
                        {
                            Debug.Log($"Конвертация успешна: {hardAmount} HC за {requiredSoft} SC");
                            onComplete?.Invoke(true);
                        }
                        else
                        {
                            Debug.LogError("Не удалось добавить жёсткую валюту после списания мягкой! Возможна потеря средств.");
                            onComplete?.Invoke(false);
                        }
                    });
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
        }
    }
}