using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System;

namespace Currency
{
    public class PlayFabCurrency : MonoBehaviour
    {
        public static event Action<string, int> OnCurrencyUpdated;
        public static event Action<string> OnCurrencyError;

        // Получить баланс валют
        public static void GetCurrencyBalance(Action<int, int> onSuccess = null, Action<string> onError = null)
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                result =>
                {
                    int softBalance = 0;
                    int hardBalance = 0;
                    
                    foreach (var currency in result.VirtualCurrency)
                    {
                        Debug.Log($"Валюта {currency.Key}: {currency.Value}");
                        
                        if (currency.Key == "SC")
                            softBalance = currency.Value;
                        else if (currency.Key == "HC")
                            hardBalance = currency.Value;
                    }
                    
                    onSuccess?.Invoke(softBalance, hardBalance);
                },
                error => 
                {
                    string errorMsg = error.GenerateErrorReport();
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            );
        }

        public static void AddCurrency(string currencyCode, int amount, 
            Action<int> onSuccess = null, 
            Action<string> onError = null)
        {
            var request = new AddUserVirtualCurrencyRequest
            {
                VirtualCurrency = currencyCode,
                Amount = amount
            };

            PlayFabClientAPI.AddUserVirtualCurrency(request,
                result => 
                { 
                    Debug.Log($"Добавлено {amount} {currencyCode}. Новый баланс: {result.Balance}");
                    OnCurrencyUpdated?.Invoke(currencyCode, result.Balance);
                    onSuccess?.Invoke(result.Balance);
                },
                error => 
                {
                    string errorMsg = error.GenerateErrorReport();
                    Debug.LogError(errorMsg);
                    OnCurrencyError?.Invoke(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            );
        }
        
        // 1. Метод для проверки и списания валюты
        public static void SafeSubtractCurrency(string currencyCode, int amountToSubtract,
            Action<int> onSuccess = null,
            Action<string> onError = null)
        {
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                getInventoryResult => 
                {
                    if (getInventoryResult.VirtualCurrency.TryGetValue(currencyCode, out int currentBalance))
                    {
                        if (currentBalance >= amountToSubtract)
                        {
                            SubtractCurrency(currencyCode, amountToSubtract, onSuccess, onError);
                        }
                        else
                        {
                            string errorMsg = $"Недостаточно средств. Текущий баланс {currencyCode}: {currentBalance}, требуется: {amountToSubtract}";
                            Debug.LogError(errorMsg);
                            onError?.Invoke(errorMsg);
                        }
                    }
                    else
                    {
                        string errorMsg = $"Валюта {currencyCode} не найдена в инвентаре игрока.";
                        Debug.LogError(errorMsg);
                        onError?.Invoke(errorMsg);
                    }
                },
                error => 
                {
                    string errorMsg = "Ошибка при получении инвентаря: " + error.GenerateErrorReport();
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            );
        }

        // 2. Метод для непосредственного списания валюты
        private static void SubtractCurrency(string currencyCode, int amount,
            Action<int> onSuccess = null,
            Action<string> onError = null)
        {
            var subtractRequest = new SubtractUserVirtualCurrencyRequest
            {
                VirtualCurrency = currencyCode,
                Amount = amount
            };

            PlayFabClientAPI.SubtractUserVirtualCurrency(subtractRequest,
                subtractResult => 
                {
                    Debug.Log($"Списание успешно. Новый баланс {currencyCode}: {subtractResult.Balance}");
                    OnCurrencyUpdated?.Invoke(currencyCode, subtractResult.Balance);
                    onSuccess?.Invoke(subtractResult.Balance);
                },
                subtractError => 
                {
                    string errorMsg = "Ошибка при списании валюты: " + subtractError.GenerateErrorReport();
                    Debug.LogError(errorMsg);
                    onError?.Invoke(errorMsg);
                }
            );
        }
    }
}