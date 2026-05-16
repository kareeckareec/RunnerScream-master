using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using Meta;

namespace Garage
{
    public class GarageUI : MonoBehaviour
    {
        [Header("Main UI")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject garagePanel;
        [SerializeField] private TextMeshProUGUI carNameText;
        [SerializeField] private TextMeshProUGUI carDescriptionText;
        
        [Header("Car Stats")]
        [SerializeField] private TextMeshProUGUI speedValueText;
        [SerializeField] private TextMeshProUGUI accelerationValueText;
        [SerializeField] private TextMeshProUGUI healthValueText;
        
        [Header("Buttons")]
        [SerializeField] private Button purchaseButton;   // кнопка "Купить"
        [SerializeField] private Button selectButton;     // кнопка "Выбрать"
        [SerializeField] private TextMeshProUGUI priceText;
        
        [Header("Navigation")]
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;


        private void Start()
        {
            if (GarageManager.Instance == null)
            {
                Debug.LogError("GarageManager не найден!");
                return;
            }
            
            GarageManager.Instance.OnCarChanged += OnCarChangedHandler;
            GarageManager.Instance.OnCarPurchased += OnCarPurchasedHandler;
            
            if (prevButton != null)
                prevButton.onClick.AddListener(() => GarageManager.Instance.PreviousCar());
            if (nextButton != null)
                nextButton.onClick.AddListener(() => GarageManager.Instance.NextCar());
            if (selectButton != null)
                selectButton.onClick.AddListener(() => GarageManager.Instance.SelectCurrentCar());
            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(() => GarageManager.Instance.PurchaseCurrentCar());
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseGarage);
        }

        [Header("Swipe Settings (New Input System)")]
        [SerializeField] private float minSwipeDistance = 50f;
        [SerializeField] private float maxVerticalDeviation = 30f; // макс. вертикальное отклонение для горизонтального свайпа

        private Vector2 touchStartPos;
        private bool isSwiping = false;
        private int activeTouchId = -1;

        private void Update()
        {
            HandleSwipeWithInputSystem();
        }

        private void HandleSwipeWithInputSystem()
        {
            // Проверяем наличие сенсорного экрана (мобильное устройство)
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var touches = Touchscreen.current.touches;
                // Берём первый активный тач
                var touch = touches[0];
                
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    // Проверяем, не попали ли по UI
                    if (IsPointerOverUI(touch.touchId.ReadValue()))
                    {
                        isSwiping = false;
                        return;
                    }
                    touchStartPos = touch.position.ReadValue();
                    isSwiping = true;
                    activeTouchId = touch.touchId.ReadValue();
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended && isSwiping && touch.touchId.ReadValue() == activeTouchId)
                {
                    Vector2 delta = touch.position.ReadValue() - touchStartPos;
                    if (Mathf.Abs(delta.x) > minSwipeDistance && Mathf.Abs(delta.y) < maxVerticalDeviation)
                    {
                        if (delta.x > 0)
                            GarageManager.Instance?.PreviousCar();
                        else
                            GarageManager.Instance?.NextCar();
                    }
                    isSwiping = false;
                    activeTouchId = -1;
                }
            }
            // Поддержка мыши в редакторе (опционально)
            else if (Mouse.current != null)
            {
                var mouse = Mouse.current;
                if (mouse.leftButton.wasPressedThisFrame)
                {
                    if (IsPointerOverUI())
                    {
                        isSwiping = false;
                        return;
                    }
                    touchStartPos = mouse.position.ReadValue();
                    isSwiping = true;
                }
                else if (mouse.leftButton.wasReleasedThisFrame && isSwiping)
                {
                    Vector2 delta = mouse.position.ReadValue() - touchStartPos;
                    if (Mathf.Abs(delta.x) > minSwipeDistance && Mathf.Abs(delta.y) < maxVerticalDeviation)
                    {
                        if (delta.x > 0)
                            GarageManager.Instance?.PreviousCar();
                        else
                            GarageManager.Instance?.NextCar();
                    }
                    isSwiping = false;
                }
            }
        }

        private bool IsPointerOverUI(int touchId = -1)
        {
            if (EventSystem.current == null) return false;
            
            if (touchId >= 0)
                return EventSystem.current.IsPointerOverGameObject(touchId);
            else
                return EventSystem.current.IsPointerOverGameObject();
        }
        
        private void OnDestroy()
        {
            if (GarageManager.Instance != null)
            {
                GarageManager.Instance.OnCarChanged -= OnCarChangedHandler;
                GarageManager.Instance.OnCarPurchased -= OnCarPurchasedHandler;
            }
        }
        
        private void OnCarChangedHandler(CarDataSO carData)
        {
            UpdateUI(carData, IsCarUnlocked(carData), IsCarSelected(carData));
        }
        
        private void OnCarPurchasedHandler(CarDataSO carData)
        {
            if (MenuCurrencyUtils.Instance != null)
                MenuCurrencyUtils.Instance.RefreshCurrencyDisplay();
            
            UpdateUI(carData, true, IsCarSelected(carData));
        }
        
        public void OpenGarage()
        {
            garagePanel.SetActive(true);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            
            if (MenuCurrencyUtils.Instance != null)
                MenuCurrencyUtils.Instance.RefreshCurrencyDisplay();
            
            if (GarageManager.Instance != null)
                GarageManager.Instance.RefreshGarage();
        }
        
        public void CloseGarage()
        {
            garagePanel.SetActive(false);
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }
        
        public void UpdateUI(CarDataSO carData, bool isUnlocked, bool isSelected)
        {
            if (carData == null) return;
            
            if (carNameText != null) carNameText.text = carData.carName;
            if (carDescriptionText != null) carDescriptionText.text = carData.description;
            
            UpdateStats(carData);
            UpdateButtonsState(isUnlocked, isSelected);
            UpdatePriceText(carData, isUnlocked);
            UpdateNavigationButtons();
        }
        
        private void UpdateStats(CarDataSO carData)
        {
            if (carData == null) return;
            
            if (speedValueText != null)
                speedValueText.text = $"{carData.baseMaxSpeed:F0}";
            if (accelerationValueText != null)
                accelerationValueText.text = $"{carData.baseAcceleration:F1}";
            if (healthValueText != null)
                healthValueText.text = $"{carData.baseHealth}";
        }
        
        private void UpdateButtonsState(bool isUnlocked, bool isSelected)
        {
            // Кнопка "Купить" активна только если НЕ разблокирована
            if (purchaseButton != null)
                purchaseButton.interactable = !isUnlocked;
            
            // Кнопка "Выбрать" активна, если разблокирована И НЕ выбрана
            if (selectButton != null)
            {
                selectButton.interactable = isUnlocked && !isSelected;
                
                // Меняем текст на кнопке
                var btnText = selectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null)
                    btnText.text = isSelected ? "ВЫБРАНО" : "ВЫБРАТЬ";
            }
        }
        
        private void UpdatePriceText(CarDataSO carData, bool isUnlocked)
        {
            if (priceText == null) return;
            
            if (isUnlocked)
            {
                priceText.text = "Куплена";
                return;
            }
            
            if (carData.priceSoft <= 0 && carData.priceHard <= 0)
                priceText.text = "Бесплатно";
            else if (carData.priceSoft > 0 && carData.priceHard > 0)
                priceText.text = $"{carData.priceSoft} / {carData.priceHard}";
            else if (carData.priceSoft > 0)
                priceText.text = $"{carData.priceSoft} монет";
            else
                priceText.text = $"{carData.priceHard} кристаллов";
        }
        
        private void UpdateNavigationButtons()
        {
            if (GarageManager.Instance == null) return;
            
            var allCars = GarageManager.Instance.GetAllCars();
            bool hasMultiple = allCars.Count > 1;
            
            if (prevButton != null) prevButton.interactable = hasMultiple;
            if (nextButton != null) nextButton.interactable = hasMultiple;
        }
        
        private bool IsCarUnlocked(CarDataSO carData)
        {
            var data = DataBase.UserData.GetCarData(carData.carId);
            return data.isUnlocked;
        }
        
        private bool IsCarSelected(CarDataSO carData)
        {
            return DataBase.UserData.SelectedCar == carData.carId;
        }
    }
}