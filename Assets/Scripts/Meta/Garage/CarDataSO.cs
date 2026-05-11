using UnityEngine;
using System.Collections.Generic;

namespace Garage
{
    [CreateAssetMenu(fileName = "NewCarData", menuName = "Garage/Car Data")]
    public class CarDataSO : ScriptableObject
    {
        [Header("Основная информация")]
        public int carId;
        public string carName;
        public string description;
        public int priceSoft;      // Цена за мягкую валюту
        public int priceHard;      // Цена за жёсткую валюту
        
        [Header("Визуал")]
        public GameObject carPrefab;
        public Sprite carIcon;
        public Material[] availableColors;
        public Color previewColor = Color.white;
        
        [Header("Характеристики (базовые)")]
        public float baseStartSpeed = 5f;
        public float baseMaxSpeed = 15f;
        public float baseAcceleration = 0.5f;
        public int baseHealth = 3;
    }
}