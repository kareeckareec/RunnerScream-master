using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class LeaderboardManager : MonoBehaviour
{
    private static LeaderboardManager instance;
    public static LeaderboardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<LeaderboardManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("LeaderboardManager");
                    instance = obj.AddComponent<LeaderboardManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    private LeaderboardData leaderboardData;
    private string savePath;
    private bool isInitialized = false;

    [Header("Настройки")]
    public int maxEntries = 10;
    public string defaultPlayerName = "Игрок";

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");
        
        // Отложенная инициализация – не загружаем данные сразу,
        // чтобы избежать проблем с ещё не готовым UserData
        // LoadLeaderboard() будет вызван при первом обращении
    }

    /// <summary>
    /// Гарантирует, что данные загружены (ленивая инициализация)
    /// </summary>
    private void EnsureInitialized()
    {
        if (isInitialized) return;
        
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                leaderboardData = JsonUtility.FromJson<LeaderboardData>(json);
                if (leaderboardData == null)
                    leaderboardData = new LeaderboardData();
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка загрузки таблицы лидеров: {e.Message}");
                leaderboardData = new LeaderboardData();
            }
        }
        else
        {
            leaderboardData = new LeaderboardData();
            // Не добавляем тестовые данные автоматически – только по необходимости
        }
        
        isInitialized = true;
    }

    /// <summary>
    /// Добавляет очки в таблицу и проверяет, побит ли личный рекорд.
    /// </summary>
    /// <param name="currentScore">Текущий счёт игрока</param>
    /// <returns>true, если установлен новый личный рекорд</returns>
    public bool TryAddScoreAndCheckRecord(int currentScore)
    {
        EnsureInitialized();
        
        string playerName = DataBase.UserData.NickName;
        if (string.IsNullOrEmpty(playerName))
            playerName = defaultPlayerName;

        int bestScore = GetPlayerBestScore(playerName);
        bool isNewRecord = currentScore > bestScore;

        AddScore(currentScore, playerName);

        if (isNewRecord)
        {
            DataBase.UserData.Record = currentScore;
            DataBase.UserData.UpdateData();
        }

        return isNewRecord;
    }

    public void AddScore(int score, string playerName = null)
    {
        EnsureInitialized();
        
        if (score < 0) score = 0;
        if (string.IsNullOrEmpty(playerName))
            playerName = defaultPlayerName;

        ScoreEntry newEntry = new ScoreEntry(playerName, score);
        leaderboardData.AddScore(newEntry);

        if (leaderboardData.scores.Count > maxEntries)
            leaderboardData.scores.RemoveRange(maxEntries, leaderboardData.scores.Count - maxEntries);

        SaveLeaderboard();
    }

    public List<ScoreEntry> GetTopScores()
    {
        EnsureInitialized();
        return leaderboardData.GetTopScores(maxEntries);
    }

    public void ClearLeaderboard()
    {
        EnsureInitialized();
        leaderboardData = new LeaderboardData();
        SaveLeaderboard();
    }

    private int GetPlayerBestScore(string playerName)
    {
        int best = 0;
        foreach (var entry in leaderboardData.scores)
        {
            if (entry.playerName == playerName && entry.score > best)
                best = entry.score;
        }
        return best;
    }

    private void SaveLeaderboard()
    {
        try
        {
            string json = JsonUtility.ToJson(leaderboardData, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка сохранения таблицы лидеров: {e.Message}");
        }
    }
}