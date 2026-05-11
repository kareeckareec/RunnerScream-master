using UnityEngine;
using System.Collections.Generic;
using DataBase;
using Meta;

namespace Garage
{
    public class GarageManager : MonoBehaviour
    {
        public static GarageManager Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private Transform carPreviewPoint;
        [SerializeField] private Camera previewCamera;
        [SerializeField] private GarageUI garageUI;
        
        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 2f;
        [SerializeField] private float cameraTransitionTime = 0.5f;
        
        private List<CarDataSO> allCars;               // теперь хранятся ВСЕ машины
        private int currentCarIndex = 0;
        private GameObject currentCarPreview;
        private CarDataSO currentCarData;
        
        public System.Action<CarDataSO> OnCarChanged;
        public System.Action<CarDataSO> OnCarPurchased;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            LoadAllCars();
            LoadSelectedCar();
            UpdateUI();
        }
        
        // Загружаем ВСЕ машины из базы (и разблокированные, и заблокированные)
        private void LoadAllCars()
        {
            if (CarsDatabase.Instance == null)
            {
                Debug.LogError("CarsDatabase.Instance is null!");
                return;
            }
            allCars = CarsDatabase.Instance.GetAllCars();
            
            if (allCars.Count == 0)
            {
                Debug.LogError("В базе нет ни одной машины!");
                allCars = new List<CarDataSO> { CarsDatabase.Instance.GetCarById(0) };
            }
        }
        
        private void LoadSelectedCar()
        {
            int selectedId = DataBase.UserData.SelectedCar;
            int foundIndex = -1;
            
            for (int i = 0; i < allCars.Count; i++)
            {
                if (allCars[i].carId == selectedId)
                {
                    foundIndex = i;
                    break;
                }
            }
            
            if (foundIndex >= 0)
            {
                currentCarIndex = foundIndex;
                currentCarData = allCars[currentCarIndex];
            }
            else
            {
                // Выбранная машина не найдена в общем списке — фиксим выбор на первой доступной
                currentCarIndex = 0;
                currentCarData = allCars[0];
                DataBase.UserData.SelectedCar = currentCarData.carId;
                DataBase.UserData.UpdateData();
                Debug.LogWarning($"Машина с ID {selectedId} не найдена. Автоматически выбрана {currentCarData.carName} (ID {currentCarData.carId})");
            }
            
            SpawnCarPreview();
        }
        
        private void SpawnCarPreview()
        {
            if (currentCarPreview != null)
                Destroy(currentCarPreview);
            
            if (currentCarData.carPrefab != null)
            {
                currentCarPreview = Instantiate(currentCarData.carPrefab, carPreviewPoint.position, carPreviewPoint.rotation);
                currentCarPreview.transform.SetParent(carPreviewPoint);
                currentCarPreview.transform.localPosition = Vector3.zero;
                currentCarPreview.transform.localRotation = Quaternion.identity;
            }
        }
        
        public void PreviousCar()
        {
            currentCarIndex--;
            if (currentCarIndex < 0)
                currentCarIndex = allCars.Count - 1;
            ChangeCar();
        }
        
        public void NextCar()
        {
            currentCarIndex++;
            if (currentCarIndex >= allCars.Count)
                currentCarIndex = 0;
            ChangeCar();
        }
        
        private void ChangeCar()
        {
            currentCarData = allCars[currentCarIndex];
            SpawnCarPreview();
            UpdateUI();
            OnCarChanged?.Invoke(currentCarData);
        }
        
        public void SelectCurrentCar()
        {
            // Нельзя выбрать заблокированную машину
            if (!IsCurrentCarUnlocked())
            {
                Debug.Log("Машина ещё не куплена!");
                return;
            }
            
            DataBase.UserData.SelectedCar = currentCarData.carId;
            DataBase.UserData.UpdateData();
            Debug.Log($"Выбрана машина: {currentCarData.carName}");
            UpdateUI();
        }
        
        public void PurchaseCurrentCar()
        {
            var carData = DataBase.UserData.GetCarData(currentCarData.carId);
            if (carData.isUnlocked) return;
            
            if (currentCarData.priceSoft <= 0 && currentCarData.priceHard <= 0)
            {
                UnlockCurrentCar();
                return;
            }
            
            bool canBuySoft = MenuCurrencyUtils.Instance != null && 
                              MenuCurrencyUtils.Instance.HasEnoughSoftCurrency(currentCarData.priceSoft);
            bool canBuyHard = MenuCurrencyUtils.Instance != null && 
                              MenuCurrencyUtils.Instance.HasEnoughHardCurrency(currentCarData.priceHard);
            
            if (currentCarData.priceSoft > 0 && canBuySoft)
            {
                if (MenuCurrencyUtils.Instance.SubtractSoftCurrency(currentCarData.priceSoft))
                    UnlockCurrentCar();
                else
                    UpdateUI();
            }
            else if (currentCarData.priceHard > 0 && canBuyHard)
            {
                if (MenuCurrencyUtils.Instance.SubtractHardCurrency(currentCarData.priceHard))
                    UnlockCurrentCar();
                else
                    UpdateUI();
            }
            else
            {
                Debug.Log("Недостаточно средств для покупки машины");
                UpdateUI();
            }
        }
        
        private void UnlockCurrentCar()
        {
            var carData = DataBase.UserData.GetCarData(currentCarData.carId);
            carData.isUnlocked = true;
            DataBase.UserData.SetCarData(currentCarData.carId, carData);
            DataBase.UserData.UpdateData();
            
            // Автовыбор купленной машины
            DataBase.UserData.SelectedCar = currentCarData.carId;
            DataBase.UserData.UpdateData();
            
            // Список allCars уже содержит все машины, перезагружать не нужно
            // Просто обновляем UI
            OnCarPurchased?.Invoke(currentCarData);
            UpdateUI();
        }
        
        private void UpdateUI()
        {
            if (garageUI != null)
                garageUI.UpdateUI(currentCarData, IsCurrentCarUnlocked(), IsCurrentCarSelected());
        }
        
        private bool IsCurrentCarUnlocked()
        {
            var carData = DataBase.UserData.GetCarData(currentCarData.carId);
            return carData.isUnlocked;
        }
        
        private bool IsCurrentCarSelected()
        {
            return DataBase.UserData.SelectedCar == currentCarData.carId;
        }
        
        public CarDataSO GetCurrentCarData() => currentCarData;
        
        // Возвращаем ВСЕ машины (для навигации и UI)
        public List<CarDataSO> GetAllCars() => new List<CarDataSO>(allCars);
        
        public int GetCurrentCarIndex() => currentCarIndex;
        
        public void RefreshGarage()
        {
            LoadAllCars();
            LoadSelectedCar();
            UpdateUI();
        }
    }
}