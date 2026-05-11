using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataBase
{
    /// <summary>
    /// Доступ через статические члены к данным игрока
    /// </summary>
    public class UserData : MonoBehaviour
    {
        [SerializeField] private bool clearDataOnNextGet = false;
        [SerializeField] private int[] expToReachNextLevel;
        [SerializeField] private int[] rewards;


        /// <summary>
        /// Ник игрока.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static string NickName
        {
            get => data.nickName;
            set => data.nickName = value;
        }

        /// <summary>
        /// Количество опыта игрока.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static int Exp
        {
            get => data.exp;
            set => data.exp = value;
        }

        /// <summary>
        /// Максимальное количество опыта игрока, после набора которого уровень повысится.
        /// </summary>
        public static int MaxExp
        {
            get
            {
                if (Level >= Instance.expToReachNextLevel.Length) return int.MaxValue;

                return Instance.expToReachNextLevel[Level];
            }
        }

        /// <summary>
        /// Уровень игрока.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static int Level
        {
            get => data.level;
            set => data.level = value;
        }

        /// <summary>
        /// Количество софт валюты игрока.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static int SoftMoney
        {
            get => data.softMoney;
            set => data.softMoney = value;
        }

        /// <summary>
        /// Количество хард валюты игрока.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static int HardMoney
        {
            get => data.hardMoney;
            set => data.hardMoney = value;
        }

        /// <summary>
        /// Рекорд игрока по очкам.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static int Record
        {
            get => data.record;
            set => data.record = value;
        }

        /// <summary>
        /// Выбранная машина.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static int SelectedCar
        {
            get => data.selectedCar;
            set => data.selectedCar = value;
        }


        /// <summary>
        /// Получить количество расходника по его номеру.
        /// </summary>
        public static int GetDisposableItemCount(int id)
            => data.disposableItemsCounts[id];

        /// <summary>
        /// Установить количество расходника по его номеру.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static void SetDisposableItemCount(int id, int count)
        {
            while (data.disposableItemsCounts.Count <= id) data.disposableItemsCounts.Add(0);
            data.disposableItemsCounts[id] = count;
        }


        /// <summary>
        /// Получить данные о машине по её номеру.
        /// </summary>
        public static CarData GetCarData(int id)
            => data.cars[id];

        /// <summary>
        /// Установить данные о машине по её номеру.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static void SetCarData(int id, CarData car)
        {
            while (data.cars.Count <= id) data.cars.Add(new CarData());
            data.cars[id] = car;
        }


        /// <summary>
        /// Получить данные о скилле по его номеру.
        /// </summary>
        public static SkillData GetSkillData(int id)
            => data.skills[id];

        /// <summary>
        /// Установить данные о скилле по его номеру.
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static void SetSkillData(int id, SkillData skill)
        {
            while (data.skills.Count <= id) data.skills.Add(new SkillData());
            data.skills[id] = skill;
        }


        /// <summary>
        /// Включено ли обучение при заходе в игру?
        /// После измерения значений вызвать <see cref="UpdateData()"/>
        /// </summary>
        public static bool Teaching
        {
            get => data.needsTeaching;
            set => data.needsTeaching = value;
        }

        public static int GetLevelReward(int level)
        {
            return Instance.rewards[level - 1];
        }


        private static Data data = DefaultData;
        private static Data DefaultData => new Data()
        {
            nickName = "Nickname",
            level = 0,
            exp = 0,
            softMoney = 1000,
            hardMoney = 10,
            record = 0,
            needsTeaching = true,
            disposableItemsCounts =
            {
                5, 5, 5, 5
            },
            cars =
            {
                new CarData() { isUnlocked = true },
                new CarData(),
                new CarData(),
                new CarData(),
                new CarData() {},
                new CarData(),
                new CarData(),
                new CarData()
            },
            skills = new List<SkillData>
            {
                new SkillData(true),
                new SkillData(false)
            }
        };
        [System.Serializable]
        private class Data
        {
            public string nickName;
            public int exp;
            public int level;
            public int softMoney;
            public int hardMoney;
            public int record;
            public bool needsTeaching;
            public List<int> disposableItemsCounts = new List<int>();

            public List<SkillData> skills = new List<SkillData>();

            public int selectedCar;
            public List<CarData> cars = new List<CarData>();


            public string ToJson() => JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Структура с информацией о скилле.
        /// </summary>
        [System.Serializable]
        public struct SkillData
        {
            public bool isUnlocked;

            public SkillData(bool isUnlocked)
            {
                this.isUnlocked = isUnlocked;
            }
        }

        /// <summary>
        /// Структура с информацией о машине.
        /// </summary>
        [System.Serializable]
        public class CarData
        {
            public bool isUnlocked;
            public bool isBuyingUnlocked;
            public int selectedSkill;

            public int color;
            public List<int> colors;

            public int selectedPassiveAbility;
            public List<int> passiveAbilitiesLevels = new List<int>();
            public List<int> passiveAbilitiesSteps = new List<int>();
            public List<float> passiveAbilitiesProgress = new List<float>();
            public List<int> passiveAbilitiesCosts = new List<int>();

            public CarData()
            {
                Init();
            }

            public void Init()
            {
                while (passiveAbilitiesLevels.Count < 5) passiveAbilitiesLevels.Add(0);
                while (passiveAbilitiesProgress.Count < 5) passiveAbilitiesProgress.Add(0);
                while (passiveAbilitiesSteps.Count < 5) passiveAbilitiesSteps.Add(0);
                while (passiveAbilitiesCosts.Count < 5) passiveAbilitiesCosts.Add(0);
            }
        }


        private static UserData Instance { get; set; }

        private static System.Action onGetDataResult = null;


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static void GetData(System.Action onResult = null)
        {
            onGetDataResult = onResult;

            if (!Instance.clearDataOnNextGet)
            {
                GetUserDataRequest request = new GetUserDataRequest() { Keys = new List<string>() { "Data" } };
                PlayFabClientAPI.GetUserData(request, OnDataGetted, HandleError);
            }
            else
            {
                Instance.clearDataOnNextGet = false;
                UpdateData();
                onGetDataResult?.Invoke();
            }
        }

        private static void OnDataGetted(GetUserDataResult result)
        {
            if (result.Data.ContainsKey("Data")) JsonUtility.FromJsonOverwrite(result.Data["Data"].Value, data);
            else UpdateData();
            foreach (var car in data.cars) car.Init();
            onGetDataResult?.Invoke();
        }

        private static void HandleError(PlayFabError error)
        {
            Debug.LogError("PlayFab Error: \n" + error.GenerateErrorReport());
        }


        /// <summary>
        /// Стирает все данные!
        /// </summary>
        public static void ClearData()
        {
            data = DefaultData;
            UpdateData();
        }


        /// <summary>
        /// Отправить локальные данные на сервер.
        /// Нельзя часто вызывать, поэтому рекомендуется вызывать после изменения всех необходимых данных.
        /// </summary>
        public static void UpdateData()
        {
            if (!PlayFabAuthenticationAPI.IsEntityLoggedIn()) return;

            UpdateUserDataRequest request = new UpdateUserDataRequest() { Data = new Dictionary<string, string>() { { "Data", data.ToJson() } } };
            PlayFabClientAPI.UpdateUserData(request, OnDataUpdated, HandleError);
        }

        private static void OnDataUpdated(UpdateUserDataResult result)
        {
            Debug.Log("Data updated");
        }


        #region Experemental
        private class DataSet<T>
        {
            private T value;
            public T Value
            {
                get => value;
                set
                {
                    this.value = value;
                    UpdateUserDataRequest request = new UpdateUserDataRequest() { Data = new Dictionary<string, string>() { { Key, JsonUtility.ToJson(value) } } };
                    PlayFabClientAPI.UpdateUserData(request, OnDataUpdated, HandleError);
                }
            }

            public string Key { get; set; }

            private T defaultValue;

            public DataSet(string key, T defaultValue = default)
            {
                Key = key;
                this.defaultValue = defaultValue;

                GetUserDataRequest request = new GetUserDataRequest() { Keys = new List<string>() { Key } };
                PlayFabClientAPI.GetUserData(request, OnValueGetted, HandleError);
            }

            private void OnValueGetted(GetUserDataResult result)
            {
                var str = result.Data[Key].Value;
                if (string.IsNullOrEmpty(str)) Value = defaultValue;
                else value = JsonUtility.FromJson<T>(str);
            }
        }

        //private static DataSet<int> expData = new DataSet<int>("exp");
        ///// <summary>
        ///// Количество опыта игрока.
        ///// После измерения значений вызвать 'UpdateData'!
        ///// </summary>
        //public static int Exp
        //{
        //    get => expData.Value;
        //    set => expData.Value = value;
        //}

        //private static DataSet<int> levelData = new DataSet<int>("level");
        ///// <summary>
        ///// Уровень игрока.
        ///// После измерения значений вызвать 'UpdateData'!
        ///// </summary>
        //public static int Level
        //{
        //    get => levelData.Value;
        //    set => levelData.Value = value;
        //}

        //private static DataSet<int> moneyData = new DataSet<int>("money");
        ///// <summary>
        ///// Количество денег игрока.
        ///// После измерения значений вызвать 'UpdateData'!
        ///// </summary>
        //public static int Money
        //{
        //    get => moneyData.Value;
        //    set => moneyData.Value = value;
        //}
        #endregion
    }
}