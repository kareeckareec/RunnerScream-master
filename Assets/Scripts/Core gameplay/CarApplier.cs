using System.Collections;
using UnityEngine;
using Garage;
using DataBase;

namespace CoreGameplay
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerHealth))]
    public class CarApplier : MonoBehaviour
    {
        [Tooltip("Родительский объект, куда будет помещена 3D-модель машины")]
        [SerializeField] private Transform modelParent;
        
        private GameObject currentModel;

        private void Start()
        {
            // Если данные ещё не загружены, подписываемся на событие
            if (DataBase.UserData.IsDataLoaded)
                ApplySelectedCar();
            else
                DataBase.UserData.OnDataLoaded += OnDataLoaded;
        }

        private void OnDataLoaded()
        {
            DataBase.UserData.OnDataLoaded -= OnDataLoaded;
            ApplySelectedCar();
        }

        public void ApplySelectedCar()
        {
            if (!UserData.IsDataLoaded)
            {
                UserData.OnDataLoaded += () => { ApplySelectedCar(); UserData.OnDataLoaded -= ApplySelectedCar; };
                return;
            }

            int selectedId = DataBase.UserData.SelectedCar;
            CarDataSO carData = CarsDatabase.Instance.GetCarById(selectedId);
            if (carData == null)
            {
                Debug.LogWarning($"Машина с ID {selectedId} не найдена, используем дефолтную");
                carData = CarsDatabase.Instance.GetCarById(0);
                if (carData == null) return;
            }

            // Применяем характеристики
            PlayerMovement movement = GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.SetBaseStats(carData.baseStartSpeed, carData.baseMaxSpeed, carData.baseAcceleration);
            }

            // Заменяем визуальную модель
            if (modelParent != null && carData.carPrefab != null)
            {
                if (currentModel != null) Destroy(currentModel);
                currentModel = Instantiate(carData.carPrefab, modelParent.position, modelParent.rotation, modelParent);
            }
            else
            {
                Debug.LogWarning("modelParent или carPrefab не заданы");
            }
        }
    }
}