using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Ссылки UI")]
    public GameObject mainMenuPanel;
    public GameObject leaderboardPanel;
    public Transform entriesContainer;
    public GameObject entryPrefab;
    public Button closeButton;
    public Button refreshButton;
    public TMP_Text titleText;
    
    [Header("Информация об игроке")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerRecordText;

    [Header("Настройки")]
    public string leaderboardTitle = "ТАБЛИЦА ЛИДЕРОВ";
    public Color firstPlaceColor = Color.yellow;
    public Color secondPlaceColor = Color.gray;
    public Color thirdPlaceColor = new Color(0.8f, 0.45f, 0.2f); // Бронзовый
    public Color defaultColor = Color.white;
    
    private void Start()
    {

        if (titleText != null)
            titleText.text = leaderboardTitle;
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseLeaderboard);
            
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);
            
        leaderboardPanel.SetActive(false);
    }

    public void UpdatePlayerInfo()
    {
        if (playerNameText != null)
        {
            string nick = DataBase.UserData.NickName;
            if (string.IsNullOrEmpty(nick))
                nick = "Игрок";
            playerNameText.text = nick;
        }

        if (playerRecordText != null)
        {
            int record = DataBase.UserData.Record;
            playerRecordText.text = $"Рекорд: {record}";
        }
    }

    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        RefreshLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        // Очищаем контейнер
        foreach (Transform child in entriesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Получаем топ игроков
        List<ScoreEntry> topScores = LeaderboardManager.Instance.GetTopScores();
        
        if (topScores.Count == 0)
        {
            GameObject emptyEntry = Instantiate(entryPrefab, entriesContainer);
            Text entryText = emptyEntry.GetComponentInChildren<Text>();
            if (entryText != null)
                entryText.text = "Записей пока нет";
            return;
        }
        
        // Создаем записи
        for (int i = 0; i < topScores.Count; i++)
        {
            ScoreEntry entry = topScores[i];
            GameObject entryObj = Instantiate(entryPrefab, entriesContainer);
            
            // Устанавливаем текст
            TMP_Text entryText = entryObj.GetComponentInChildren<TMP_Text>();
            if (entryText != null)
            {
                string dateString = entry.date.ToString("dd.MM.yyyy");
                entryText.text = $"{entry.rank}. {entry.playerName}: {entry.score} очков";
            }
        }
    }
    public void CloseLeaderboard(){
        mainMenuPanel.SetActive(true);
        leaderboardPanel.SetActive(false);
    }
    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseLeaderboard);
        
        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(RefreshLeaderboard);
    }
}