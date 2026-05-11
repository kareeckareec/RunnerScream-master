using UnityEngine;
using System.Collections.Generic;

namespace Garage
{
    public class CarsDatabase : MonoBehaviour
    {
        public static CarsDatabase Instance { get; private set; }
        
        [SerializeField] private List<CarDataSO> allCars;
        [SerializeField] private CarDataSO defaultCar;
        
        private Dictionary<int, CarDataSO> carsDict;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDictionary();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeDictionary()
        {
            carsDict = new Dictionary<int, CarDataSO>();
            if (allCars == null) return;
            
            foreach (var car in allCars)
            {
                if (car != null && !carsDict.ContainsKey(car.carId))
                    carsDict.Add(car.carId, car);
            }
        }
        
        public CarDataSO GetCarById(int id)
        {
            if (carsDict == null) InitializeDictionary();
            return carsDict.TryGetValue(id, out CarDataSO car) ? car : defaultCar;
        }
        
        public List<CarDataSO> GetAllCars()
        {
            return allCars != null ? new List<CarDataSO>(allCars) : new List<CarDataSO>();
        }
        
        public List<CarDataSO> GetLockedCars()
        {
            List<CarDataSO> locked = new List<CarDataSO>();
            if (allCars == null) return locked;
            
            foreach (var car in allCars)
            {
                if (car == null) continue;
                var carData = DataBase.UserData.GetCarData(car.carId);
                if (!carData.isUnlocked)
                    locked.Add(car);
            }
            return locked;
        }
        
        public List<CarDataSO> GetUnlockedCars()
        {
            List<CarDataSO> unlocked = new List<CarDataSO>();
            if (allCars == null) return unlocked;
            
            foreach (var car in allCars)
            {
                if (car == null) continue;
                var carData = DataBase.UserData.GetCarData(car.carId);
                if (carData.isUnlocked)
                    unlocked.Add(car);
            }
            return unlocked;
        }
    }
}