using UnityEngine;
using Garage;

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
            ApplySelectedCar();
        }

        public void ApplySelectedCar()
        {
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
                // При необходимости настроить слои для камеры
            }
            else
            {
                Debug.LogWarning("modelParent или carPrefab не заданы");
            }
        }
    }
}